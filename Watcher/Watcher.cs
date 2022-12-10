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
    public class Watcher:UploaderBase
    {
        private FileSystemWatcher watcher;
  
        [Option("initialUpload", Required = false, HelpText = "Upload all files at program start", Default = true)]
        public bool InitalUploadAllFiles { get; set; }

  
        public int Run()
        {
            return Task.Run(async () =>
            {

                await this.Initialize();

                if (this.InitalUploadAllFiles)
                {
                    await this.uploadAllFiles();
                }
                this.watch();
                await this.WaitForServer();
                return 0;

            }).Result;
        }

        private void watch()
        {
            this.watcher = new FileSystemWatcher();
            watcher.Path = this.Folder;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = this.Filter;
            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.EnableRaisingEvents = true;
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
