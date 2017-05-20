using NLog;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Plugins.BotBanPlugin
{
    
    public class BotBanPlugin : AbstractPlugin
    {
        
        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region AbstractPlugin implementation
        static BotBanPlugin()
        {
            logger.Trace("Registering bot ban plugin");
            LumbricusConfiguration.AddPlugin(new BotBanPlugin());
        }

        public override void RegisterPlugin(Connection conn)
        {
            conn.RegisterCommand("!botban", new Commands.BotBanCommand(conn));
        }

        public override string Name {
            get {
                return "Bot Ban Plugin";
            }
        }
        #endregion

    }

}
