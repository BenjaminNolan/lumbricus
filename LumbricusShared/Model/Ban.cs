﻿using System;
using System.Linq;
using System.Data.Linq.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace TwoWholeWorms.Lumbricus.Shared.Model
{

    public class Ban
	{

        #region Database members
        [Key]
        public long      Id             { get; set; }

        [Required]
        public string    Mask           { get; set; }

        public DateTime  CreatedAt      { get; set; } = DateTime.Now;
        public DateTime  LastModifiedAt { get; set; } = DateTime.Now;
        public DateTime? UnbannedAt     { get; set; } = null;
        public string    BanMessage     { get; set; } = null;
        public string    UnbanMessage   { get; set; } = null;
        public bool      IsMugshotBan   { get; set; } = false;
        public bool      IsActive       { get; set; } = false;

        public virtual Account Account { get; set; }
        public virtual Channel Channel { get; set; }
        public virtual Nick    Nick    { get; set; }
        public virtual Server  Server  { get; set; }

        public virtual Account BannerAccount   { get; set; }
        public virtual Account UnbannerAccount { get; set; }
        #endregion Database members

        public override string ToString()
        {
            return Id + ", "
                 + Nick.Id + ", "
                 + Account.Id + ", "
                 + Server.Id + ", "
                 + Mask + ", "
                 + CreatedAt + ", "
                 + LastModifiedAt + ", "
                 + Channel.Id + ", "
                 + BannerAccount.Id + ", "
                 + BanMessage + ", "
                 + UnbanMessage + ", "
                 + IsMugshotBan + ", "
                 + IsActive + ", "
                 + UnbannedAt + ", "
                 + UnbannerAccount.Id;
        }

        public static Ban Create()
        {
            Ban ban = LumbricusContext.db.Bans.Create();
            LumbricusContext.db.Bans.Attach(ban);

            return ban;
        }

        public void Save()
        {
            LumbricusContext.db.SaveChanges();
        }

        public static Ban Fetch(long id)
        {
            return (from b in LumbricusContext.db.Bans
                    where b.Id == id
                    select b).FirstOrDefault();
        }

        public static Ban Fetch(Nick nick, Account account)
        {
            string host = (nick != null && nick.User != null && nick.Host != null ? nick.DisplayNick + "!" + nick.User + "@" + nick.Host : null);
            string accountBan = (account != null ? "$a:" + account.Name : null);

            return (from b in LumbricusContext.db.Bans
                    where (b.Mask != null && SqlMethods.Like(host, b.Mask))
                       || (b.Mask != null && b.Mask == accountBan)
                       || (b.Nick != null && b.Nick.Id == nick.Id)
                       || (b.Account != null && account != null && b.Account.Id == account.Id)
                    select b).FirstOrDefault();
        }

        public static Ban Fetch(Channel channel, string mask)
        {
            return (from b in LumbricusContext.db.Bans
                    where (b.Channel != null && b.Channel.Id == channel.Id)
                       && (b.Mask == mask.Replace('*', '%'))
                    select b).FirstOrDefault();
        }

        public static Ban Fetch(Server server, string mask)
        {
            return (from b in LumbricusContext.db.Bans
                    where (b.Server != null && b.Server.Id == server.Id)
                       && (b.Mask == mask.Replace('*', '%'))
                    select b).FirstOrDefault();
        }

        public static IQueryable<Ban> Fetch(Nick nick)
        {
            string host = (nick != null && nick.User != null && nick.Host != null ? nick.DisplayNick + "!" + nick.User + "@" + nick.Host : null);
            string accountBan = (nick.Account != null ? "$a:" + nick.Account.Name : null);

            return (from b in LumbricusContext.db.Bans
                    where (b.Mask != null && SqlMethods.Like(host, b.Mask))
                       || (b.Mask != null && b.Mask == accountBan)
                       || (b.Nick != null && b.Nick.Id == nick.Id)
                       || (b.Account != null && nick.Account != null && b.Account.Id == nick.Account.Id)
                    select b);
        }

        public static Ban Fetch(Channel channel, Nick nick)
        {
            string host = (nick != null && nick.User != null && nick.Host != null ? nick.DisplayNick + "!" + nick.User + "@" + nick.Host : null);
            string accountBan = (nick.Account != null ? "$a:" + nick.Account.Name : null);

            return (from b in LumbricusContext.db.Bans
                    where b.Channel != null && b.Channel.Id == channel.Id
                        && (
                            (b.Mask != null && SqlMethods.Like(host, b.Mask))
                         || (b.Mask != null && b.Mask == accountBan)
                         || (b.Nick != null && b.Nick.Id == nick.Id)
                         || (b.Account != null && nick.Account != null && b.Account.Id == nick.Account.Id)
                        )
                    select b).FirstOrDefault();
        }

        public static Ban Fetch(Channel channel, Account account)
        {
            string accountBan = (account != null ? "$a:" + account.Name : null);

            return (from b in LumbricusContext.db.Bans
                where b.Channel != null && b.Channel.Id == channel.Id
                && (
                    (b.Account != null && account != null && b.Account.Id == account.Id)
                    || (b.Mask != null && b.Mask == accountBan)
                )
                select b).FirstOrDefault();
        }

	}

}