using NLog;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Plugins.InfoPlugin
{

    public class InfoPlugin : AbstractPlugin
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region AbstractPlugin implementation
        static InfoPlugin()
        {
            logger.Trace("Registering info plugin");
            LumbricusConfiguration.AddPlugin(new InfoPlugin());
        }

        public override void RegisterPlugin(IrcConnection conn)
        {
            conn.RegisterCommand("!setinfo", new Commands.SetInfoCommand(conn));
            conn.RegisterCommand("!clearinfo", new Commands.ClearInfoCommand(conn));

            InfoContext.Initialise(conn.Config);
        }

        public override string Name {
            get {
                return "Info Plugin";
            }
        }
        #endregion

    }

}
