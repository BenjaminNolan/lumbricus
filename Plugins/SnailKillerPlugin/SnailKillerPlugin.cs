using NLog;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Plugins.SnailKillerPlugin
{

    public class SnailKillerPlugin : AbstractPlugin
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region AbstractPlugin implementation
        static SnailKillerPlugin()
        {
            logger.Trace("Registering snail killer plugin");
            LumbricusConfiguration.AddPlugin(new SnailKillerPlugin());
        }

        public override void RegisterPlugin(IrcConnection conn)
        {
        }

        public override string Name {
            get {
                return "SnailKiller Plugin";
            }
        }
        #endregion

    }

}
