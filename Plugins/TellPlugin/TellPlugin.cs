using NLog;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Plugins.TellPlugin
{

    public class TellPlugin : AbstractPlugin
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region AbstractPlugin implementation
        static TellPlugin()
        {
            logger.Trace("Registering tell plugin");
            LumbricusConfiguration.AddPlugin(new TellPlugin());
        }

        public override void RegisterPlugin(IrcConnection conn)
        {
            conn.RegisterCommand("!tell", new Commands.TellCommand(conn));
        }

        public override string Name {
            get {
                return "Tell Plugin";
            }
        }
        #endregion

    }

}
