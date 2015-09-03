using NLog;
using System.Data.Entity;
using TwoWholeWorms.Lumbricus.Plugins.IrcLogPlugin.Model;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.IrcLogPlugin
{

    public class IrcLogContext : DbContext
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public static IrcLogContext db;

        public DbSet<Log> Logs { get; set; }

        public static void Initialise(LumbricusConfiguration config)
        {
            if (db == null) {
                logger.Debug("Initialising InfoContext");
                db = new IrcLogContext();
            } else {
                logger.Debug("Skipping database initialisation; already connected");
            }
        }

        public static void Obliterate()
        {
            db.Dispose();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Log>()
                .HasKey<long>(l => l.Id);

            modelBuilder.Entity<Log>()
                .Property(l => l.Trail)
                .IsOptional()
                .HasMaxLength(512);

            modelBuilder.Entity<Log>()
                .Property(l => l.Line)
                .IsOptional()
                .HasMaxLength(512);

            modelBuilder.Entity<Log>()
                .Property(l => l.IrcCommand)
                .IsRequired();

            modelBuilder.Entity<Log>()
                .HasRequired<Server>(l => l.Server)
                .WithMany()
                .HasForeignKey(l => l.ServerId);

            modelBuilder.Entity<Log>()
                .HasOptional<Account>(l => l.Account)
                .WithMany()
                .HasForeignKey(l => l.AccountId);

            modelBuilder.Entity<Log>()
                .HasOptional<Channel>(l => l.Channel)
                .WithMany()
                .HasForeignKey(l => l.ChannelId);

            modelBuilder.Entity<Log>()
                .HasOptional<Nick>(l => l.Nick)
                .WithMany()
                .HasForeignKey(l => l.NickId);

            base.OnModelCreating(modelBuilder);
        }

    }

}
