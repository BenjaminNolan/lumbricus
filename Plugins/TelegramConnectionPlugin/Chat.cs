using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TwoWholeWorms.Lumbricus.Plugins.TelegramConnectionPlugin.Model
{

    [Table("Chat")]
    public class Chat : IDisposable
    {

        public enum ChatType
        {
            PRIVATE,
            GROUP,
            SUPERGROUP,
            CHANNEL
        }

        #region Database members
        [Key]
        public long Id { get; set; }

        public long ServerId { get; set; }
        public Server Server { get; set; }

        public string Name { get; set; }
        public string Title { get; set; } = null;
        public string Username { get; set; } = null;
        public string GivenName { get; set; } = null;
        public string FamilyName { get; set; } = null;
        public bool AllMembersAreAdministrators { get; set; } = false;

        public bool AutoJoin { get; set; } = true;
        public bool AllowCommandsInChat { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastModifiedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public ICollection<Ban> Bans { get; set; }
        #endregion Database members

        public List<User> ConnectedUsers = new List<User>();

        bool disposed = false;
        public bool Disposed => disposed;

        public void AddUser(User user, bool recurse = true)
        {
            if (!ConnectedUsers.Contains(user)) {
                ConnectedUsers.Add(user);
            }
            if (recurse) {
                user.AddChat(this, false);
            }
        }

        public void RemoveUser(User user, bool recurse = true)
        {
            if (ConnectedUsers.Contains(user)) {
                ConnectedUsers.Remove(user);
            }
            if (recurse) {
                user.RemoveChat(this, false);
            }
        }

        #region IDisposable implementation
        ~Chat()
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
                Server.RemoveChat(this);
            }

            foreach (User user in ConnectedUsers) {
                user.RemoveChat(this);
                if (user.JoinedChats.Count <= 0) {
                    if (!user.Disposed)
                        user.Dispose();
                }
            }
        }
        #endregion

        public static Chat FetchOrCreate(long chatId, Server server)
        {
            return Fetch(chatId, server) ?? Create(chatId, server);
        }

        public static Chat Create(long chatId, Server server)
        {
            Chat chat = new Chat() {
                Id = chatId,
                Server = server,
            };

            TelegramPluginContext.db.Chats.Add(chat);
            TelegramPluginContext.db.SaveChanges();

            return chat;
        }

        public static Chat Fetch(long chatId, Server server = null)
        {
            return (from c in TelegramPluginContext.db.Chats
                    where c.Id == chatId
                        && c.Server == server
                    select c).FirstOrDefault();
        }

        public static IQueryable<Chat> Fetch(Server server)
        {
            return (from c in TelegramPluginContext.db.Chats
                    where c.Server.Id == server.Id
                        && c.IsDeleted == false
                    select c);
        }

    }

}
