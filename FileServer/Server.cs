
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

public class Server
{
    public void Run(string url, string path)
    {
        var args = new List<string>() {
            @"--urls",url
        };
        var builder = WebApplication.CreateBuilder(args.ToArray());

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

        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = (ctx) =>
            {
                Console.WriteLine(ctx.File.Name);
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
}
