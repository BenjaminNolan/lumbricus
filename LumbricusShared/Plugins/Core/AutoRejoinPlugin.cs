using NLog;
using System;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;
using TwoWholeWorms.Lumbricus.Shared.Model;

namespace TwoWholeWorms.Lumbricus.Shared.Plugins.Core
{

    public class AutoRejoinPlugin : AbstractPlugin
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region AbstractPlugin implementation
        public override void RegisterPlugin(IrcConnection conn)
        {
            conn.ProcessIrcLine += DoAutoRejoinPlugin;
        }

        public override string Name {
            get {
                return "Auto-Rejoin Plugin";
            }
        }

        #endregion

        public void DoAutoRejoinPlugin(IrcConnection conn, IrcLine line)
        {
            IrcCommand ircCommand = (IrcCommand)Enum.Parse(typeof(IrcCommand), line.IrcCommand);
            if (ircCommand == IrcCommand.KICK && line.IrcCommandArgs.GetValue(1).ToString() == conn.Server.BotNick) {
                logger.Info("{0} kicked me from {1}, so rejoining it!", line.Nick, line.IrcCommandArgs.GetValue(0));
                conn.Send(string.Format("JOIN {0}", line.IrcCommandArgs.GetValue(0)));
            } else if (ircCommand == IrcCommand.PART && line.Nick == conn.Server.BotNick) {
                logger.Info("I parted from {0}, so rejoining it!", line.IrcCommandArgs.GetValue(0));
                conn.Send(string.Format("JOIN {0}", line.IrcCommandArgs.GetValue(0)));
            }
        }

    }

}
