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

        public LumbricusContext() : base()
        {
            // …
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Ban>     Bans     { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Log>     Logs     { get; set; }
        public DbSet<Nick>    Nicks    { get; set; }
        public DbSet<Server>  Servers  { get; set; }
        public DbSet<Setting> Settings { get; set; }

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
            
            modelBuilder.Entity<Channel>()
                .HasKey<long>(c => c.Id);

            modelBuilder.Entity<Log>()
                .HasKey<long>(l => l.Id);

            modelBuilder.Entity<Account>()
                .HasKey<long>(a => a.Id);

            modelBuilder.Entity<Nick>()
                .HasKey<long>(n => n.Id);

            modelBuilder.Entity<Ban>()
                .HasKey<long>(b => b.Id);

            modelBuilder.Entity<Setting>()
                .HasKey<long>(s => s.Id);

            modelBuilder.Entity<Server>()
                .Property(s => s.Host)
                .IsRequired()
                .HasMaxLength(128);
            
            modelBuilder.Entity<Server>()
                .Property(s => s.Port)
                .IsRequired();
            
            modelBuilder.Entity<Server>()
                .Property(s => s.BotNick)
                .IsRequired()
                .HasMaxLength(32);
            
            modelBuilder.Entity<Server>()
                .Property(s => s.BotNickPassword)
                .IsRequired()
                .HasMaxLength(64);
            
            modelBuilder.Entity<Server>()
                .Property(s => s.BotUserName)
                .IsRequired()
                .HasMaxLength(32);
            
            modelBuilder.Entity<Server>()
                .Property(s => s.BotRealName)
                .IsRequired()
                .HasMaxLength(128);
            
            modelBuilder.Entity<Server>()
                .Property(s => s.NickServNick)
                .IsRequired()
                .HasMaxLength(32);
            
            modelBuilder.Entity<Server>()
                .Property(s => s.NickServHost)
                .IsRequired()
                .HasMaxLength(128);
            
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

            modelBuilder.Entity<Account>()
                .Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(32);
            
            modelBuilder.Entity<Account>()
                .Property(a => a.DisplayName)
                .IsRequired()
                .HasMaxLength(32);

            modelBuilder.Entity<Account>()
                .Property(a => a.UserName)
                .IsOptional()
                .HasMaxLength(32);

            modelBuilder.Entity<Account>()
                .Property(a => a.Host)
                .IsOptional()
                .HasMaxLength(128);

            modelBuilder.Entity<Ban>()
                .Property(b => b.BanMessage)
                .IsOptional()
                .HasMaxLength(512);

            modelBuilder.Entity<Ban>()
                .Property(b => b.UnbanMessage)
                .IsOptional()
                .HasMaxLength(512);

            modelBuilder.Entity<Nick>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(32);

            modelBuilder.Entity<Nick>()
                .Property(p => p.DisplayName)
                .IsRequired()
                .HasMaxLength(32);

            modelBuilder.Entity<Nick>()
                .Property(p => p.UserName)
                .IsOptional()
                .HasMaxLength(32);

            modelBuilder.Entity<Nick>()
                .Property(p => p.Host)
                .IsOptional()
                .HasMaxLength(128);

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

            modelBuilder.Entity<Server>()
                .HasMany<Account>(s => s.ServerAccounts)
                .WithRequired(a => a.Server)
                .HasForeignKey(a => a.ServerId);

            modelBuilder.Entity<Server>()
                .HasMany<Nick>(s => s.ServerNicks)
                .WithRequired(n => n.Server)
                .HasForeignKey(n => n.ServerId);

            modelBuilder.Entity<Server>()
                .HasMany<Channel>(s => s.ServerChannels)
                .WithRequired(c => c.Server)
                .HasForeignKey(c => c.ServerId);

            modelBuilder.Entity<Server>()
                .HasMany<Ban>(s => s.Bans)
                .WithRequired(b => b.Server)
                .HasForeignKey(b => b.ServerId);

            modelBuilder.Entity<Account>()
                .HasOptional<Nick>(a => a.MostRecentNick);
//                .WithMany()
//                .HasForeignKey(a => a.MostRecentNickId);

            modelBuilder.Entity<Account>()
                .HasOptional<Channel>(a => a.ChannelLastSeenIn);
//                .WithMany()
//                .HasForeignKey(a => a.ChannelLastSeenInId);

            modelBuilder.Entity<Account>()
                .HasMany<Nick>(a => a.Nicks)
                .WithOptional(n => n.Account)
                .HasForeignKey(n => n.AccountId);

            modelBuilder.Entity<Account>()
                .HasMany<Ban>(a => a.Bans)
                .WithRequired(b => b.Account)
                .HasForeignKey(b => b.AccountId);

            modelBuilder.Entity<Channel>()
                .HasMany<Ban>(c => c.Bans)
                .WithRequired(b => b.Channel)
                .HasForeignKey(b => b.ChannelId);

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

//            modelBuilder.Entity<Nick>()
//                .HasOptional<Account>(n => n.Account)
//                .WithMany(a => a.Nicks)
//                .HasForeignKey(n => n.AccountId);
            
            modelBuilder.Entity<Nick>()
                .HasOptional<Channel>(n => n.ChannelLastSeenIn)
                .WithMany()
                .HasForeignKey(n => n.ChannelLastSeenInId);

            modelBuilder.Entity<Nick>()
                .HasMany<Ban>(n => n.Bans)
                .WithRequired(b => b.Nick)
                .HasForeignKey(b => b.NickId);

            base.OnModelCreating(modelBuilder);
        }

    }

}
