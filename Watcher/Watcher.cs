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
    [Verb("watch", HelpText = "Watch folder and upload to tasmota device")]
    public class Watcher
    {
        private FileSystemWatcher watcher;
        private HttpClient wc;
        private Timer timer = null;
        private Task serverTask = null;


        [Option('u', "url", Required = true, HelpText = "Host local server on this url. Example: http://192.168.178.101:5001")]
        public string ServerUrl { get; set; }


        [Option('d', "device", Required = true, HelpText = "IP/Adress of tasmota device")]
        public string DeviceAddress { get; set; }

        [Option('f', "folder", Required = true, HelpText = "Folder to watch. Must be an absolute path.")]
        public string Folder { get; set; }

        [Option("filter", Required = false, HelpText = "Filter for files to watch", Default = "*.*")]
        public string Filter { get; set; }

        [Option("restart", Required = false, HelpText = "Restart device after each upload", Default = true)]
        public bool RestartAfterUpload{ get; set; }

        [Option("initialUpload", Required = false, HelpText = "Upload all files at program start", Default = true)]
        public bool InitalUploadAllFiles { get; set; }

        public int Run()
        {
            return Task.Run(async () =>
            {
                this.wc = new HttpClient();
                this.wc.BaseAddress = new Uri($"http://{this.DeviceAddress}");

                await this.StartServer();

                if (this.InitalUploadAllFiles)
                {
                    await this.uploadAllFiles();
                }
                this.watch();
                if (serverTask != null)
                    await serverTask;

                return 0;

            }).Result;
        }

        private async Task StartServer()
        {
            serverTask = Task.Run(()=>
            {
                var srv = new Server();
                srv.Run(this.ServerUrl, this.Folder);
            });  
            while (!await this.CheckServerStarted() )
            {
                await Task.Delay(500);
            }
        }

        private async Task<bool> CheckServerStarted() {
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
        private async Task uploadAllFiles()
        {
            var all = Directory.GetFiles(this.Folder,this.Filter, SearchOption.TopDirectoryOnly);
            foreach (var file in all) 
            { 
                await this.uploadFileAsync(file);
            }
        }

        private void TriggerRestart()
        {
            if (!this.RestartAfterUpload)
                return;
            if (this.timer != null)
            {
                this.timer.Dispose(); 

            } 
        
            this.timer = new Timer((obj) => this.restarter(), null, 1000, Timeout.Infinite);
            

        }
        private void restarter()
        {
            this.timer.Dispose();
            this.timer = null;
            Task.Run(async () =>
            {
                await this.RunTasmotaCommand("restart 1");
                Console.WriteLine("Restarted");                
            }).Wait();
        }

        private void watch()
        {
            this.watcher = new FileSystemWatcher();
            watcher.Path = this.Folder;
            //watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
            //                       | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = this.Filter;
            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.EnableRaisingEvents = true;
        }
        private async void uploadFile(string path)
        {
            Task.Run(async () => {
                await this.uploadFileAsync(path); 
            }).Wait(3000);
        }

        private async Task uploadFileAsync(string path)
        {
            Console.WriteLine($"Upload file {path}");

            await this.RunTasmotaCommand($"UrlFetch {ServerUrl}/prj/{path}");
            if (this.RestartAfterUpload)
            {
                this.TriggerRestart();
            }
        }

        private async Task RunTasmotaCommand(string cmd)
        {
            var encoded = HttpUtility.UrlEncode(cmd);
            var request = $"/cm?cmnd={encoded}";
            Console.Write(request);
            var result = await this.wc.GetAsync(request);
            Console.WriteLine(" " +result.StatusCode);
        }
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            this.uploadFile(e.Name);
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            this.uploadFile(e.Name);
        }

        public void Dispose()
        {
            // avoiding resource leak
            watcher.Changed -= OnChanged;
            this.watcher.Dispose();
        }
    }
}
