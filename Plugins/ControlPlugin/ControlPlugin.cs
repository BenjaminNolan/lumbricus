using NLog;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Plugins.ControlPlugin
{

    public class ControlPlugin : AbstractPlugin
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region AbstractPlugin implementation
        static ControlPlugin()
        {
            logger.Trace("Registering control plugin");
            LumbricusConfiguration.AddPlugin(new ControlPlugin());
        }

        public override void RegisterPlugin(Connection conn)
        {
            conn.RegisterCommand("!join", new Commands.JoinCommand(conn));
            conn.RegisterCommand("!part", new Commands.PartCommand(conn));
        }

        public override string Name {
            get {
                return "Control Plugin";
            }
        }
        #endregion

    }

}
