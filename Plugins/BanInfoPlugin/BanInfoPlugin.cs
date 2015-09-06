using NLog;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Plugins.BanInfoPlugin
{

    public class BanInfoPlugin : AbstractPlugin
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        static BanInfoPlugin()
        {
            logger.Trace("Registering ban info plugin");
            LumbricusConfiguration.AddPlugin(new BanInfoPlugin());
        }

        #region AbstractPlugin implementation
        public override void RegisterPlugin(IrcConnection conn)
        {
            conn.RegisterCommand("!baninfo", new Commands.BanInfoCommand(conn));
        }

        public override string Name {
            get {
                return "Ban Info Plugin";
            }
        }
        #endregion

    }

}
