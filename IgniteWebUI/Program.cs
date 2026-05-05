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

            // Register core dashboard widgets
            var widgetRegistry = app.Services.GetRequiredService<WidgetRegistry>();
            widgetRegistry.Register(new WidgetDescriptor
            {
                Id = "core.welcome",
                DisplayName = "Welcome",
                Icon = Icons.Material.Filled.Home,
                ColSpan = 6, RowSpan = 1,
                Content = () => b => { b.OpenComponent<WelcomeWidget>(0); b.CloseComponent(); }
            });
            widgetRegistry.Register(new WidgetDescriptor
            {
                Id = "core.active_instances",
                DisplayName = "Active Instances",
                Icon = Icons.Material.Filled.Dns,
                ColSpan = 6, RowSpan = 1,
                Content = () => b => { b.OpenComponent<ActiveInstancesWidget>(0); b.CloseComponent(); }
            });
            widgetRegistry.Register(new WidgetDescriptor
            {
                Id = "core.recent_logs",
                DisplayName = "Recent Logs",
                Icon = Icons.Material.Filled.Article,
                ColSpan = 6, RowSpan = 2,
                Content = () => b => { b.OpenComponent<RecentLogsWidget>(0); b.CloseComponent(); }
            });
            widgetRegistry.Register(new WidgetDescriptor
            {
                Id = "core.quick_stats",
                DisplayName = "Quick Stats",
                Icon = Icons.Material.Filled.BarChart,
                ColSpan = 4, RowSpan = 2,
                Content = () => b => { b.OpenComponent<QuickStatsWidget>(0); b.CloseComponent(); }
            });
            widgetRegistry.Register(new WidgetDescriptor
            {
                Id = "core.server_status",
                DisplayName = "Server Status",
                Icon = Icons.Material.Filled.Monitor,
                ColSpan = 6, RowSpan = 1,
                Content = () => b => { b.OpenComponent<ServerStatusWidget>(0); b.CloseComponent(); }
            });
            widgetRegistry.Register(new WidgetDescriptor
            {
                Id = "core.quick_actions",
                DisplayName = "Quick Actions",
                Icon = Icons.Material.Filled.Bolt,
                ColSpan = 3, RowSpan = 1,
                Content = () => b => { b.OpenComponent<QuickActionsWidget>(0); b.CloseComponent(); },
                ConfigComponent = widget => b => 
                { 
                    b.OpenComponent<QuickActionsWidgetConfig>(0);
                    b.AddAttribute(1, "Widget", widget);
                    b.CloseComponent(); 
                }
            });
            widgetRegistry.Register(new WidgetDescriptor
            {
                Id = "core.errors",
                DisplayName = "Errors & Warnings",
                Icon = Icons.Material.Filled.Warning,
                ColSpan = 6, RowSpan = 2,
                Content = () => b => { b.OpenComponent<ErrorsWidget>(0); b.CloseComponent(); }
            });

            // Register instance info widgets
            widgetRegistry.Register(new WidgetDescriptor
            {
                Id = "instance.world",
                DisplayName = "World",
                Icon = Icons.Material.Filled.Public,
                ColSpan = 4, RowSpan = 1,
                Content = () => b => { b.OpenComponent<InstanceWorldWidget>(0); b.CloseComponent(); },
                ConfigComponent = widget => b =>
                {
                    b.OpenComponent<InstanceWidgetConfigBase>(0);
                    b.CloseComponent();
                }
            });
            widgetRegistry.Register(new WidgetDescriptor
            {
                Id = "instance.profile",
                DisplayName = "Profile Name",
                Icon = Icons.Material.Filled.Description,
                ColSpan = 4, RowSpan = 1,
                Content = () => b => { b.OpenComponent<InstanceProfileWidget>(0); b.CloseComponent(); },
                ConfigComponent = widget => b =>
                {
                    b.OpenComponent<InstanceWidgetConfigBase>(0);
                    b.CloseComponent();
                }
            });
            widgetRegistry.Register(new WidgetDescriptor
            {
                Id = "instance.host",
                DisplayName = "Host Machine",
                Icon = Icons.Material.Filled.Computer,
                ColSpan = 4, RowSpan = 1,
                Content = () => b => { b.OpenComponent<InstanceHostWidget>(0); b.CloseComponent(); },
                ConfigComponent = widget => b =>
                {
                    b.OpenComponent<InstanceWidgetConfigBase>(0);
                    b.CloseComponent();
                }
            });
            widgetRegistry.Register(new WidgetDescriptor
            {
                Id = "instance.address",
                DisplayName = "Address",
                Icon = Icons.Material.Filled.LocationOn,
                ColSpan = 4, RowSpan = 1,
                Content = () => b => { b.OpenComponent<InstanceAddressWidget>(0); b.CloseComponent(); },
                ConfigComponent = widget => b =>
                {
                    b.OpenComponent<InstanceWidgetConfigBase>(0);
                    b.CloseComponent();
                }
            });
            widgetRegistry.Register(new WidgetDescriptor
            {
                Id = "instance.version",
                DisplayName = "Torch Version",
                Icon = Icons.Material.Filled.Info,
                ColSpan = 4, RowSpan = 1,
                Content = () => b => { b.OpenComponent<InstanceVersionWidget>(0); b.CloseComponent(); },
                ConfigComponent = widget => b =>
                {
                    b.OpenComponent<InstanceWidgetConfigBase>(0);
                    b.CloseComponent();
                }
            });
            widgetRegistry.Register(new WidgetDescriptor
            {
                Id = "instance.uptime",
                DisplayName = "Uptime",
                Icon = Icons.Material.Filled.Schedule,
                ColSpan = 4, RowSpan = 1,
                Content = () => b => { b.OpenComponent<InstanceUptimeWidget>(0); b.CloseComponent(); },
                ConfigComponent = widget => b =>
                {
                    b.OpenComponent<InstanceWidgetConfigBase>(0);
                    b.CloseComponent();
                }
            });
            widgetRegistry.Register(new WidgetDescriptor
            {
                Id = "instance.state",
                DisplayName = "Current State",
                Icon = Icons.Material.Filled.PlayCircle,
                ColSpan = 4, RowSpan = 1,
                Content = () => b => { b.OpenComponent<InstanceStateWidget>(0); b.CloseComponent(); },
                ConfigComponent = widget => b =>
                {
                    b.OpenComponent<InstanceWidgetConfigBase>(0);
                    b.CloseComponent();
                }
            });
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
