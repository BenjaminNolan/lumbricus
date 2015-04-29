using NLog;
using System;
using TwoWholeWorms.Lumbricus.Shared;
using System.Text.RegularExpressions;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Plugins.InfoPlugin.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.InfoPlugin.Commands
{

    public class SetInfo : AbstractCommand
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public SetInfo(IrcConnection conn) : base(conn)
        {
            // …
        }

        public override string Name {
            get {
                return "Set Info Command";
            }
        }

        public override void HandleCommand(IrcLine line, Nick nick, Channel channel)
        {
            try {
                Seen seen = Seen.Fetch(nick);
                DateTime checkTime = DateTime.Now;
                checkTime.AddDays(-7);
                if (seen.FirstSeenAt == DateTime.MinValue || seen.FirstSeenAt > checkTime) {
                    conn.SendPrivmsg(nick.Name, String.Format("Sorry, {0}, but you aren't allowed to use the mugshots functions yet. :(", nick.DisplayName));
                    return;
                }

                Regex r = new Regex(@"^!setinfo ?");
                line.Args = r.Replace(line.Args, "").Trim();
                if (line.Args.Length <= 0) { // Whaaaat??
                    conn.SendPrivmsg(nick.Name, "Usage: !setinfo <your message here>");
                } else {
                    nick.Account.MostRecentNick = nick;

                    Info info = Info.FetchOrCreate(nick.Account);
                    info.InfoTxt = line.Args;
                    info.Save();
                }
            } catch (Exception e) {
                logger.Error(e);
                conn.SendPrivmsg(nick.Name, "Oof… My neck is killing me, I can't do that right now. :(");
            }
        }

    }

}
    