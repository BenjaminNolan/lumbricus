using NLog;
using System;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Plugins.MugshotsPlugin.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.MugshotsPlugin.Commands
{

    public class ClearMugshot : AbstractCommand
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public ClearMugshot(IrcConnection conn) : base(conn)
        {
            // …
        }

        public override string Name {
            get {
                return "Clear Mugshot Command";
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

                Mugshot mugshot = Mugshot.Fetch(nick.Account);
                if (mugshot == null) {
                    conn.SendPrivmsg(nick.Name, "You don't have a mugshot in the database to clear! :o");
                    return;
                }

                nick.Account.MostRecentNick = nick;

                mugshot.IsDeleted = true;
                mugshot.Save();

                conn.SendPrivmsg(nick.Name, "Your mugshot has been cleared. :(");
            } catch (Exception e) {
                logger.Error(e);
                conn.SendPrivmsg(nick.Name, "Oof… I've got indigestion or something, I can't do that right now. :(");
            }
        }

    }

}
    