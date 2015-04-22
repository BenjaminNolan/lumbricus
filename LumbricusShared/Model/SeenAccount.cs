using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoWholeWorms.Lumbricus.Shared.Model
{

    public class SeenAccount : ISeen
	{

        #region Database members
        [Key]
        [Required]
        [ForeignKey("Account")]
        public long AccountId { get; set; }

        [Required]
        public virtual Account Account { get; set; }

        [Required]
        public virtual Server  Server  { get; set; }
        
        public virtual Channel Channel { get; set; }
        public virtual Nick    Nick    { get; set; }

        public DateTime FirstSeenAt { get; set; } = DateTime.Now;
        public DateTime LastSeenAt  { get; set; } = DateTime.Now;
        #endregion Database members

        public static SeenAccount FetchByAccountId(long accountId)
        {
            return (from s in LumbricusContext.db.SeenAccounts
                where s.Account != null && s.Account.Id == accountId
                select s).FirstOrDefault();
        }

        public static SeenAccount FetchByNickId(long nickId)
        {
            return (from s in LumbricusContext.db.SeenAccounts
                where s.Nick != null && s.Nick.Id == nickId
                select s).FirstOrDefault();
        }

        public static SeenAccount Fetch(Nick nick)
        {
            return (from s in LumbricusContext.db.SeenAccounts
                where (s.Nick != null && s.Nick.Id == nick.Id)
                || s.Account != null && nick.Account != null && s.Account.Id == nick.Account.Id
                select s).FirstOrDefault();
        }

        public static SeenAccount Fetch(Account account)
        {
            return (from s in LumbricusContext.db.SeenAccounts
                where (s.Nick != null && account.PrimaryNick != null && s.Nick.Id == account.PrimaryNick.Id)
                || s.Account != null && s.Account.Id == account.Id
                select s).FirstOrDefault();
        }

	}

}
