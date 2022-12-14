
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

public class Server
{
    public event Action<string> FileCompleted;
    public void Run(string url, string path)
    {
        var args = new List<string>() {
            @"--urls",url
        };
        var builder = WebApplication.CreateBuilder(args.ToArray());

        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        builder.Services.AddRazorPages();
        builder.Services.AddControllersWithViews();
        builder.Services.AddDirectoryBrowser();

        var app = builder.Build();
       

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        var fileProvider = new PhysicalFileProvider(path, ExclusionFilters.None);
        var requestPath = "/prj";

        var provider = new FileExtensionContentTypeProvider();
        // Add new mappings
        provider.Mappings[".be"] = "text/javascript";
        provider.Mappings[".json"] = "text/javascript";
        provider.Mappings[".jsonl"] = "text/javascript";
        provider.Mappings[".cmd"] = "text/javascript";
        provider.Mappings[".tft"] = "application/octet-stream";

        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = (ctx) =>
            {
                ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store";
                ctx.Context.Response.Headers["Pragma"] = "no-cache";
                ctx.Context.Response.Headers["Expires"] = "-1";
                ctx.Context.Response.OnCompleted(this.OnFileCompleted, ctx.File.Name);
            },
            ContentTypeProvider = provider,
            FileProvider = fileProvider,
            RequestPath = requestPath
        });

        app.UseDirectoryBrowser(new DirectoryBrowserOptions
        {
            FileProvider = fileProvider,
            RequestPath = requestPath
        });

        app.UseAuthorization();

        app.MapDefaultControllerRoute();
        app.MapRazorPages();

        app.Run();
    }

    private async Task OnFileCompleted(object stringObject)
    {
        var fileName = stringObject as string;
        FileCompleted?.Invoke(fileName);
    }
}
