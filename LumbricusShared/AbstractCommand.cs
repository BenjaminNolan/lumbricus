using System;
using System.Linq;
using TwoWholeWorms.Lumbricus.Shared.Model;

namespace TwoWholeWorms.Lumbricus.Shared
{
    
    abstract public class AbstractCommand
    {
        
        protected readonly Connection conn;
        protected AbstractCommand(Connection conn)
        {
            this.conn = conn;
        }

        abstract public void HandleCommand(IrcLine line, Nick nick, Channel channel);
        abstract public string Name { get; }

        protected bool isOp(Nick nick)
        {
            if (nick.Account == null) {
                return false;
            } else if (nick.Account.IsOp) {
                return true;
            }

            Setting opsSetting = Setting.Fetch("ops", "nicks");
            if (opsSetting != null) {
                string[] nicks = opsSetting.Value.Split(" ".ToArray());
                foreach (string n in nicks) {
                    if (n.ToLower() == nick.Name.ToLower()) {
                        return true;
                    }
                }
            }

            Setting opsChannelSetting = Setting.Fetch("ops", "channel");
            if (opsChannelSetting != null) {
                Channel opsChannel = conn.Server.ConnectedChannels.FirstOrDefault(x => x.Name == opsChannelSetting.Value);
                if (opsChannel == null) {
                    throw new Exception("Unable to find ops channel in conn.Server.Channels");
                }

                return opsChannel.ConnectedNicks.Contains(nick);
            }

            return false;
        }

    }

}
