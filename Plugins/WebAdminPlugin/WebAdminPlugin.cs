using NLog;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Plugins.WebAdminPlugin
{

    public class WebAdminPlugin : AbstractPlugin
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region AbstractPlugin implementation
        static WebAdminPlugin()
        {
            logger.Trace("Registering web admin plugin");
            LumbricusConfiguration.AddPlugin(new WebAdminPlugin());
        }

        public override void RegisterPlugin(IrcConnection conn)
        {
        }

        public override string Name {
            get {
                return "WebAdmin Plugin";
            }
        }
        #endregion

    }

}
