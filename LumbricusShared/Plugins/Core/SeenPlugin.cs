using NLog;
using System;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;
using TwoWholeWorms.Lumbricus.Shared.Model;

namespace TwoWholeWorms.Lumbricus.Shared.Plugins.Core
{

    public class SeenPlugin : AbstractPlugin
    {

//        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region AbstractPlugin implementation

        public override void RegisterPlugin(Connection conn)
        {
            conn.ProcessIrcLine += DoSeenPlugin;
            conn.RegisterCommand("!seen", new Commands.SeenCommand(conn));
        }

        public override string Name {
            get {
                return "Seen Plugin";
            }
        }

        #endregion

        public void DoSeenPlugin(Connection conn, IrcLine line)
        {
            // :Nick!~User@cpc1-newt1-2-3-cust1.2-3.cable.virginm.net JOIN #channel
            // :Nick!~User@unaffiliated/Nick JOIN #channel
            // :Nick!~User@unaffiliated/Nick PART #channel
            // :Nick!~User@unaffiliated/Nick PART #channel :"screw you"
            // :Nick!~User@a1-2-3-4.cpe.netcabo.pt QUIT :Quit: Quit!
            // :TwoWholeWorms!heifer@ben.mu NICK :FourHalfWorms
            // :Nick!~User@nickna.me PRIVMSG Lumbricus :!info Nick2
            Channel channel = null;
            IrcCommand ircCommand = (IrcCommand)Enum.Parse(typeof(IrcCommand), line.IrcCommand);
            switch (ircCommand) {
                case IrcCommand.JOIN:
                case IrcCommand.PART:
                case IrcCommand.PRIVMSG:
                    if (line.IrcCommandArgsRaw.StartsWith("#")) {
                        channel = conn.GetChannel(line.IrcCommandArgsRaw);
                    }
                    goto case IrcCommand.NICK;

                case IrcCommand.NICK:
                case IrcCommand.NOTICE:
                case IrcCommand.QUIT:
                    Nick nick = Nick.FetchOrCreate(line.Nick, conn.Server);
                    if (nick == null) {
                        throw new Exception(String.Format("Unable to fetch or create nick `{0}`", nick));
                    }

                    Seen.Update(conn.Server, nick, nick.Account, channel);

                    break;
            }
        }

    }

}
