using System;
using System.Collections.Generic;
using System.Data.Entity;
using MySql.Data.MySqlClient;

namespace TwoWholeWorms.Lumbricus.Shared.Model
{

    public class Account : IDisposable
	{
        #region Database members
        public long id { get; protected set; }
        public string account { get; protected set; }
        public long serverId { get; protected set; }
        public bool isActive { get; protected set; }
        public bool isDeleted { get; protected set; }
        public DateTime createdAt { get; protected set; }
        public DateTime lastModifiedAt { get; protected set; }
        public long nickId { get; protected set; }
        public bool isOp { get; protected set; }
        
        public virtual Server server { get; protected set; }
        public virtual Nick currentNick { get; protected set; }
        #endregion Database members

        public List<Channel> channels = new List<Channel>();
        public List<Nick> nicks = new List<Nick>();

        public bool disposed { get; protected set; }
        
        public Account(MySqlDataReader reader, Server s, Nick n) : this(reader, s)
        {
            currentNick = n;
            nicks.Add(n);
        }

        public Account(MySqlDataReader reader, Server s) : this(reader)
        {
            server = s;
        }

        public Account(MySqlDataReader reader)
        {
            id = reader.GetInt64Safe("id");
            account = reader.GetStringSafe("account");
            serverId = reader.GetInt64Safe("server_id");
            isActive = (reader.GetInt64Safe("is_active") == 1);
            isDeleted = (reader.GetInt64Safe("is_deleted") == 1);
            createdAt = reader.GetDateTime("created_at");
            lastModifiedAt = reader.GetDateTime("last_modified_at");
            nickId = reader.GetInt64Safe("nick_id");
            isOp = (reader.GetInt64Safe("is_op") == 1);
        }

        public void SetServer(Server server)
        {
            this.server = server;
        }

        public void SetNick(Nick nick)
        {
            AddNick(nick, true, true);
        }

        public void AddNick(Nick nick, bool recurse = true, bool isCurrent = false)
        {
            if (!nicks.Contains(nick)) {
                nicks.Add(nick);
            }
            if (isCurrent) {
                currentNick = nick;
            }
            if (recurse) {
                nick.SetAccount(this, false);
            }
        }

        public void RemoveNick(Nick nick)
        {
            if (nicks.Contains(nick)) {
                nicks.Remove(nick);
            }
        }

        public override string ToString()
        {
            return id + ", "
                + account + ", "
                + serverId + ", "
                + isActive + ", "
                + isDeleted + ", "
                + createdAt + ", "
                + lastModifiedAt + ", "
                + nickId + ", "
                + isOp;
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

            if (server != null) {
                server.RemoveAccount(this);
            }

            if (currentNick != null) {
                server.RemoveNick(currentNick);
                foreach (Channel channel in channels) {
                    channel.RemoveNick(currentNick);
                }
                if (!currentNick.disposed) currentNick.Dispose();
            }
        }
        #endregion

        public void SaveNickLink()
        {
            if (currentNick == null) {
                throw new Exception("currentNick is null!");
            }
            using (MySqlCommand cmd = DataConnection.CreateDbCommand()) {
                cmd.CommandText = "UPDATE `accounts` SET `nick_id` = @nickId WHERE `id` = @accountId";
                cmd.Parameters.AddWithValue("@accountId", id);
                cmd.Parameters.AddWithValue("@nickId", currentNick.id);

                cmd.ExecuteNonQuery();
            }
        }

        public static Account FetchOrCreate(string accountName, Server server)
        {
            Account account = Fetch(accountName, server) ?? Create(accountName, server);
            return account;
        }

        public static Account Fetch(long accountId, Server server)
        {
            if (accountId < 1) return null;

            Account account = null;
            using (MySqlCommand cmd = DataConnection.CreateDbCommand()) {
                cmd.CommandText = "SELECT * FROM `accounts` WHERE `id` = @accountId;";
                cmd.Parameters.AddWithValue("@accountId", accountId);

                using (MySqlDataReader reader = cmd.ExecuteReader()) {
                    if (reader.Read()) {
                        account = new Account(reader, server);
                    }
                }
            }

            return account;
        }

        public static Account Fetch(string accountName, Server server)
        {
            Account account = null;

            using (MySqlCommand cmd = DataConnection.CreateDbCommand()) {
                cmd.CommandText = "SELECT * FROM `accounts` WHERE `account` = @accountName AND `server_id` = @serverId;";
                cmd.Parameters.AddWithValue("@accountName", accountName);
                cmd.Parameters.AddWithValue("@serverId", server.id);

                using (MySqlDataReader reader = cmd.ExecuteReader()) {
                    if (reader.Read()) {
                        account = new Account(reader, server);
                    }
                }
            }

            return account;
        }

        public static Account Create(string accountName, Server server)
        {
            long id;

            using (MySqlCommand cmd = DataConnection.CreateDbCommand()) {
                cmd.CommandText = "INSERT INTO `accounts` (`account`, `server_id`) VALUES (@accountName, @serverId);";
                cmd.Parameters.AddWithValue("@accountName", accountName);
                cmd.Parameters.AddWithValue("@serverId", server.id);

                cmd.ExecuteNonQuery();
                id = cmd.LastInsertedId;
            }

            return id > 0 ? Fetch(id, server) : null;
        }

	}

}
