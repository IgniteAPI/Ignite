using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Services;
using NLog;
using NLog.Extensions.Logging;
using NLog.Targets;
using IgniteWebUI.Components;
using IgniteWebUI.Components.Dashboard;
using IgniteWebUI.Components.Dashboard.InstanceInfoWidgets;
using IgniteWebUI.Configs;
using IgniteWebUI.Logging;
using IgniteWebUI.Models.Dashboard;
using IgniteWebUI.Services;
using IgniteWebUI.Services.InstanceServices;

namespace IgniteWebUI
{
    internal class Program
    {
        //Main entry point for the Web UI application. Sets up the web host, configures services, and starts the application.
        public static async Task Main(string[] args)
        {
            // Load Web Yaml configuration
            IgniteWebUICfg config = IgniteWebUICfg.LoadYaml(Path.Combine(AppContext.BaseDirectory, "IgniteWebUI.yml"));

            // Configure NLog from nlog.config
            Target.Register<LogViewerTarget>("LogViewer");
            LogManager.LoadConfiguration(Path.Combine(AppContext.BaseDirectory, "nlog.config"));
            LogManager.GlobalThreshold = NLog.LogLevel.FromString(config.Logging.LogLevel);

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
            builder.Services.AddSingleton<InstanceMetricsService>();
            builder.Services.AddSingleton<ThemeService>();
            builder.Services.AddSingleton<WidgetRegistry>();
            builder.Services.AddSingleton<DashboardLayoutService>();
            builder.Services.AddSingleton(config);
            builder.Services.AddSocketMessageHandlers(typeof(Program).Assembly);
            builder.Services.SetupSQL();
            builder.Logging.ClearProviders();
            builder.Logging.AddNLog();

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

            if (config.Network.UseHttpsRedirection)
                app.UseHttpsRedirection();

            await app.Services.MigrateDatabase();

            app.UseAntiforgery();
            app.MapControllers();

            app.MapStaticAssets();
            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();


             Console.WriteLine("Torch2 Web UI started successfully!");
            LogViewerTarget.Register(app.Services.GetRequiredService<InstanceLogService>());

            // Wire up metrics service to instance manager
            var instanceManager = app.Services.GetRequiredService<InstanceManager>();
            var metricsService = app.Services.GetRequiredService<InstanceMetricsService>();
            instanceManager.SetMetricsService(metricsService);

            // Auto-register widgets from attributes
            var widgetRegistry = app.Services.GetRequiredService<WidgetRegistry>();
            widgetRegistry.AutoRegisterWidgets(typeof(Program).Assembly);
            Console.WriteLine($"ConfigPath: {config.filePath}");
            Console.WriteLine($"Panel: {config.PanelName}");
            Console.WriteLine($"Port: {config.Port}");
            foreach (var url in app.Urls)
            {
                Console.WriteLine($"URL: {url}/scalar");
            }


            app.Run();


        }


            }
        }
