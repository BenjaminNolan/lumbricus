using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TwoWholeWorms.Lumbricus.Shared.Model
{

    public class Channel : IDisposable
	{
        
        #region Database members
        [Key]
        public long     Id                     { get; set; }

        [Required]
        public long     ServerId               { get; set; }

        [Required]
        [ForeignKey("ServerId")]
        public virtual  Server Server          { get; set; }
        
        [Required]
        public string   Name                   { get; set; }

        public bool     AutoJoin               { get; set; } = true;
        public bool     AllowCommandsInChannel { get; set; } = false;

        public DateTime CreatedAt              { get; set; } = DateTime.Now;
        public DateTime LastModifiedAt         { get; set; } = DateTime.Now;
        public bool     IsActive               { get; set; } = true;
        public bool     IsDeleted              { get; set; } = false;
        #endregion Database members

        public List<Nick> Nicks = new List<Nick>();

        bool disposed = false;
        public bool Disposed => disposed;

        public void AddNick(Nick nick, bool recurse = true)
        {
            if (!Nicks.Contains(nick)) {
                Nicks.Add(nick);
            }
            if (recurse) {
                nick.AddChannel(this, false);
            }
        }

        public void RemoveNick(Nick nick, bool recurse = true)
        {
            if (Nicks.Contains(nick)) {
                Nicks.Remove(nick);
            }
            if (recurse) {
                nick.RemoveChannel(this, false);
            }
        }

        #region IDisposable implementation
        ~Channel()
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
                Server.RemoveChannel(this);
            }

            foreach (Nick nick in Nicks) {
                nick.RemoveChannel(this);
                if (nick.channels.Count <= 0) {
                    if (!nick.Disposed) nick.Dispose();
                }
            }
        }
        #endregion

        public static Channel FetchOrCreate(string channelName, Server server)
        {
            return Fetch(channelName, server) ?? Create(channelName, server);
        }

        public static Channel Create(string channelName, Server server)
        {
            Channel channel = LumbricusContext.db.Channels.Create();
            channel.Name = channelName;
            channel.Server = server;

            LumbricusContext.db.Channels.Attach(channel);
            LumbricusContext.db.SaveChanges();
            return channel;
        }

        public static Channel Fetch(long channelId)
        {
            return (from c in LumbricusContext.db.Channels
                    where c.Id == channelId
                    select c).FirstOrDefault();
        }

        public static Channel Fetch(string channelName, Server server = null)
        {
            return (from c in LumbricusContext.db.Channels
                    where c.Name == channelName
                        && server != null
                        && c.Server != null
                        && c.Server.Id == server.Id
                    select c).FirstOrDefault();
        }

        public static IQueryable<Channel> Fetch(Server server)
        {
            return (from c in LumbricusContext.db.Channels
                    where c.Server.Id == server.Id
                        && c.IsDeleted == false
                    select c);
        }

	}

}
