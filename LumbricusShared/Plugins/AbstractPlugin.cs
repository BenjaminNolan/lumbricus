using NLog;
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

        abstract public void RegisterPlugin(IrcConnection conn);
        abstract public string Name { get; }

        protected bool isOp(IrcConnection conn, Nick nick)
        {
            Setting opsChannelSetting = Setting.Fetch("ops", "channel");
            if (opsChannelSetting == null) {
                return false;
            }

            Channel opsChannel = conn.Server.Channels.FirstOrDefault(x => x.Name == opsChannelSetting.Value);
            if (opsChannel == null) {
                throw new Exception("Unable to find ops channel in conn.Server.Channels");
            }

            return opsChannel.Nicks.Contains(nick);
        }

    }

}
