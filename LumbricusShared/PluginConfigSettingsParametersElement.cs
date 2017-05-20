//using NLog;
using System.Configuration;

namespace TwoWholeWorms.Lumbricus.Shared
{

    public class PluginConfigSettingsParametersElement : ConfigurationElement
	{
     
//        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (base["name"] as string); }
        }

        [ConfigurationProperty("parameters", IsRequired = false, IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(PluginConfigSettingsParametersCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public PluginConfigSettingsParametersCollection Settings
        {
            get { return base["parameters"] as PluginConfigSettingsParametersCollection; }
        }

	}

}
