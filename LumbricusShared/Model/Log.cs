using System;
using System.Linq;
using TwoWholeWorms.Lumbricus.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoWholeWorms.Lumbricus.Shared.Model
{

    /**
     * @TODO Extend this to capture everything extracted in the regex in IrcConnection
     */
    [Table("Log")]
    public class Log
    {

        #region Database members
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long       Id         { get; set; }

        public long       ServerId   { get; set; }
        public Server     Server     { get; set; }

        public long?      NickId     { get; set; } = null;
        public Nick       Nick       { get; set; } = null;

        public long?      AccountId  { get; set; } = null;
        public Account    Account    { get; set; } = null;

        public long?      ChannelId  { get; set; } = null;
        public Channel    Channel    { get; set; } = null;

        public IrcCommand IrcCommand { get; set; }

        public string     Trail      { get; set; }
        public string     Line       { get; set; }
        
        public DateTime   LoggedAt   { get; set; } = DateTime.Now;
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
                where (account.MostRecentNick != null && l.Nick != null && l.Nick.Id == account.MostRecentNick.Id)
                || (l.Account != null && l.Account.Id == account.Id)
                select l).Count();
        }

        public static Log Fetch(Account account, Log ignoreLogLine = null, Channel excludeChannel = null)
        {
            return (from l in LumbricusContext.db.Logs
                where ((account.MostRecentNick != null && l.Nick != null && l.Nick.Id == account.MostRecentNick.Id)
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
            Log log = new Log();
            return log;
        }

        public void Save()
        {
            if (!LumbricusContext.db.Logs.Contains(this)) {
                LumbricusContext.db.Logs.Add(this);
            }
            LumbricusContext.db.SaveChanges();
        }

	}

}
