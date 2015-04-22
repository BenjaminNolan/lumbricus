using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TwoWholeWorms.Lumbricus.Shared.Model
{

    public class Nick : IDisposable
	{

        #region Database members
        [Key]
        public long     Id             { get; set; }

        [Required]
        public string   Name           { get; set; }
        [Required]
        public string   DisplayNick    { get; set; }

        public bool     IsActive       { get; set; } = true;
        public bool     IsDeleted      { get; set; } = false;
        public DateTime CreatedAt      { get; set; } = DateTime.Now;
        public DateTime LastModifiedAt { get; set; } = DateTime.Now;
        public string   User           { get; set; }
        public string   Host           { get; set; }

        public virtual Server Server { get; set; }
        public virtual Account Account { get; set; }
        #endregion Database members

        public List<Channel> channels = new List<Channel>();

        bool disposed = false;
        public bool Disposed => disposed;

        public void AddChannel(Channel channel, bool recurse = true)
        {
            if (!channels.Contains(channel)) {
                channels.Add(channel);
            }
            if (recurse) {
                channel.AddNick(this, false);
            }
        }

        public void RemoveChannel(Channel channel, bool recurse = true)
        {
            if (channels.Contains(channel)) {
                channels.Remove(channel);
            }
            if (recurse) {
                channel.RemoveNick(this, false);
            }
        }

        #region IDisposable implementation
        ~Nick()
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
                Server.RemoveNick(this);
            }

            foreach (Channel channel in channels) {
                channel.RemoveNick(this);
            }
            
            if (Account != null) {
                Server.RemoveAccount(Account);
                if (!Account.Disposed) Account.Dispose();
            }
        }
        #endregion

        public static Nick FetchOrCreate(string nickName, Server server)
        {
            return Fetch(nickName, server) ?? Create(nickName, server);
        }

        public static Nick Fetch(long nickId)
        {
            return (from n in LumbricusContext.db.Nicks
                where n.Id == nickId
                select n).FirstOrDefault();
        }

        public static Nick Fetch(string nickName, Server server)
        {
            return (from n in LumbricusContext.db.Nicks
                where n.Name == nickName && n.Server != null && n.Server.Id == server.Id
                select n).FirstOrDefault();
        }

        public static Nick Create(string nickName, Server server)
        {
            Nick nick = LumbricusContext.db.Nicks.Create();
            nick.Name = nickName;
            nick.Server = server;

            LumbricusContext.db.Nicks.Attach(nick);
            LumbricusContext.db.SaveChanges();
            return nick;
        }

	}

}
