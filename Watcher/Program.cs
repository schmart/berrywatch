// See https://aka.ms/new-console-template for more information
using berrywatch;
using CommandLine;

await Parser.Default
    .ParseArguments<Watcher, Uploader>(args)
    .WithParsedAsync<ICommand>(async o =>
        {
            try
            {
                var result = await o.Run();
                Console.WriteLine($"Result {result}");
            } catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        });
