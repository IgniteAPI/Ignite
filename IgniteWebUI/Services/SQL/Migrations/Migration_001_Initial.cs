using FluentMigrator;

namespace IgniteWebUI.Services.SQL.Migrations
{
    [Migration(1, "Initial schema creation")]
    public class Migration_001_Initial : Migration
    {
        public override void Up()
        {
            Create.Table("ConfiguredInstances")
                .WithColumn("InstanceID").AsString().PrimaryKey().NotNullable()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("MachineName").AsString().Nullable()
                .WithColumn("IPAddress").AsString().Nullable()
                .WithColumn("GamePort").AsInt32().NotNullable()
                .WithColumn("ProfileName").AsString().Nullable().WithDefaultValue(string.Empty)
                .WithColumn("TargetWorld").AsString().Nullable().WithDefaultValue(string.Empty)
                .WithColumn("TorchVersion").AsString().Nullable()
                .WithColumn("LastUpdate").AsDateTime().Nullable();

            Create.Table("ModLists")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).NotNullable()
                .WithColumn("Description").AsString(1000).Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("UpdatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.Table("Mods")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("ModListId").AsInt32().NotNullable().ForeignKey("FK_Mods_ModListId", "ModLists", "Id")
                .WithColumn("Name").AsString(255).NotNullable()
                .WithColumn("ModId").AsString(255).NotNullable()
                .WithColumn("Url").AsString(500).NotNullable()
                .WithColumn("Source").AsString(50).NotNullable().WithDefaultValue("SteamWorkshop")
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("FileSize").AsInt64().Nullable().WithDefaultValue(0)
                .WithColumn("PreviewUrl").AsString(500).Nullable()
                .WithColumn("Title").AsString(255).Nullable()
                .WithColumn("Description").AsString(int.MaxValue).Nullable()
                .WithColumn("TimeCreated").AsInt64().Nullable().WithDefaultValue(0)
                .WithColumn("TimeUpdated").AsInt64().Nullable().WithDefaultValue(0)
                .WithColumn("Subscriptions").AsInt32().Nullable().WithDefaultValue(0)
                .WithColumn("Favorites").AsInt32().Nullable().WithDefaultValue(0)
                .WithColumn("Views").AsInt32().Nullable().WithDefaultValue(0)
                .WithColumn("Tags").AsString(1000).Nullable()
                .WithColumn("SteamMetadataUpdatedAt").AsDateTime().Nullable();

            Create.Index("IX_Mods_ModListId").OnTable("Mods").OnColumn("ModListId").Ascending();
        }

        public override void Down()
        {
            Delete.Table("Mods");
            Delete.Table("ModLists");
            Delete.Table("ConfiguredInstances");
        }
    }
}
