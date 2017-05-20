using NLog;
using System.Linq;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Plugins.TelegramConnectionPlugin.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.TelegramConnectionPlugin
{

    class TelegramConnection : AbstractConnection
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        protected Server server;
        protected PluginConfigSettingsParametersCollection settings;

        public TelegramConnection(PluginConfigSettingsParametersElement config) : base(config)
        {
            settings = config.Settings;
        }

        protected override void connect()
        {
        }

        protected override void loadServerDetails()
        {
            server = new Server() {
                Name = config.Name,
                Domain = settings.Single(p => p.Name == "Domain").Value,
                AccessToken = settings.Single(p => p.Name == "AccessToken").Value,
            };
        }

        protected override void Dispose(bool disposing)
        {

        }

        protected override void initialiseConnection()
        {

        }

        protected override void handleInput()
        {

        }

    }

}
