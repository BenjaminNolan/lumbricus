using NLog;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Plugins.GuardianPlugin
{

    public class GuardianPlugin : AbstractPlugin
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region AbstractPlugin implementation
        static GuardianPlugin()
        {
            logger.Trace("Registering guardian plugin");
            LumbricusConfiguration.AddPlugin(new GuardianPlugin());
        }

        public override void RegisterPlugin(IrcConnection conn)
        {
        }

        public override string Name {
            get {
                return "Guardian Plugin";
            }
        }
        #endregion

    }

}
