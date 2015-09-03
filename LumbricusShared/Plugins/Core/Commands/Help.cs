using NLog;
using System;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;

namespace TwoWholeWorms.Lumbricus.Shared.Plugins.Core.Commands
{

    public class Help : AbstractCommand
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public Help(IrcConnection conn) : base(conn)
        {
            // …
        }

        public override string Name {
            get {
                return "Help Command";
            }
        }

        public override void HandleCommand(IrcLine line, Nick nick, Channel channel)
        {
            try {
                Setting helpUriSetting = Setting.Fetch("Help", "Uri");
                if (helpUriSetting == null) {
                    conn.SendPrivmsg(nick.Name, String.Format("Oh dear… I'm afraid I don't seem to be able to answer that right now, {0}!", nick.DisplayName));
                    return;
                }
                if (nick.Account != null && isOp(nick)) {
                    string target = nick.Name;
                    if (channel != null && channel.AllowCommandsInChannel) {
                        target = channel.Name;
                    }
                    conn.SendPrivmsg(target, String.Format("Hi, @{0}. Main help is at {1}. You also have access to these op-only commands: !seen, !baninfo, !botban, !restart", nick.DisplayName, helpUriSetting.Value));
                } else {
                    conn.SendPrivmsg(nick.Name, String.Format("Hi, {0}. Help is at {1}.", nick.DisplayName, helpUriSetting.Value));
                    if (channel != null && !channel.AllowCommandsInChannel) {
                        conn.SendPrivmsg(nick.Name, "Also, please try to only interact with me directly through this window. Bot commands in the main channel are against channel policy, and some people get really annoyed about it. :(");
                    }
                }
                } catch (Exception e) {
                logger.Error(e);
                conn.SendPrivmsg(nick.Name, "Oof… I really shouldn't have had that second slice of cake, I can't do that right now. :(");
            }
        }

    }

}
