using NLog;
using System.Data.Entity;
using TwoWholeWorms.Lumbricus.Shared.Model;

namespace TwoWholeWorms.Lumbricus.Shared
{
    
    public class LumbricusContext : DbContext
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();
        
        public static LumbricusContext db;

        public DbSet<Account>     Accounts     { get; set; }
        public DbSet<Ban>         Bans         { get; set; }
        public DbSet<Channel>     Channels     { get; set; }
        public DbSet<Log>         Logs         { get; set; }
        public DbSet<Nick>        Nicks        { get; set; }
        public DbSet<Server>      Servers      { get; set; }
        public DbSet<Setting>     Settings     { get; set; }

        public static void Initialise(LumbricusConfiguration config)
        {
            if (db == null) {
                logger.Info("Initialising Lumbricus database context");
                db = new LumbricusContext();
            } else {
                logger.Info("Skipping database initialisation; already connected");
            }
        }

        public static void Obliterate()
        {
            db.Dispose();
        }

    }

}

