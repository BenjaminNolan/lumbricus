using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TwoWholeWorms.Lumbricus.Shared.Model
{
    
    public class Server : IDisposable
	{

        #region Database members
        [Key]
        public long     Id              { get; protected set; }

        [Required]
        public string   Host            { get; protected set; }
        [Required]
        public int      Port            { get; protected set; }
        [Required]
        public string   BotNick         { get; protected set; }
        [Required]
        public string   BotUserName     { get; protected set; }
        [Required]
        public string   BotNickPassword { get; protected set; }
        [Required]
        public string   BotRealName     { get; protected set; }
        [Required]
        public string   NickServNick    { get; protected set; }
        [Required]
        public string   NickServHost    { get; protected set; }

        public bool     AutoConnect     { get; protected set; } = true;
        public bool     IsActive        { get; protected set; } = true;
        public bool     IsDeleted       { get; protected set; } = false;
        public DateTime CreatedAt       { get; protected set; } = DateTime.Now;
        public DateTime LastModifiedAt  { get; protected set; } = DateTime.Now;
        #endregion Database members

        protected List<Channel> channels = new List<Channel>();
        public List<Channel> Channels => channels;

        protected List<Nick> nicks = new List<Nick>();
        public List<Nick> Nicks => nicks;

        protected List<Account> accounts = new List<Account>();
        public List<Account> Accounts => accounts;

        bool disposed = false;
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
