using NLog;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Plugins.OpsPlugin
{

    public class OpsPlugin : AbstractPlugin
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region AbstractPlugin implementation
        static OpsPlugin()
        {
            logger.Trace("Registering ops plugin");
            LumbricusConfiguration.AddPlugin(new OpsPlugin());
        }

        public override void RegisterPlugin(IrcConnection conn)
        {
            conn.RegisterCommand("!ops", new Commands.OpsCommand(conn));
        }

        public override string Name {
            get {
                return "Ops Plugin";
            }
        }
        #endregion

    }

}
