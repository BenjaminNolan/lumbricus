using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NLog;

namespace TwoWholeWorms.Lumbricus.Plugins.IrcConnectionPlugin.Model
{

    [Table("Nick")]
    public class Nick : IDisposable
	{

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region Database members
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public         long              Id                  { get; set; }

        public         long              ServerId            { get; set; }
        public         Server            Server              { get; set; }
        
        public         string            Name                { get; set; }
        public         string            DisplayName         { get; set; }

        public         string            UserName            { get; set; } = null;
        public         string            Host                { get; set; } = null;

        public         long?             AccountId           { get; set; } = null;
        public         Account           Account             { get; set; } = null;

        public         DateTime          FirstSeenAt         { get; set; } = DateTime.Now;
        public         DateTime          LastSeenAt          { get; set; } = DateTime.Now;

        public         long?             ChannelLastSeenInId { get; set; } = null;
        public         Channel           ChannelLastSeenIn   { get; set; } = null;

        public         bool              IsActive            { get; set; } = true;
        public         bool              IsDeleted           { get; set; } = false;
        public         DateTime          CreatedAt           { get; set; } = DateTime.Now;
        public         DateTime          LastModifiedAt      { get; set; } = DateTime.Now;

        public virtual ICollection<Ban>  Bans                { get; set; }
        #endregion Database members

        public List<Channel> ConnectedChannels = new List<Channel>();

        bool disposed = false;
        public bool Disposed => disposed;

        public void AddChannel(Channel channel, bool recurse = true)
        {
            if (!ConnectedChannels.Contains(channel)) {
                ConnectedChannels.Add(channel);
            }
            if (recurse) {
                channel.AddNick(this, false);
            }
        }

        public void RemoveChannel(Channel channel, bool recurse = true)
        {
            if (ConnectedChannels.Contains(channel)) {
                ConnectedChannels.Remove(channel);
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

            foreach (Channel channel in ConnectedChannels) {
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
            return (from n in IrcPluginContext.db.Nicks
                where n.Id == nickId
                select n).FirstOrDefault();
        }

        public static IQueryable<Nick> FetchByAccountId(long accountId)
        {
            return (from n in IrcPluginContext.db.Nicks
                where n.Account != null && n.Account.Id == accountId
                select n);
        }

        public static Nick Fetch(string nickName, Server server)
        {
            return (from n in IrcPluginContext.db.Nicks
                where n.Name == nickName && n.Server != null && n.Server.Id == server.Id
                select n).FirstOrDefault();
        }

        public static Nick Create(string nickName, Server server)
        {
            logger.Debug("Creating new nick `" + nickName + "` to server id `" + server.Id + "`");

            Nick nick = new Nick() {
                Name = nickName.ToLower(),
                DisplayName = nickName,
                Server = server,
            };

            IrcPluginContext.db.Nicks.Add(nick);
            IrcPluginContext.db.SaveChanges();

            return nick;
        }

	}

}
