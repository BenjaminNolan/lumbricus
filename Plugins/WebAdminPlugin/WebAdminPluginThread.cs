using Nancy.Hosting.Self;
using NLog;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;
using System;

namespace TwoWholeWorms.Lumbricus.Plugins.WebAdminPlugin
{

    public class WebAdminPluginThread : AbstractPluginThread
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region AbstractPlugin implementation
        public WebAdminPluginThread()
        {
            logger.Trace("Initialising web admin plugin thread");
        }

        public override void Run()
        {
            logger.Trace("Initialising Nancy");
            using (var host = new NancyHost(new Uri("http://localhost:4297"))) {
                host.Start();
            }
        }
        #endregion

    }

}
