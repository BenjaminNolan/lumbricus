using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TwoWholeWorms.Lumbricus.Shared;

namespace TwoWholeWorms.Lumbricus.Shared.Model
{

    public class Account : IDisposable
	{
        #region Database members
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long     Id             { get; set; }

        [Required]
        [ForeignKey("Server")]
        public long ServerId { get; set; }
        public virtual  Server Server  { get; set; }
        
        [Required]
        public string   Name           { get; set; }
        public string   UserName       { get; set; }
        public string   Host           { get; set; }

        [ForeignKey("PrimaryNick")]
        public long PrimaryNickId { get; set; }
        public virtual Nick   PrimaryNick { get; set; }
        
        public DateTime FirstSeenAt    { get; set; } = DateTime.Now;
        public DateTime LastSeenAt     { get; set; } = DateTime.Now;
        
        [ForeignKey("ChannelLastSeenIn")]
        public long ChannelLastSeenInId { get; set; }
        public virtual Channel ChannelLastSeenIn { get; set; }

        public bool     IsActive       { get; set; } = true;
        public bool     IsDeleted      { get; set; } = false;
        public DateTime CreatedAt      { get; set; } = DateTime.Now;
        public DateTime LastModifiedAt { get; set; } = DateTime.Now;
        public bool     IsOp           { get; set; } = false;
        
        public virtual List<Nick> Nicks { get; set; }
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

            if (PrimaryNick != null) {
                Server.RemoveNick(PrimaryNick);
                foreach (Channel channel in JoinedChannels) {
                    channel.RemoveNick(PrimaryNick);
                }
                if (!PrimaryNick.Disposed) PrimaryNick.Dispose();
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
            Account account = LumbricusContext.db.Accounts.Create();
            account.Name = accountName;
            account.Server = server;

            LumbricusContext.db.Accounts.Attach(account);
            LumbricusContext.db.SaveChanges();

            return account;
        }

        public static Account FetchByNickId(long nickId)
        {
            return (from a in LumbricusContext.db.Accounts
                where a.PrimaryNick != null && a.PrimaryNick.Id == nickId
                select a).FirstOrDefault();
        }

        public static Account Fetch(long accountId)
        {
            if (accountId < 1) return null;

            return (from a in LumbricusContext.db.Accounts
                    where (a.Id == accountId)
                    select a).FirstOrDefault();
        }

        public static Account Fetch(string accountName, Server server)
        {
            return (from a in LumbricusContext.db.Accounts
                    where (a.Name == accountName && a.Server.Id == server.Id)
                    select a).FirstOrDefault();
        }

	}

}
