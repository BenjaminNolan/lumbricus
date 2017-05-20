using NLog;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using System.Linq;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Shared.Exceptions;
using TwoWholeWorms.Lumbricus.Plugins.TelegramConnectionPlugin.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.TelegramConnectionPlugin
{

    class TelegramPluginContext : DbContext
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public static TelegramPluginContext db;

        public DbSet<Ban> Bans { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<User> Users { get; set; }

        public TelegramPluginContext() : base()
        {
            // …
        }

        public static void Initialise(LumbricusConfiguration config)
        {
            if (db == null) {
                logger.Info("Initialising TelegramConnectionPlugin database context");
                db = new TelegramPluginContext();
            } else {
                logger.Info("Skipping TelegramConnectionPlugin database context initialisation; already done");
            }
        }

        public override int SaveChanges()
        {
            try {
                bool saveFailed;
                do {
                    saveFailed = false;

                    try {
                        logger.Debug("Saving changes.");
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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Server>()
               .HasKey<long>(s => s.Id);

            modelBuilder.Entity<Chat>()
                .HasKey<long>(c => c.Id);

            modelBuilder.Entity<User>()
                .HasKey<long>(a => a.Id);

            modelBuilder.Entity<Ban>()
                .HasKey<long>(b => b.Id);

            modelBuilder.Entity<Setting>()
                .HasKey<long>(s => s.Id);

            modelBuilder.Entity<Server>()
                .Property(s => s.Domain)
                .IsRequired()
                .HasMaxLength(128);

            modelBuilder.Entity<Server>()
                .Property(s => s.AccessToken)
                .IsRequired();

            /*modelBuilder.Entity<Ban>()
                .Property(b => b.BanMessage)
                .IsOptional()
                .HasMaxLength(512);

            modelBuilder.Entity<Ban>()
                .Property(b => b.UnbanMessage)
                .IsOptional()
                .HasMaxLength(512);*/

            modelBuilder.Entity<Server>()
                .HasMany<User>(s => s.Users)
                .WithRequired(a => a.Server)
                .HasForeignKey(a => a.ServerId);

            modelBuilder.Entity<Server>()
                .HasMany<Chat>(s => s.Chats)
                .WithRequired(c => c.Server)
                .HasForeignKey(c => c.ServerId);

            modelBuilder.Entity<Server>()
                .HasMany<Ban>(s => s.Bans)
                .WithRequired(b => b.Server)
                .HasForeignKey(b => b.ServerId);

            modelBuilder.Entity<User>()
                .HasOptional<Chat>(a => a.ChatLastSeenIn);
            //                .WithMany()
            //                .HasForeignKey(a => a.ChatLastSeenInId);

            modelBuilder.Entity<User>()
                .HasMany<Ban>(a => a.Bans)
                .WithRequired(b => b.User)
                .HasForeignKey(b => b.UserId);

            modelBuilder.Entity<Chat>()
                .HasMany<Ban>(c => c.Bans)
                .WithRequired(b => b.Chat)
                .HasForeignKey(b => b.ChatId);

            base.OnModelCreating(modelBuilder);
        }

    }

}
