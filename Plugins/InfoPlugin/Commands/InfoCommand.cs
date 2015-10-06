using NLog;
using System;
using TwoWholeWorms.Lumbricus.Shared;
using System.Text.RegularExpressions;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Plugins.InfoPlugin.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.InfoPlugin.Commands
{

    public class InfoCommand : AbstractCommand
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public InfoCommand(IrcConnection conn) : base(conn)
        {
            // …
        }

        public override string Name {
            get {
                return "Info Command";
            }
        }

        public override void HandleCommand(IrcLine line, Nick nick, Channel channel)
        {
            try {
                Seen seen = Seen.Fetch(nick);
                DateTime checkTime = DateTime.Now;
                checkTime.AddDays(-7);
                if (seen.FirstSeenAt == DateTime.MinValue || seen.FirstSeenAt > checkTime) {
                    conn.SendPrivmsg(nick.Name, String.Format("Sorry, {0}, but you aren't allowed to use the info functions yet. :(", nick.DisplayName));
                    return;
                }

                Regex r = new Regex(@"^!info ?");
                line.Args = r.Replace(line.Args, "").Trim();
                if (line.Args.Length <= 0) { // Whaaaat??
                    conn.SendPrivmsg(nick.Name, "Usage: !info <Nick>");
                } else {
                    Nick infoNick = Nick.Fetch(line.Args, conn.Server);
                    string returnPath = nick.DisplayName;
                    if (null != channel && channel.AllowCommandsInChannel) {
                        returnPath = channel.Name;
                    }
                    if (null != infoNick) {
                        Info info = Info.Fetch(infoNick);
                        if (null != info && info.InfoTxt.Length > 0) {
                            conn.SendPrivmsg(returnPath, string.Format("{0}: {1}", infoNick.DisplayName, info.InfoTxt));
                        } else {
                            conn.SendPrivmsg(returnPath, string.Format("Sorry, but I don't know anything about {0}. :(", line.Args));
                        }
                    } else {
                        conn.SendPrivmsg(returnPath, string.Format("Sorry, but I don't know anything about {0}. :(", line.Args));
                    }
                }
            } catch (Exception e) {
                logger.Error(e);
                conn.SendPrivmsg(nick.Name, "Oof… My neck is killing me, I can't do that right now. :(");
            }
        }

    }

}
    