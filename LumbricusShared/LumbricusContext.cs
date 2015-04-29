using NLog;
using System.Data.Entity;
using TwoWholeWorms.Lumbricus.Shared.Model;
using System.Data.Entity.Validation;
using TwoWholeWorms.Lumbricus.Shared.Exceptions;
using System.Data.Entity.Infrastructure;
using System.Linq;

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
                logger.Info("Skipping database initialisation; already done");
            }
        }

//        protected override void OnModelCreating(DbModelBuilder modelBuilder)
//        {
//            modelBuilder.Entity<Account>()
//                .HasRequired(account => account.Server);
//            
//            modelBuilder.Entity<Account>()
//                .HasOptional(account => account.MostRecentNick);
//            
//            modelBuilder.Entity<Account>()
//                .HasOptional(account => account.ChannelLastSeenIn);
//            
//            modelBuilder.Entity<Account>()
//                .HasMany(account => account.Nicks)
//                .WithOptional(nick => nick.Account);
//
//            modelBuilder.Entity<Ban>()
//                .HasRequired(ban => ban.Server);
//
//            modelBuilder.Entity<Ban>()
//                .HasOptional(ban => ban.Account);
//
//            modelBuilder.Entity<Ban>()
//                .HasOptional(ban => ban.Channel);
//
//            modelBuilder.Entity<Ban>()
//                .HasOptional(ban => ban.Nick);
//
//            modelBuilder.Entity<Channel>()
//                .HasRequired(channel => channel.Server);
//
//            modelBuilder.Entity<Log>()
//                .HasRequired(log => log.Server);
//
//            modelBuilder.Entity<Log>()
//                .HasOptional(log => log.Account);
//
//            modelBuilder.Entity<Log>()
//                .HasOptional(log => log.Channel);
//
//            modelBuilder.Entity<Log>()
//                .HasOptional(log => log.Nick);
//
//            modelBuilder.Entity<Nick>()
//                .HasRequired(nick => nick.Server);
//
//            modelBuilder.Entity<Nick>()
//                .HasOptional(nick => nick.Account);
//
//            modelBuilder.Entity<Nick>()
//                .HasOptional(nick => nick.ChannelLastSeenIn);
//
//            base.OnModelCreating(modelBuilder);
//        }

        public override int SaveChanges()
        {
            try {
                bool saveFailed;
                do {
                    saveFailed = false;

                    try {
                        return base.SaveChanges();
                    } catch (DbUpdateConcurrencyException ex) {
                        saveFailed = true;

                        // Update the values of the entity that failed to save from the store
                        ex.Entries.Single().Reload();
                    }
                } while (saveFailed);
            } catch (DbEntityValidationException e) {
                var newException = new FormattedDbEntityValidationException(e);
                throw newException;
            }
            return 0;
        }

        public static void Obliterate()
        {
            db.Dispose();
        }

    }

}

