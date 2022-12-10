using CommandLine.Text;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Web;

namespace berrywatch
{
    public class UploaderBase
    {
        private HttpClient wc = null;
        private Timer timer = null;
        private Task serverTask = null;
        private Dictionary<string, TaskCompletionSource> waitForTasksDict = new Dictionary<string, TaskCompletionSource>();

        [Option('u', "url", Required = true, HelpText = "Host local server on this url. Example: http://192.168.178.101:5001")]
        public string ServerUrl { get; set; }

        [Option('d', "device", Required = true, HelpText = "IP/Adress of tasmota device")]
        public string DeviceAddress { get; set; }

        [Option('f', "folder", Required = true, HelpText = "Folder to watch. Must be an absolute path.")]
        public string Folder { get; set; }

        [Option("filter", Required = false, HelpText = "Filter for files to watch", Default = "*.*")]
        public string Filter { get; set; }

        [Option("restart", Required = false, HelpText = "Restart device after each upload", Default = true)]
        public bool RestartAfterUpload { get; set; }

        [Option("restartAction", Required = false, HelpText = "Tasmota command to restart the app", Default = "BR load(\"autoexec.be\")")]
        public string RestartAction { get; set; }

        protected async Task Initialize()
        {
            this.wc = new HttpClient()
            {
                BaseAddress = new Uri($"http://{this.DeviceAddress}"),
                Timeout = TimeSpan.FromMilliseconds(4000)
            };

            await this.StartServer();
        }

        private void OnFileCompleted(string fileName)
        {
            //Console.WriteLine("File completed " + (fileName));
            if (waitForTasksDict.ContainsKey(fileName))
            {
                var tcs = waitForTasksDict[fileName];
                tcs.SetResult();
            }
        }

        protected Task WaitForServerTask(string filename)
        {
            lock (waitForTasksDict)
            {
                var tcs = new TaskCompletionSource();
                waitForTasksDict[filename] = tcs;
                return tcs.Task;
            }
        }

        protected void ClearWaitForServerTask(string filename)
        {
            lock (waitForTasksDict)
            {
                if (waitForTasksDict.ContainsKey(filename))
                    waitForTasksDict.Remove(filename);
            }
        }
        private async Task StartServer()
        {
            serverTask = Task.Run(() =>
            {
                try
                {
                    var srv = new Server();
                    srv.FileCompleted += OnFileCompleted;
                    srv.Run(this.ServerUrl, this.Folder);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            });
            while (!await this.CheckServerStarted())
            {
                await Task.Delay(500);
            }
        }
        protected async Task WaitForServer()
        {
            await serverTask;
        }

        private async Task<bool> CheckServerStarted()
        {
            var wc = new HttpClient();
            wc.BaseAddress = new Uri(this.ServerUrl);
            try
            {
                var result = await wc.GetAsync("/prj");
                return result.StatusCode == System.Net.HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
            finally
            {
                wc.Dispose();
            }
        }
        protected virtual async Task uploadAllFiles()
        {
            var all = Directory.GetFiles(this.Folder, this.Filter, SearchOption.TopDirectoryOnly);
            var restart = RestartAfterUpload;
            foreach (var file in all)
            {
                restart = await this.uploadFileAsync(file, restart);
            }
            if (restart)
                this.TriggerRestart();
        }

        private void TriggerRestart()
        {
            if (!this.RestartAfterUpload)
                return;
            if (this.timer != null)
            {
                this.timer.Dispose();
            }
            this.timer = new Timer((obj) => this.restarter(), null, 4000, Timeout.Infinite);
        }

        private void restarter()
        {
            this.timer.Dispose();
            this.timer = null;
            Task.Run(async () =>
            {
                try
                {
                    await this.RunTasmotaCommand(this.RestartAction);
                    Console.WriteLine("Restarted");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Restart failed with" + ex.Message);
                }

            }).Wait();
        }

        protected void uploadFile(string path)
        {
            Task.Run(async () =>
            {
                var restart = await this.uploadFileAsync(path, this.RestartAfterUpload);
                if (restart)
                    this.TriggerRestart();
            }).Wait(4000);
        }

        private async Task<bool> uploadFileAsync(string path, bool canRestart)
        {
            try
            {
                var fName = (path.StartsWith(this.Folder)) ? path.Substring(this.Folder.Length) : path;

                var cmd = "";
                var restart = this.RestartAfterUpload;
                var fInfo = new FileInfo(path);
                var extension = fInfo.Extension.ToLower();
                Console.WriteLine($"Upload file {fName} with extension {extension}");
                switch (extension)
                {
                    case ".tft":
                        cmd = $"FlashNextion {ServerUrl}/prj/{fName}";
                        restart = false;
                        break;
                    default:
                        cmd = $"UrlFetch {ServerUrl}/prj/{fName}";
                        break;
                }
                var waitingTask = this.WaitForServerTask(fName);
                await this.RunTasmotaCommand(cmd);
                var done = Task.WaitAny(waitingTask, Task.Delay(4000));

                if (done == 1)
                    throw new Exception($"Timeout while waiting for {fName}");

                this.ClearWaitForServerTask(fName);
                return restart && canRestart;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Upload failed with " + ex.Message);
                return false;
            }
        }

        private async Task RunTasmotaCommand(string cmd)
        {
            Console.WriteLine("CMD " + cmd);
            var encoded = HttpUtility.UrlEncode(cmd);
            var request = $"/cm?cmnd={encoded}";
            var result = await this.wc.GetAsync(request);
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                Console.Error.WriteLine(" " + result.StatusCode);
        }
    }
}
