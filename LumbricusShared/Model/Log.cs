using System;
using System.Linq;
using TwoWholeWorms.Lumbricus.Shared;
using System.ComponentModel.DataAnnotations;

namespace TwoWholeWorms.Lumbricus.Shared.Model
{
    
    public class Log
    {

        #region Database members
        [Key]
        public long       Id         { get; set; }

        [Required]
        public IrcCommand IrcCommand { get; set; }

        public DateTime   LoggedAt   { get; set; } = DateTime.Now;
        public string     Trail      { get; set; }
        public string     Line       { get; set; }

        public virtual Account Account { get; set; }
        public virtual Channel Channel { get; set; }
        public virtual Nick    Nick    { get; set; }
        public virtual Server  Server  { get; set; }
        #endregion Database members

        public static Log Fetch(long id)
        {
            return (from l in LumbricusContext.db.Logs
                    where l.Id == id
                    select l).FirstOrDefault();
        }

        public static long FetchTotal(Account account)
        {
            return (from l in LumbricusContext.db.Logs
                where (account.PrimaryNick != null && l.Nick != null && l.Nick.Id == account.PrimaryNick.Id)
                || (l.Account != null && l.Account.Id == account.Id)
                select l).Count();
        }

        public static Log Fetch(Account account, Log ignoreLogLine = null, Channel excludeChannel = null)
        {
            return (from l in LumbricusContext.db.Logs
                where ((account.PrimaryNick != null && l.Nick != null && l.Nick.Id == account.PrimaryNick.Id)
                    || (l.Account != null && l.Account.Id == account.Id))
                && (ignoreLogLine == null || l.Id != ignoreLogLine.Id)
                && (excludeChannel == null || (l.Channel != null && l.Channel.Id != excludeChannel.Id))
                select l).FirstOrDefault();
        }

        public static long FetchTotal(Nick nick)
        {
            return (from l in LumbricusContext.db.Logs
                where (l.Nick != null && l.Nick.Id == nick.Id)
                || (nick.Account != null && l.Account != null && l.Account.Id == nick.Account.Id)
                select l).Count();
        }

        public static Log Fetch(Nick nick, Log ignoreLogLine = null, Channel excludeChannel = null)
        {
            return (from l in LumbricusContext.db.Logs
                where ((l.Nick != null && l.Nick.Id == nick.Id)
                    || (nick.Account != null && l.Account != null && l.Account.Id == nick.Account.Id))
                && (ignoreLogLine == null || l.Id != ignoreLogLine.Id)
                && (excludeChannel == null || (l.Channel != null && l.Channel.Id != excludeChannel.Id))
                select l).FirstOrDefault();
        }

        public static long FetchTotal(Channel channel)
        {
            return (from l in LumbricusContext.db.Logs
                where (l.Channel != null && l.Channel.Id == channel.Id)
                select l).Count();
        }

        public static Log Fetch(Channel channel, Log ignoreLogLine = null)
        {
            return (from l in LumbricusContext.db.Logs
                where l.Channel != null && l.Channel.Id == channel.Id
                && (ignoreLogLine == null || l.Id != ignoreLogLine.Id)
                select l).FirstOrDefault();
        }

        public static long FetchTotal(string search)
        {
            return (from l in LumbricusContext.db.Logs
                where (l.Line != null && l.Line.Contains(search))
                select l).Count();
        }

        public static Log Fetch(string search, Log ignoreLogLine = null, Channel excludeChannel = null)
        {
            return (from l in LumbricusContext.db.Logs
                where (l.Line != null && l.Line.Contains(search))
                && (ignoreLogLine == null || l.Id != ignoreLogLine.Id)
                && (excludeChannel == null || (l.Channel != null && l.Channel.Id != excludeChannel.Id))
                select l).FirstOrDefault();
        }

        public static Log Create()
        {
            Log log = LumbricusContext.db.Logs.Create();
            LumbricusContext.db.Logs.Attach(log);
            return log;
        }

        public void Save()
        {
            LumbricusContext.db.SaveChanges();
        }

	}

}
