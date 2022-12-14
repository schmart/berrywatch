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
    [Verb("upload", HelpText = "Watch folder and upload to tasmota device")]
    public class Uploader:UploaderBase,ICommand
    {
        public async Task<int> Run()
        {
            await this.Initialize();

            await this.uploadAllFiles();

            return 0;
        }
    }
}
