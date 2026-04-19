using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IgniteAPI.DTOs.Instances;
using IgniteWebUI.Models.Database;

namespace IgniteWebUI.Services.SQL
{
    public class AppDbContext : DbContext
    {
        static readonly string DatabaseName = "Torch2WebData";

        //Saved Instances
        public DbSet<ConfiguredInstance> ConfiguredInstances { get; set; }

        //Mod Lists and Mods
        public DbSet<ModList> ModLists { get; set; }
        public DbSet<Mod> Mods { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            this.SaveChangesFailed += AppDbContext_SaveChangesFailed;
        }

        private void AppDbContext_SaveChangesFailed(object? sender, SaveChangesFailedEventArgs e)
        {
            Console.WriteLine("SaveChangesFailed: " + e.Exception.Message);
        }


    }
}
