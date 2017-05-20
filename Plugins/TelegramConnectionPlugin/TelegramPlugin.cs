using NLog;
using System;
using System.Linq;
using System.Collections.Generic;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Plugins.TelegramConnectionPlugin
{

    class TelegramPlugin : AbstractConnectionPlugin
    {

        TelegramConnection conn;
        PluginConfigElement pluginConfig;

        #region AbstractConnectionPlugin implementation
        public override void RegisterPlugin()
        {
            LumbricusConfiguration config = LumbricusConfiguration.GetConfig();
            pluginConfig = config.PluginConfigs.Single(c => c.Name == "IrcConnectionPlugin");
            foreach (PluginConfigSettingsParametersElement settings in pluginConfig.PluginConfigs) {
                if (conn != null) {
                    throw new Exception("Telegram config can only have one settings set.");
                }
                conn = new TelegramConnection(settings);
            }
        }

        public override string Name {
            get {
                return "TelegramConnectionPlugin";
            }
        }

        public override List<AbstractConnection> GetConnections()
        {
            List<AbstractConnection> conns = new List<AbstractConnection>();
            conns.Add(conn);

            return conns;
        }

        public override void Run()
        {
            
        }
        #endregion

    }

}
