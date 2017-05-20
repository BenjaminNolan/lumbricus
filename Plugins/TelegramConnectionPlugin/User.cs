using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NLog;
using TwoWholeWorms.Lumbricus.Shared;

namespace TwoWholeWorms.Lumbricus.Plugins.TelegramConnectionPlugin.Model
{

    [Table("UserEntity")]
    public class User : IDisposable
    {
        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        #region Database members
        [Key]
        public long Id { get; set; }

        public long ServerId { get; set; }
        public Server Server { get; set; }

        public string GivenName { get; set; }
        public string FamilyName { get; set; } = null;

        public string UserName { get; set; } = null;
        public string LanguageCode { get; set; } = null;

        public DateTime FirstSeenAt { get; set; } = DateTime.Now;
        public DateTime LastSeenAt { get; set; } = DateTime.Now;

        public long? ChatLastSeenInId { get; set; } = null;
        public Chat ChatLastSeenIn { get; set; } = null;

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastModifiedAt { get; set; } = DateTime.Now;
        public bool IsOp { get; set; } = false;

        public virtual ICollection<Ban> Bans { get; set; }
        #endregion Database members

        public List<Chat> JoinedChats = new List<Chat>();

        bool disposed = false;
        public bool Disposed => disposed;


        public void AddChat(Chat chat, bool recurse = true)
        {
            if (!JoinedChats.Contains(chat)) {
                JoinedChats.Add(chat);
            }
            if (recurse) {
                chat.AddUser(this, false);
            }
        }

        public void RemoveChat(Chat chat, bool recurse = true)
        {
            if (JoinedChats.Contains(chat)) {
                JoinedChats.Remove(chat);
            }
            if (recurse) {
                chat.RemoveUser(this, false);
            }
        }

        public override string ToString()
        {
            return Id + ", "
                + GivenName + ", "
                + FamilyName + ", "
                + UserName + ", "
                + LanguageCode + ", "
                + IsActive + ", "
                + IsDeleted + ", "
                + CreatedAt + ", "
                + LastModifiedAt + ", "
                + IsOp;
        }

        #region IDisposable implementation
        ~User()
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
            if (disposed)
                return;
            disposed = true;

            // Unlink me from ALL THE THINGS!

            if (Server != null) {
                Server.RemoveUser(this);
            }
        }
        #endregion

        public static User FetchOrCreate(long userId, Server server)
        {
            User user = Fetch(userId, server) ?? Create(userId, server);
            return user;
        }

        public static User Create(long userId, Server server)
        {
            logger.Debug("Creating new user `" + userId + "` for server id `" + server.Id + "`");

            User user = new User() {
                Id = userId,
                Server = server,
                ServerId = server.Id,
            };

            TelegramPluginContext.db.Users.Add(user);
            TelegramPluginContext.db.SaveChanges();

            logger.Debug("Account `" + userId + "` created");

            return user;
        }

        public static User Fetch(long userId, Server server)
        {
            return (from a in TelegramPluginContext.db.Users
                    where (a.Id == userId && a.Server.Id == server.Id)
                    select a).FirstOrDefault();
        }


    }

}
