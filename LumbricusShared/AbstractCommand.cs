using System;
using System.Linq;
using TwoWholeWorms.Lumbricus.Shared.Model;

namespace TwoWholeWorms.Lumbricus.Shared
{
    
    abstract public class AbstractCommand
    {
        
        protected readonly IrcConnection conn;
        protected AbstractCommand(IrcConnection conn)
        {
            this.conn = conn;
        }

        abstract public void HandleCommand(IrcLine line, Nick nick, Channel channel);
        abstract public string Name { get; }

        protected bool isOp(Nick nick)
        {
            Setting opsChannelSetting = Setting.Fetch("ops", "channel");
            if (opsChannelSetting == null) {
                return false;
            }

            Channel opsChannel = conn.Server.ConnectedChannels.FirstOrDefault(x => x.Name == opsChannelSetting.Value);
            if (opsChannel == null) {
                throw new Exception("Unable to find ops channel in conn.Server.Channels");
            }

            return opsChannel.ConnectedNicks.Contains(nick);
        }

    }

}
