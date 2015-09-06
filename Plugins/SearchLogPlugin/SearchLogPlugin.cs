using NLog;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Plugins.SearchLogPlugin
{

    public class SearchLogPlugin : AbstractPlugin
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region AbstractPlugin implementation
        static SearchLogPlugin()
        {
            logger.Trace("Registering search log plugin");
            LumbricusConfiguration.AddPlugin(new SearchLogPlugin());
        }

        public override void RegisterPlugin(IrcConnection conn)
        {
            conn.RegisterCommand("!searchlog", new Commands.SearchLogCommand(conn));
        }

        public override string Name {
            get {
                return "Search Log Plugin";
            }
        }
        #endregion

    }

}
