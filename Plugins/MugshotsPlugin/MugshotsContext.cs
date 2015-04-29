using NLog;
using System.Data.Entity;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Plugins.MugshotsPlugin.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.MugshotsPlugin
{

    public class MugshotsContext : DbContext
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public static MugshotsContext db;

        public DbSet<Mugshot> Mugshots { get; set; }

        public static void Initialise(LumbricusConfiguration config)
        {
            if (db == null) {
                logger.Debug("Connecting to MySQL server");
                db = new MugshotsContext();
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
