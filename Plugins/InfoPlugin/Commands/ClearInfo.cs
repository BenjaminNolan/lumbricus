using NLog;
using System;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Plugins.InfoPlugin.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.InfoPlugin.Commands
{

    public class ClearInfo : AbstractCommand
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public ClearInfo(IrcConnection conn) : base(conn)
        {
            // …
        }

        public override string Name {
            get {
                return "Clear Info Command";
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

                Info info = Info.Fetch(nick.Account);
                if (info == null) {
                    conn.SendPrivmsg(nick.Name, "You don't have any info in the database to clear! :o");
                    return;
                }

                nick.Account.MostRecentNick = nick;

                info.IsDeleted = true;
                info.Save();

                conn.SendPrivmsg(nick.Name, "Your info has been cleared. :(");
            } catch (Exception e) {
                logger.Error(e);
                conn.SendPrivmsg(nick.Name, "Oof… I banged my knee and it don't half hurt, I can't do that right now. :(");
            }
        }

    }

}
    