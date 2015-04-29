using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoWholeWorms.Lumbricus.Shared.Model
{
    
    public class Server : IDisposable
	{

        #region Database members
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long     Id              { get; set; }

        [Required]
        [MaxLength(128)]
        public string   Host            { get; set; }
        [Required]
        public int      Port            { get; set; }
        [Required]
        [MaxLength(32)]
        public string   BotNick         { get; set; }
        [Required]
        [MaxLength(64)]
        public string   BotNickPassword { get; set; }
        [Required]
        [MaxLength(32)]
        public string   BotUserName     { get; set; }
        [Required]
        [MaxLength(128)]
        public string   BotRealName     { get; set; }
        [Required]
        [MaxLength(32)]
        public string   NickServNick    { get; set; }
        [Required]
        [MaxLength(128)]
        public string   NickServHost    { get; set; }

        public bool     AutoConnect     { get; set; } = true;
        public bool     IsActive        { get; set; } = true;
        public bool     IsDeleted       { get; set; } = false;
        public DateTime CreatedAt       { get; set; } = DateTime.Now;
        public DateTime LastModifiedAt  { get; set; } = DateTime.Now;
        #endregion Database members

        [NotMapped]
        protected List<Channel> channels = new List<Channel>();
        [NotMapped]
        public List<Channel> Channels => channels;

        [NotMapped]
        protected List<Nick> nicks = new List<Nick>();
        [NotMapped]
        public List<Nick> Nicks => nicks;

        [NotMapped]
        protected List<Account> accounts = new List<Account>();
        [NotMapped]
        public List<Account> Accounts => accounts;

        [NotMapped]
        bool disposed = false;
        [NotMapped]
        public bool Disposed => disposed;

        public void SetChannels(List<Channel> channels)
        {
            if (channels != null) {
                this.channels = channels;
            }
        }

        public void AddChannel(Channel channel)
        {
            if (!channels.Contains(channel)) {
                channels.Add(channel);
            }
        }

        public void RemoveChannel(Channel channel)
        {
            if (channels.Contains(channel)) {
                channels.Remove(channel);
            }
        }

        public void SetNicks(List<Nick> nicks)
        {
            if (nicks != null) {
                this.nicks = nicks;
            }
        }

        public void AddNick(Nick nick)
        {
            if (!nicks.Contains(nick)) {
                nicks.Add(nick);
            }
        }

        public void RemoveNick(Nick nick, bool recurse = true)
        {
            if (nicks.Contains(nick)) {
                nicks.Remove(nick);
            }
            if (recurse) {
                foreach (Channel channel in channels) {
                    channel.RemoveNick(nick);
                }
            }
        }

        public void AddAccount(Account account)
        {
            if (!accounts.Contains(account)) {
                accounts.Add(account);
            }
        }

        public void RemoveAccount(Account account)
        {
            if (accounts.Contains(account)) {
                accounts.Remove(account);
                account.Dispose();
            }
        }

        public void SetAccounts(List<Account> accounts)
        {
            if (accounts != null) {
                this.accounts = accounts;
            }
        }

        #region IDisposable implementation
        ~Server()
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

            foreach (Nick n in nicks) {
                if (!n.Disposed) n.Dispose();
            }

            foreach (Channel channel in channels) {
                if (!channel.Disposed) channel.Dispose();
            }

            foreach (Account account in accounts) {
                if (!account.Disposed) account.Dispose();
            }
        }
        #endregion

        public static IQueryable<Server> Fetch()
        {
            return (from s in LumbricusContext.db.Servers
                where s.IsDeleted == false
                select s);
        }

        public static Server Fetch(long serverId)
        {
            return (from s in LumbricusContext.db.Servers
                where s.Id == serverId
                select s).FirstOrDefault();
        }

	}

}
