using NLog;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoWholeWorms.Lumbricus.Plugins.IrcConnectionPlugin.Model
{

    [Table("Account")]
    public class Account : IDisposable
	{
        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region Database members
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public         long              Id                  { get; set; }

        public         long              ServerId            { get; set; }
        public         Server            Server              { get; set; }

        public         string            Name            { get; set; }
        public         string            DisplayName         { get; set; }

        public         string            UserName            { get; set; } = null;
        public         string            Host                { get; set; } = null;

        public         long?             MostRecentNickId    { get; set; } = null;
        public         Nick              MostRecentNick      { get; set; } = null;
        
        public         DateTime          FirstSeenAt         { get; set; } = DateTime.Now;
        public         DateTime          LastSeenAt          { get; set; } = DateTime.Now;

        public         long?             ChannelLastSeenInId { get; set; } = null;
        public         Channel           ChannelLastSeenIn   { get; set; } = null;

        public         bool              IsActive            { get; set; } = true;
        public         bool              IsDeleted           { get; set; } = false;
        public         DateTime          CreatedAt           { get; set; } = DateTime.Now;
        public         DateTime          LastModifiedAt      { get; set; } = DateTime.Now;
        public         bool              IsOp                { get; set; } = false;

        public virtual ICollection<Ban>  Bans                { get; set; }
        public virtual ICollection<Nick> Nicks               { get; set; }
        #endregion Database members

        public List<Channel> JoinedChannels = new List<Channel>();

        bool disposed = false;
        public bool Disposed => disposed;

        public void AddNick(Nick nick, bool recurse = true)
        {
            if (!Nicks.Contains(nick)) {
                Nicks.Add(nick);
            }
        }

        public override string ToString()
        {
            return Id + ", "
                + Name + ", "
                + IsActive + ", "
                + IsDeleted + ", "
                + CreatedAt + ", "
                + LastModifiedAt + ", "
                + IsOp;
        }

        #region IDisposable implementation
        ~Account()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            disposed = true;

            // Unlink me from ALL THE THINGS!

            if (Server != null) {
                Server.RemoveAccount(this);
            }

            if (MostRecentNick != null) {
                Server.RemoveNick(MostRecentNick);
                foreach (Channel channel in JoinedChannels) {
                    channel.RemoveNick(MostRecentNick);
                }
                if (!MostRecentNick.Disposed) MostRecentNick.Dispose();
            }
        }
        #endregion

        public static Account FetchOrCreate(string accountName, Server server)
        {
            Account account = Fetch(accountName, server) ?? Create(accountName, server);
            return account;
        }

        public static Account Create(string accountName, Server server)
        {
            logger.Debug("Creating new account `" + accountName + "` to server id `" + server.Id + "`");

            Account account = new Account() {
                Name = accountName.ToLower(),
                DisplayName = accountName,
                Server = server,
                ServerId = server.Id,
            };

            IrcPluginContext.db.Accounts.Add(account);
            IrcPluginContext.db.SaveChanges();

            logger.Debug("Account `" + accountName + "` created with id `" + account.Id + "`");

            return account;
        }

        public static Account FetchByNickId(long nickId)
        {
            return (from a in IrcPluginContext.db.Accounts
                where a.MostRecentNick != null && a.MostRecentNick.Id == nickId
                select a).FirstOrDefault();
        }

        public static Account Fetch(long accountId)
        {
            if (accountId < 1) return null;

            return (from a in IrcPluginContext.db.Accounts
                    where (a.Id == accountId)
                    select a).FirstOrDefault();
        }

        public static Account Fetch(string accountName, Server server)
        {
            return (from a in IrcPluginContext.db.Accounts
                    where (a.Name == accountName && a.Server.Id == server.Id)
                    select a).FirstOrDefault();
        }

	}

}
