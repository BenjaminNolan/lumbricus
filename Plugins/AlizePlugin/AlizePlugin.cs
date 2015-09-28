using NLog;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Plugins.AlizePlugin
{

    public class AlizePlugin : AbstractPlugin
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region AbstractPlugin implementation
        static AlizePlugin()
        {
            logger.Trace("Registering Alize plugin");
            LumbricusConfiguration.AddPlugin(new AlizePlugin());
        }

        public override void RegisterPlugin(IrcConnection conn)
        {
        }

        public override string Name {
            get {
                return "Alize Plugin";
            }
        }
        #endregion

    }

}
