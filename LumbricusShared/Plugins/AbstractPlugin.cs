//using NLog;
using System;
using System.Linq;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Shared.Plugins
{

    abstract public class AbstractPlugin
    {

//        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        abstract public void RegisterPlugin(Connection conn);
        abstract public string Name { get; }

        protected bool isOp(Connection conn, Nick nick)
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
