using NLog;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using System.Linq;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Shared.Exceptions;

namespace TwoWholeWorms.Lumbricus.Shared
{
    
    public class LumbricusContext : DbContext
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();
        
        public static LumbricusContext db;

        public DbSet<Setting> Settings { get; set; }

        public LumbricusContext() : base()
        {
            // …
        }

        public static void Initialise(LumbricusConfiguration config)
        {
            if (db == null) {
                logger.Info("Initialising Lumbricus database context");
                db = new LumbricusContext();
            } else {
                logger.Info("Skipping database initialisation; already done");
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
            modelBuilder.Entity<Setting>()
                .HasKey<long>(s => s.Id);

            modelBuilder.Entity<Setting>()
                .Property(s => s.Section)
                .IsRequired()
                .HasMaxLength(32);

            modelBuilder.Entity<Setting>()
                .Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(32);

            modelBuilder.Entity<Setting>()
                .Property(s => s.Value)
                .IsRequired()
                .HasMaxLength(512);

            modelBuilder.Entity<Setting>()
                .Property(s => s.DefaultValue)
                .IsRequired()
                .HasMaxLength(512);

            base.OnModelCreating(modelBuilder);
        }

    }

}
