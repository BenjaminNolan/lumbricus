﻿using NLog;
using System.Configuration;
using System.Collections.Generic;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Shared
{

    public class LumbricusConfiguration : ConfigurationSection
    {

        static LumbricusConfiguration current = null;
        public const string SECTION_NAME = "lumbricusConfig";

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        [ConfigurationProperty("plugins")]
        public PluginConfigCollection PluginConfigs
        {
            get { return (base["plugins"] as PluginConfigCollection); }
        }

        public static LumbricusConfiguration GetConfig()
        {
            if (current == null) {
                current = ConfigurationManager.GetSection(LumbricusConfiguration.SECTION_NAME) as LumbricusConfiguration;
            }
            return current;
        }

        public static List<AbstractPlugin> Plugins = new List<AbstractPlugin>();

        public static void AddPlugin(AbstractPlugin plugin)
        {
            logger.Debug("Registered plugin {0}", plugin.GetType());
            Plugins.Add(plugin);
        }

    }

}
