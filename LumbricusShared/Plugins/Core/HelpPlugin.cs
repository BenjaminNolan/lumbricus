using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Shared.Plugins.Core
{

    public class HelpPlugin : AbstractPlugin
    {

        #region AbstractPlugin implementation
        public override void RegisterPlugin(Connection conn)
        {
            conn.RegisterCommand("!help", new Commands.HelpCommand(conn));
        }

        public override string Name {
            get {
                return "Help Plugin";
            }
        }

        #endregion

    }

}
