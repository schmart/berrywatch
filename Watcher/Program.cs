// See https://aka.ms/new-console-template for more information
using berrywatch;
using CommandLine;

Parser.Default.ParseArguments<Watcher,Uploader>(args)
              .WithParsed<Watcher>(o =>
                {
                    o.Run();
                })
              .WithParsed<Uploader>(o =>
               {
                   o.Run();
               });
