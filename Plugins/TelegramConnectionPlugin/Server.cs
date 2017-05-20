using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoWholeWorms.Lumbricus.Plugins.TelegramConnectionPlugin.Model
{

    [Table("Server")]
    public class Server : IDisposable
    {

        #region Database members
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public         string               Name            { get; set; }
        public string Domain { get; set; }
        public string AccessToken { get; set; }

        public bool AutoConnect { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastModifiedAt { get; set; } = DateTime.Now;

        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Chat> Chats { get; set; }
        public virtual ICollection<Ban> Bans { get; set; }
        #endregion Database members

        [NotMapped]
        protected List<User> connectedUsers;
        [NotMapped]
        public List<User> ConnectedUsers => connectedUsers;

        [NotMapped]
        protected List<Chat> connectedChats;
        [NotMapped]
        public List<Chat> ConnectedChats => connectedChats;
        
        [NotMapped]
        bool disposed;
        [NotMapped]
        public bool Disposed => disposed;

        public Server()
        {
            connectedUsers = new List<User>();
            connectedChats = new List<Chat>();

            disposed = false;
        }

        public void SetChats(List<Chat> chats)
        {
            if (connectedChats != null) {
                connectedChats = chats;
            }
        }

        public void AddChat(Chat chat)
        {
            if (!connectedChats.Contains(chat)) {
                connectedChats.Add(chat);
            }
        }

        public void RemoveChat(Chat chat)
        {
            if (connectedChats.Contains(chat)) {
                connectedChats.Remove(chat);
            }
        }
        
        public void AddUser(User user)
        {
            if (!connectedUsers.Contains(user)) {
                connectedUsers.Add(user);
            }
        }

        public void RemoveUser(User user)
        {
            if (connectedUsers.Contains(user)) {
                connectedUsers.Remove(user);
                user.Dispose();
            }
        }

        public void SetUsers(List<User> users)
        {
            if (users != null) {
                connectedUsers = users;
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
            if (disposed)
                return;
            disposed = true;

            // Unlink me from ALL THE THINGS!

            foreach (Chat chat in connectedChats) {
                if (!chat.Disposed)
                    chat.Dispose();
            }

            foreach (User user in connectedUsers) {
                if (!user.Disposed)
                    user.Dispose();
            }
        }
        #endregion

        public static IQueryable<Server> Fetch()
        {
            return (from s in TelegramPluginContext.db.Servers
                    where s.IsDeleted == false
                    select s);
        }

        public static Server Fetch(long serverId)
        {
            return (from s in TelegramPluginContext.db.Servers
                    where s.Id == serverId
                    select s).FirstOrDefault();
        }

    }

}
