using MudBlazor.Services;
using NLog;
using NLog.Targets;
using Torch2WebUI.Components;
using Torch2WebUI.Configs;
using Torch2WebUI.Services;
using Torch2WebUI.Services.InstanceServices;

namespace Torch2WebUI
{
    internal class Program
    {
        //Main entry point for the Web UI application. Sets up the web host, configures services, and starts the application.
        public static async Task Main(string[] args)
        {
            // Load Web Yaml configuration
            Torch2WebUICfg config = Torch2WebUICfg.LoadYaml(Path.Combine(AppContext.BaseDirectory, "torch2webui.yml"));

            // Configure NLog for instance logging
            SetupInstanceLogging(config);

            var builder = WebApplication.CreateBuilder(args);
 
            builder.Services.AddControllers();

            // Add MudBlazor services
            builder.Services.AddMudServices();
            builder.Services.AddMemoryCache();

            // Add services to the container.
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();

            builder.Services.AddSingleton<InstanceManager>();
            builder.Services.AddSingleton<InstanceChatService>();
            builder.Services.AddSingleton<InstanceSocketManager>();
            builder.Services.AddSingleton<InstanceCommandService>();
            builder.Services.AddSingleton<ThemeService>();
            builder.Services.AddSingleton(config);
            builder.Services.AddSocketMessageHandlers(typeof(Program).Assembly);
            builder.Services.SetupSQL();
            builder.Logging.ClearProviders();

            Console.WriteLine("Starting Torch2 Web UI...");
            var app = builder.Build();
            app.UseWebSockets();
            app.Map("/ws/instance", async context =>
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
                    context.Response.StatusCode = 400;
                    return;
                }

                var socket = await context.WebSockets.AcceptWebSocketAsync();
                var manager = context.RequestServices.GetRequiredService<InstanceSocketManager>();

                await manager.HandleConnectionAsync(context, socket);
            });

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            await app.Services.MigrateDatabase();

            app.UseAntiforgery();
            app.MapControllers();

            app.MapStaticAssets();
            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();


            Console.WriteLine("Torch2 Web UI started successfully!");
            Console.WriteLine($"ConfigPath: {config.filePath}");
            Console.WriteLine($"Panel: {config.PanelName}");
            Console.WriteLine($"Port: {config.Port}");
            foreach (var url in app.Urls)
            {
                Console.WriteLine($"URL: {url}/scalar");
            }


            app.Run();


        }


        //Convert to file?
        private static void SetupInstanceLogging(Torch2WebUICfg config)
        {
            if (!config.Logging.EnableInstanceLogging)
                return;

            var logConfig = new NLog.Config.LoggingConfiguration();

            var fileTarget = new FileTarget("instanceFile")
            {
                FileName = Path.Combine(config.Logging.LogDirectory, "instances.log"),
                ArchiveFileName = Path.Combine(config.Logging.LogDirectory, "instances.{#}.log"),
                Layout = "${longdate} | ${level:uppercase=true:padding=5} | ${message}",
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Date,
                ArchiveDateFormat = "yyyy-MM-dd",
                MaxArchiveFiles = config.Logging.MaxLogAgeDays,
            };

            var chatFileTarget = new FileTarget("chatFile")
            {
                FileName = Path.Combine(config.Logging.LogDirectory, "chat.log"),
                ArchiveFileName = Path.Combine(config.Logging.LogDirectory, "chat.{#}.log"),
                Layout = "${longdate} | ${level:uppercase=true:padding=5} | ${message}",
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Date,
                ArchiveDateFormat = "yyyy-MM-dd",
                MaxArchiveFiles = config.Logging.MaxLogAgeDays,
            };

            var minLogLevel = NLog.LogLevel.FromString(config.Logging.LogLevel);

            logConfig.AddTarget(fileTarget);
            logConfig.AddTarget(chatFileTarget);
            logConfig.AddRule(minLogLevel, NLog.LogLevel.Fatal, fileTarget, "Torch2WebUI.Services.InstanceServices.InstanceLogService*");
            logConfig.AddRule(minLogLevel, NLog.LogLevel.Fatal, chatFileTarget, "Torch2WebUI.Services.InstanceServices.InstanceChatService*");

            LogManager.Configuration = logConfig;
        }
    }
}
