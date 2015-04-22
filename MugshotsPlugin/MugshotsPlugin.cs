using NLog;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Plugins.MugshotsPlugin
{

    public class MugshotsPlugin : AbstractPlugin
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region AbstractPlugin implementation
        static MugshotsPlugin()
        {
            logger.Trace("Registering mugshots plugin");
            LumbricusConfiguration.AddPlugin(new MugshotsPlugin());
        }

        public override void RegisterPlugin(IrcConnection conn)
        {
            conn.RegisterCommand("!setmugshot", new Commands.SetMugshot(conn));
            conn.RegisterCommand("!clearmugshot", new Commands.ClearMugshot(conn));
        }

        public override string Name {
            get {
                return "Mugshots Plugin";
            }
        }
        #endregion

    }

}
