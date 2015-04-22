using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace GayGeeksBot
{

    public class IrcUser
	{
        public long nickId { get; protected set; }
        public string nick { get; protected set; }
        public string displayNick { get; protected set; }
        public long accountId { get; protected set; }
        public string account { get; protected set; }
        public DateTime accountCreatedAt { get; protected set; }
        public DateTime nickCreatedAt { get; protected set; }
        public bool isOp { get; protected set; }

        public List<IrcChannel> channels = new List<IrcChannel>();

        public IrcUser(MySqlDataReader reader)
        {
            this.nickId = reader.GetInt64Safe("nick_id");
            this.nick = reader.GetStringSafe("nick");
            this.accountId = reader.GetInt64Safe("account_id");
            this.account = reader.GetStringSafe("account");
            this.isOp = (reader.GetInt64Safe("is_op") == 1);
            this.nickCreatedAt = reader.GetDateTime("nick_created_at");
            this.accountCreatedAt = reader.GetDateTimeSafe("account_created_at");
            this.displayNick = reader.GetStringSafe("display_nick");
        }

        public IrcUser(string nick)
		{
            this.nick = nick;
		}

        public void SetNick(string nick)
        {
            this.nick = nick;
        }

        public void AddChannel(IrcChannel channel, bool recurse = true)
        {
            if (!channels.Contains(channel)) {
//                GayGeeksBot.Log(String.Format("Adding channel {0} to user {1}", channel.name, nick));
                channels.Add(channel);
            }
            if (recurse) {
                channel.AddUser(this, false);
            }
        }

        public void RemoveChannel(IrcChannel channel, bool recurse = true)
        {
            if (channels.Contains(channel)) {
//                GayGeeksBot.Log(String.Format("Removing channel {0} from user {1}", channel.name, nick));
                channels.Remove(channel);
            }
            if (recurse) {
                channel.RemoveUser(this, false);
            }
        }

        public override string ToString()
        {
            string cString = "";
            foreach (IrcChannel c in channels) {
                cString += " " + c.name;
            }
            return this.nickId + ", "
                + this.nick + ", "
                + this.accountId + ", "
                + this.account + ", "
                + this.isOp + ", "
                + this.nickCreatedAt + ", "
                + this.accountCreatedAt + ", "
                + this.displayNick + ". Channels:" + cString;
        }

	}

}
