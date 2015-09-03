using NLog;
using System;
using System.Linq;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Shared.Plugins;
using TwoWholeWorms.Lumbricus.Plugins.IrcLogPlugin.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.IrcLogPlugin
{

    public class IrcLogPlugin : AbstractPlugin
    {
        
        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region AbstractPlugin implementation
        static IrcLogPlugin()
        {
            logger.Trace("Registering IRC log plugin");
            LumbricusConfiguration.AddPlugin(new IrcLogPlugin());
        }

        public override void RegisterPlugin(IrcConnection conn)
        {
            conn.ProcessIrcLine += DoLogPlugin;

            IrcLogContext.Initialise(conn.Config);
        }

        public override string Name {
            get {
                return "IRC Log Plugin";
            }
        }
        #endregion

        public void DoLogPlugin(IrcConnection conn, IrcLine line)
        {
            Nick nick = conn.Server.ConnectedNicks.FirstOrDefault(x => x.Name == line.Nick.ToLower());
            Channel ircChannel = conn.Server.ConnectedChannels.FirstOrDefault(x => x.Name == line.IrcCommandArgsRaw.ToLower());

            Log log = Log.Create();
            log.Server = conn.Server;
            log.Nick = nick;
            log.Account = nick?.Account;
            log.Channel = ircChannel;
            log.IrcCommand = line.IrcCommand.ToEnum<IrcCommand>();
            log.Trail = line.Trail;
            log.Line = line.RawLine;
            log.Save();
        }

    }

}

