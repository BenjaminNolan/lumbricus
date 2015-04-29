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
        public long       Id                  { get; set; }

        public long       ServerId            { get; set; }
        [Required]
        [ForeignKey("Id")]
        public Server     Server              { get; set; }
        
        [Required]
        [MaxLength(32)]
        public string     Name                { get; set; }
        [Required]
        [MaxLength(32)]
        public string     DisplayName         { get; set; }
        [MaxLength(32)]
        public string     UserName            { get; set; }
        [MaxLength(128)]
        public string     Host                { get; set; }

        public long?      MostRecentNickId    { get; set; } = null;
        [ForeignKey("Id")]
        public Nick       MostRecentNick      { get; set; } = null;
        
        public DateTime   FirstSeenAt         { get; set; } = DateTime.Now;
        public DateTime   LastSeenAt          { get; set; } = DateTime.Now;

        public long?      ChannelLastSeenInId { get; set; } = null;
        [ForeignKey("Id")]
        public Channel    ChannelLastSeenIn   { get; set; } = null;

        public bool       IsActive            { get; set; } = true;
        public bool       IsDeleted           { get; set; } = false;
        public DateTime   CreatedAt           { get; set; } = DateTime.Now;
        public DateTime   LastModifiedAt      { get; set; } = DateTime.Now;
        public bool       IsOp                { get; set; } = false;
        
        public ICollection<Nick> Nicks        { get; set; }
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
            Account account = new Account() {
                Name = accountName.ToLower(),
                DisplayName = accountName,
                Server = server,
            };

            LumbricusContext.db.Accounts.Attach(account);
            LumbricusContext.db.SaveChanges();

            return account;
        }

        public static Account FetchByNickId(long nickId)
        {
            return (from a in LumbricusContext.db.Accounts
                where a.MostRecentNick != null && a.MostRecentNick.Id == nickId
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
