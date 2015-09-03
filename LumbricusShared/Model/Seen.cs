using System;
using System.Collections.Generic;
using System.Linq;

namespace TwoWholeWorms.Lumbricus.Shared.Model
{

    // Slightly hacky pseudo-model to join together seen data from Account and Nick and save it back to both tables
    public class Seen
	{

        public Account Account { get; set; }
        public Channel Channel { get; set; }
        public Nick    Nick    { get; set; }
        public Server  Server  { get; set; }

        public DateTime FirstSeenAt { get; set; } = DateTime.Now;
        public DateTime LastSeenAt  { get; set; } = DateTime.Now;

        public Seen()
        {
        }

        public Seen(Nick nick = null, Account account = null, Channel channel = null)
        {
            if (nick == null && account == null) {
                return;
            }

            if (account == null && nick.Account != null) {
                account = nick.Account;
            }
            if (nick == null && account.MostRecentNick != null) {
                nick = account.MostRecentNick;
            }
            if (channel == null) {
                if (nick != null && nick.ChannelLastSeenIn != null) {
                    channel = nick.ChannelLastSeenIn;
                } else if (account != null && account.ChannelLastSeenIn != null) {
                    channel = account.ChannelLastSeenIn;
                }
            }

            Account = account;
            Nick = nick;
            Channel = channel;

            Server = nick?.Server ?? account?.Server ?? channel?.Server;

            if (nick != null) {
                FirstSeenAt = nick.FirstSeenAt;
                if (account != null && account.FirstSeenAt < FirstSeenAt) {
                    FirstSeenAt = account.FirstSeenAt;
                }
            } else if (account != null) {
                FirstSeenAt = account.FirstSeenAt;
            }
            if (nick != null) {
                LastSeenAt = nick.LastSeenAt;
                if (account != null && account.LastSeenAt < LastSeenAt) {
                    LastSeenAt = account.LastSeenAt;
                }
            } else if (account != null) {
                LastSeenAt = account.LastSeenAt;
            }
        }

        public static Seen FetchByAccountId(long accountId)
        {
            List<Nick> nicks = Nick.FetchByAccountId(accountId).ToList();
            Account account = Account.Fetch(accountId);

            Nick nick = nicks.FirstOrDefault();
            return new Seen(nick, account);
        }

        public static Seen FetchByNickId(long nickId)
        {
            Nick nick = Nick.Fetch(nickId);
            Account account = Account.FetchByNickId(nickId);

            return new Seen(nick, account);
        }

        public static Seen Fetch(Nick nick)
        {
            if (nick == null) return null;

            Account account = Account.FetchByNickId(nick.Id);

            return new Seen(nick, account);
        }

        public static Seen Fetch(Account account)
        {
            if (account == null) return null;

            List<Nick> nicks = Nick.FetchByAccountId(account.Id).ToList();
            Nick nick = nicks.FirstOrDefault();

            return new Seen(nick, account);
        }

        public void Save()
        {
            if (Account != null) {
                Account.ChannelLastSeenIn = Channel;
                Account.FirstSeenAt       = FirstSeenAt;
                Account.LastSeenAt        = LastSeenAt;
                Account.MostRecentNick    = Nick;

                try {
                    LumbricusContext.db.Accounts.Attach(Account);
                } catch (Exception e) {
                    // …
                }
                LumbricusContext.db.SaveChanges();
            }
            if (Nick != null) {
                Nick.ChannelLastSeenIn = Channel;
                Nick.FirstSeenAt       = FirstSeenAt;
                Nick.LastSeenAt        = LastSeenAt;
                Nick.Account           = Account;

                try {
                    LumbricusContext.db.Nicks.Attach(Nick);
                } catch (Exception e) {
                    // …
                }
                LumbricusContext.db.SaveChanges();
            }
        }

        public static Seen Update(Server ircServer, Nick ircNick)
        {
            return Update(ircServer, ircNick, null, null);
        }

        public static Seen Update(Server ircServer, Nick ircNick, Account ircAccount)
        {
            return Update(ircServer, ircNick, ircAccount, null);
        }

        public static Seen Update(Server ircServer, Nick ircNick, Account ircAccount, Channel ircChannel)
        {
            Seen seen = Seen.Fetch(ircNick);
            if (seen == null) {
                seen = new Seen();
                seen.FirstSeenAt = DateTime.Now;
            }
            seen.Channel = ircChannel;
            seen.Server = ircServer;
            seen.Account = ircAccount;
            seen.Nick = ircNick;
            seen.LastSeenAt = DateTime.Now;
            seen.Save();

            if (seen.Account != null && ircNick.Account == null) {
                ircServer.AddAccount(seen.Account);
                ircNick.Account = seen.Account;
                seen.Account.AddNick(ircNick);
            }

            return seen;
        }

	}

}
