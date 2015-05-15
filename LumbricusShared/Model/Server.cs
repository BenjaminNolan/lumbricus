﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoWholeWorms.Lumbricus.Shared.Model
{

    [Table("Server")]
    public class Server : IDisposable
	{

        #region Database members
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long                 Id              { get; set; }

        public string               Host            { get; set; }
        public int                  Port            { get; set; }
        public string               BotNick         { get; set; }
        public string               BotNickPassword { get; set; }
        public string               BotUserName     { get; set; }
        public string               BotRealName     { get; set; }
        public string               NickServNick    { get; set; }
        public string               NickServHost    { get; set; }

        public bool                 AutoConnect     { get; set; } = true;
        public bool                 IsActive        { get; set; } = true;
        public bool                 IsDeleted       { get; set; } = false;
        public DateTime             CreatedAt       { get; set; } = DateTime.Now;
        public DateTime             LastModifiedAt  { get; set; } = DateTime.Now;

        public ICollection<Account> ServerAccounts  { get; set; }
        public ICollection<Channel> ServerChannels  { get; set; }
        public ICollection<Nick>    ServerNicks     { get; set; }
        public ICollection<Ban>     Bans            { get; set; }
        #endregion Database members

        [NotMapped]
        protected List<Channel> connectedChannels = new List<Channel>();
        [NotMapped]
        public List<Channel> ConnectedChannels => connectedChannels;

        [NotMapped]
        protected List<Account> connectedAccounts = new List<Account>();
        [NotMapped]
        public List<Account> ConnectedAccounts => connectedAccounts;

        [NotMapped]
        protected List<Nick> connectedNicks = new List<Nick>();
        [NotMapped]
        public List<Nick> ConnectedNicks => connectedNicks;

        [NotMapped]
        bool disposed = false;
        [NotMapped]
        public bool Disposed => disposed;

        public void SetChannels(List<Channel> channels)
        {
            if (connectedChannels != null) {
                this.connectedChannels = channels;
            }
        }

        public void AddChannel(Channel channel)
        {
            if (!connectedChannels.Contains(channel)) {
                connectedChannels.Add(channel);
            }
        }

        public void RemoveChannel(Channel channel)
        {
            if (connectedChannels.Contains(channel)) {
                connectedChannels.Remove(channel);
            }
        }

        public void SetNicks(List<Nick> nicks)
        {
            if (nicks != null) {
                connectedNicks = nicks;
            }
        }

        public void AddNick(Nick nick)
        {
            if (!connectedNicks.Contains(nick)) {
                connectedNicks.Add(nick);
            }
        }

        public void RemoveNick(Nick nick, bool recurse = true)
        {
            if (connectedNicks.Contains(nick)) {
                connectedNicks.Remove(nick);
            }
            if (recurse) {
                foreach (Channel channel in connectedChannels) {
                    channel.RemoveNick(nick);
                }
            }
        }

        public void AddAccount(Account account)
        {
            if (!connectedAccounts.Contains(account)) {
                connectedAccounts.Add(account);
            }
        }

        public void RemoveAccount(Account account)
        {
            if (connectedAccounts.Contains(account)) {
                connectedAccounts.Remove(account);
                account.Dispose();
            }
        }

        public void SetAccounts(List<Account> accounts)
        {
            if (accounts != null) {
                connectedAccounts = accounts;
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

            foreach (Nick n in connectedNicks) {
                if (!n.Disposed) n.Dispose();
            }

            foreach (Channel channel in connectedChannels) {
                if (!channel.Disposed) channel.Dispose();
            }

            foreach (Account account in connectedAccounts) {
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
