//using NLog;
using System.Configuration;

namespace TwoWholeWorms.Lumbricus.Shared
{

    public class PluginConfigElement : ConfigurationElement
	{
     
//        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (base["name"] as string); }
        }

        [ConfigurationProperty("enabled", IsRequired = true)]
        public bool Enabled
        {
            get { return (base["enabled"].ToString() == "True"); }
        }

        [ConfigurationProperty("settings", IsRequired = false, IsDefaultCollection = false)]
        public PluginConfigSettingsCollection PluginConfigs {
            get { return (base["settings"] as PluginConfigSettingsCollection); }
        }

	}

}
