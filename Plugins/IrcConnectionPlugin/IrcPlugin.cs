using NLog;
using System;
using System.Collections.Generic;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Shared.Plugins;
using TwoWholeWorms.Lumbricus.Plugins.IrcConnectionPlugin.Model;
using System.Linq;

namespace TwoWholeWorms.Lumbricus.Plugins.IrcConnectionPlugin
{

    class IrcPlugin : AbstractConnectionPlugin
    {

        List<IrcConnection> connections = new List<IrcConnection>();
        PluginConfigElement pluginConfig;

        #region AbstractConnectionPlugin implementation
        public override void RegisterPlugin()
        {
            LumbricusConfiguration config = LumbricusConfiguration.GetConfig();
            pluginConfig = config.PluginConfigs.Single(c => c.Name == "IrcConnectionPlugin");
            foreach (PluginConfigSettingsParametersElement settings in pluginConfig.PluginConfigs) {
                IrcConnection conn = new IrcConnection(settings);
                connections.Add(conn);
            }
        }

        public override string Name {
            get {
                return "IrcConnectionPlugin";
            }
        }

        public override List<AbstractConnection> GetConnections()
        {
            List<AbstractConnection> conns = new List<AbstractConnection>();

            foreach (AbstractConnection conn in connections) {
                conns.Add(conn);
            }

            return conns;
        }
        #endregion

        protected bool isOp(IrcConnection conn, Nick nick)
        {
            if (nick.Account == null) {
                return false;
            } else if (nick.Account.IsOp) {
                return true;
            }

            Setting opsSetting = Setting.Fetch("Ops", "Nicks");
            if (opsSetting != null) {
                string[] nicks = opsSetting.Value.Split(" ".ToArray());
                foreach (string n in nicks) {
                    if (n.ToLower() == nick.Name.ToLower()) {
                        return true;
                    }
                }
            }

            Setting opsChannelSetting = Setting.Fetch("Ops", "Channels");
            if (opsChannelSetting != null) {
                string[] channels = opsChannelSetting.Value.Split(" ".ToArray());
                foreach (string c in channels) {
                    Channel opsChannel = conn.Server.ConnectedChannels.FirstOrDefault(x => x.Name == c);
                    if (opsChannel == null) {
                        throw new Exception(string.Format("Unable to find ops channel `{0}` in conn.Server.Channels", c));
                    }

                    if (opsChannel.ConnectedNicks.Contains(nick)) {
                        return true;
                    }
                }
            }

            return false;
        }

    }

}
