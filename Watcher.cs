using CommandLine.Text;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace berrywatch
{
    [Verb("watch", HelpText = "Watch folder and upload to tasmota device")]
    public class Watcher
    {
        private FileSystemWatcher watcher;

        [Option('d', "device", Required = true, HelpText = "IP/Adress of tasmota device")]
        public string DeviceAddress { get; set; }

        [Option('f', "folder", Required = true, HelpText = "Folder to watch", Default = ".")]
        public string Folder { get; set; }

        [Option("filter", Required = false, HelpText = "Filter for files to watch", Default = "*.be")]
        public string Filter { get; set; }
        public int Run()
        {
            return Task.Run(async () => {
                return await this.RunAsync();
            }).Result;
        }

        private async Task<int> RunAsync()
        {
            this.watch();
            while (true)
            {
                await Task.Delay(100);
            }
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

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File {e.Name} created - {e.ChangeType}");
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"File {e.Name} modified - {e.ChangeType}");
            //Copies file to another directory.
        }

        public void Dispose()
        {
            // avoiding resource leak
            watcher.Changed -= OnChanged;
            this.watcher.Dispose();
        }
    }
}
