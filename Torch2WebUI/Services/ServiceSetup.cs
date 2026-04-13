using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore; // Add this using directive
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Reflection;
using System.Threading.Tasks; // Add this using directive
using Torch2WebUI.Configs;
using Torch2WebUI.Services.InstanceServices;
using Torch2WebUI.Services.ModServices;
using Torch2WebUI.Services.SQL;

namespace Torch2WebUI.Services
{
    public static class ServiceSetup
    {
        static readonly string DatabaseFileName = "Torch2";

        public static void SetupSQL(this IServiceCollection Services)
        {
            string basePath = AppContext.BaseDirectory; // or Directory.GetCurrentDirectory()
            string directoryPath = Path.Combine(basePath, "Data");

            //Create Base Directory
            Directory.CreateDirectory(directoryPath);

            string databasePath = Path.Combine(directoryPath, $"{DatabaseFileName}.db");
            string SQLiteConnectionString = $"Data Source={databasePath}";

            Services.AddDbContext<AppDbContext>(options => options.UseSqlite(SQLiteConnectionString));
            Services.AddFluentMigratorCore().ConfigureRunner(rb => rb.AddSQLite().WithGlobalConnectionString(SQLiteConnectionString).ScanIn(typeof(ServiceSetup).Assembly).For.Migrations());

            // Register Mods Service
            Services.AddScoped<IModsService, ModsService>();

            // Register Steam Service for fetching mod metadata
            Services.AddHttpClient<ISteamService, ModServices.SteamService>();
        }

        /// <summary>
        /// Scans <paramref name="assembly"/> for every concrete <see cref="ISocketMessageHandler"/> implementation,
        /// registers each as a singleton of its own type (if not already registered), then adds a second
        /// <see cref="ISocketMessageHandler"/> registration that resolves the same singleton instance.
        /// New handlers are picked up automatically with no changes to Program.cs.
        /// </summary>
        public static IServiceCollection AddSocketMessageHandlers(this IServiceCollection services, Assembly assembly)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(ISocketMessageHandler).IsAssignableFrom(t));

            foreach (var handlerType in handlerTypes)
            {
                if (services.All(sd => sd.ServiceType != handlerType))
                    services.AddSingleton(handlerType);

                var captured = handlerType;
                services.AddSingleton<ISocketMessageHandler>(sp => (ISocketMessageHandler)sp.GetRequiredService(captured));
            }

            return services;
        }

        public static async Task MigrateDatabase(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Create database if it doesn't exist
                await dbContext.Database.EnsureCreatedAsync();

                // Run migrations
                runner.MigrateUp();

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
