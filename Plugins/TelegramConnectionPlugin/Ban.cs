using System;
using System.Linq;
using System.Data.Linq.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoWholeWorms.Lumbricus.Plugins.TelegramConnectionPlugin.Model
{

    [Table("Ban")]
    public class Ban
    {

        #region Database members
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long ServerId { get; set; }
        public Server Server { get; set; } = null;

        public long? UserId { get; set; } = null;
        public User User { get; set; } = null;

        public long? ChatId { get; set; } = null;
        public Chat Chat { get; set; } = null;
        #endregion Database members

        public override string ToString()
        {
            return Id + ", "
                 + Server.Id;
        }

        public static Ban Create(Server server)
        {
            Ban ban = new Ban() {
                Server = server,
            };
            TelegramPluginContext.db.Bans.Add(ban);

            return ban;
        }

        public void Save()
        {
            TelegramPluginContext.db.SaveChanges();
        }

        public static Ban Fetch(long id)
        {
            return (from b in TelegramPluginContext.db.Bans
                    where b.Id == id
                    select b).FirstOrDefault();
        }

        public static Ban Fetch(User user)
        {
            return null;
        }

    }

}
