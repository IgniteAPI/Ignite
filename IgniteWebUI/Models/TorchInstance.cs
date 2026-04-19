using Microsoft.VisualBasic;
using IgniteAPI.DTOs.Instances;
using IgniteAPI.Models.Configs;
using IgniteAPI.Models.Schema;
using IgniteAPI.Models.SE1;

namespace IgniteWebUI.Models
{
    public class TorchInstance : TorchInstanceBase
    {
        public bool Configured { get; set; } = false;

        //Profiles that are saved and recognized by the instance. This is used to display the available profiles in the UI
        public List<ProfileCfg> Profiles { get; set; } = new();

        //Worlds that are saved and recognized by the instance. This is used to display the available worlds in the UI
        public List<WorldInfo> WorldInfos { get; set; } = new();

        //Game custom worlds (Premade) that are saved and recognized by the instance. This is used to display the available custom worlds in the UI
        public List<WorldInfo> CustomWorlds { get; set; } = new();


        // The concrete dedicated config DTO (new): used to receive/send full config objects
        public ConfigDedicatedSE1 DedicatedConfig { get; set; }
    }
}
