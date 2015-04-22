using NLog;
using System.Data.Entity;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Plugins.InfoPlugin.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.InfoPlugin
{

    public class InfoContext : DbContext
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public static InfoContext db;

        public DbSet<Info> Infos { get; set; }

        public static void Initialise(LumbricusConfiguration config)
        {
            if (db == null) {
                logger.Debug("Connecting to MySQL server");
                db = new InfoContext();
            } else {
                logger.Debug("Skipping database initialisation; already connected");
            }
        }

        public static void Obliterate()
        {
            db.Dispose();
        }

    }

}
