using System;

namespace TwoWholeWorms.Lumbricus.Shared.Model
{

    public class Seen : ISeen
	{

        public virtual Account Account { get; set; }
        public virtual Channel Channel { get; set; }
        public virtual Nick    Nick    { get; set; }
        public virtual Server  Server  { get; set; }

        public DateTime FirstSeenAt { get; set; } = DateTime.Now;
        public DateTime LastSeenAt  { get; set; } = DateTime.Now;

        public Seen()
        {
        }

        public Seen(ISeen seenOne, ISeen seenTwo)
        {
            if (seenOne != null && seenOne.Account != null) {
                Account = seenOne.Account;
            } else if (seenTwo != null && seenTwo.Account != null) {
                Account = seenTwo.Account;
            }
            if (seenOne != null && seenOne.Channel != null) {
                Channel = seenOne.Channel;
            } else if (seenTwo != null && seenTwo.Channel != null) {
                Channel = seenTwo.Channel;
            }
            if (seenOne != null && seenOne.Nick != null) {
                Nick = seenOne.Nick;
            } else if (seenTwo != null && seenTwo.Nick != null) {
                Nick = seenTwo.Nick;
            }
            if (seenOne != null && seenOne.Server != null) {
                Server = seenOne.Server;
            } else if (seenTwo != null && seenTwo.Server != null) {
                Server = seenTwo.Server;
            }
            if (seenOne != null) {
                FirstSeenAt = seenOne.FirstSeenAt;
                if (seenTwo != null && seenTwo.FirstSeenAt < FirstSeenAt) {
                    FirstSeenAt = seenTwo.FirstSeenAt;
                }
            } else if (seenTwo != null) {
                FirstSeenAt = seenTwo.FirstSeenAt;
            }
            if (seenOne != null) {
                LastSeenAt = seenOne.LastSeenAt;
                if (seenTwo != null && seenTwo.LastSeenAt < LastSeenAt) {
                    LastSeenAt = seenTwo.LastSeenAt;
                }
            } else if (seenTwo != null) {
                LastSeenAt = seenTwo.LastSeenAt;
            }
        }

        public static Seen FetchByAccountId(long accountId)
        {
            SeenNick seenNick = SeenNick.FetchByAccountId(accountId);
            SeenAccount seenAccount = SeenAccount.FetchByAccountId(accountId);

            return new Seen(seenNick, seenAccount);
        }

        public static Seen FetchByNickId(long nickId)
        {
            SeenNick seenNick = SeenNick.FetchByNickId(nickId);
            SeenAccount seenAccount = SeenAccount.FetchByNickId(nickId);

            return new Seen(seenNick, seenAccount);
        }

        public static Seen Fetch(Nick nick)
        {
            SeenNick seenNick = SeenNick.Fetch(nick);
            SeenAccount seenAccount = SeenAccount.Fetch(nick);

            return new Seen(seenNick, seenAccount);
        }

        public static Seen Fetch(Account account)
        {
            SeenNick seenNick = SeenNick.Fetch(account);
            SeenAccount seenAccount = SeenAccount.Fetch(account);

            return new Seen(seenNick, seenAccount);
        }

        public void Save()
        {
            if (Account != null) {
                SeenAccount seenAccount = LumbricusContext.db.SeenAccounts.Create();
                seenAccount.Account     = Account;
                seenAccount.Channel     = Channel;
                seenAccount.FirstSeenAt = FirstSeenAt;
                seenAccount.LastSeenAt  = LastSeenAt;
                seenAccount.Nick        = Nick;
                seenAccount.Server      = Server;

                LumbricusContext.db.SeenAccounts.Attach(seenAccount);
                LumbricusContext.db.SaveChanges();
            }
            if (Nick != null) {
                SeenNick seenNick = LumbricusContext.db.SeenNicks.Create();
                seenNick.Nick        = Nick;
                seenNick.Channel     = Channel;
                seenNick.FirstSeenAt = FirstSeenAt;
                seenNick.LastSeenAt  = LastSeenAt;
                seenNick.Account     = Account;
                seenNick.Server      = Server;

                LumbricusContext.db.SeenNicks.Attach(seenNick);
                LumbricusContext.db.SaveChanges();
            }
        }

	}

}
