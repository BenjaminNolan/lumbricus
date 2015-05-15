using System;
using System.Linq;
using System.Data.Linq.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoWholeWorms.Lumbricus.Shared.Model
{

    [Table("Ban")]
    public class Ban
	{

        #region Database members
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long      Id                { get; set; }

        public string    Mask              { get; set; }

        public long      ServerId          { get; set; }
        public Server    Server            { get; set; } = null;

        public long?     NickId            { get; set; } = null;
        public Nick      Nick              { get; set; } = null;

        public long?     AccountId         { get; set; } = null;
        public Account   Account           { get; set; } = null;

        public long?     ChannelId         { get; set; } = null;
        public Channel   Channel           { get; set; } = null;
        
        public bool      IsActive          { get; set; } = true;
        public bool      IsMugshotBan      { get; set; } = true;

        public DateTime  CreatedAt         { get; set; } = DateTime.Now;
        public DateTime  LastModifiedAt    { get; set; } = DateTime.Now;

        public long?     BannerAccountId   { get; set; } = null;
        public Account   BannerAccount     { get; set; } = null;

        public string    BanMessage        { get; set; } = null;

        public long?     UnbannerAccountId { get; set; } = null;
        public Account   UnbannerAccount   { get; set; } = null;

        public DateTime? UnbannedAt        { get; set; } = null;
        public string    UnbanMessage      { get; set; } = null;
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

        public static Ban Create(Server server)
        {
            Ban ban = new Ban() {
                Server = server,
            };
            LumbricusContext.db.Bans.Add(ban);

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
            string host = (nick != null && nick.UserName != null && nick.Host != null ? nick.DisplayName + "!" + nick.UserName + "@" + nick.Host : null);
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
            string host = (nick != null && nick.UserName != null && nick.Host != null ? nick.DisplayName + "!" + nick.UserName + "@" + nick.Host : null);
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
            string host = (nick != null && nick.UserName != null && nick.Host != null ? nick.DisplayName + "!" + nick.UserName + "@" + nick.Host : null);
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
