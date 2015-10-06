using System;
using Nancy;
using NLog;

namespace TwoWholeWorms.Lumbricus.Plugins.WebAdminPlugin
{
    
    public class WebAdminModule : NancyModule
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();
        
        public WebAdminModule()
        {
            logger.Trace("Initialising web admin module");

            Get["/"] = parameters => "WebAdmin";
        }

    }

}

