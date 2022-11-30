// See https://aka.ms/new-console-template for more information
using berrywatch;
using CommandLine;

Parser.Default.ParseArguments<Watcher>(args)
              .WithParsed<Watcher>(o =>
                {

                });

CommandLine.Parser.Default.ParseArguments<Watcher>(args)
   .MapResult(
     (Watcher watcher) => watcher.Run(),
     errs => 1);
