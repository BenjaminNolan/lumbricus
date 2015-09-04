using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Plugins.MugshotsPlugin;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoWholeWorms.Lumbricus.Plugins.MugshotsPlugin.Model
{

    public class Mugshot
    {
        
        #region Database members
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long     Id               { get; set; }

        public long     AccountId        { get; set; }
        [ForeignKey("AccountId")]
        public Account  Account          { get; set; }

        public string   FileName         { get; set; }
        public string   OriginalImageUri { get; set; }

        public DateTime CreatedAt        { get; set; } = DateTime.Now;
        public DateTime LastModifiedAt   { get; set; } = DateTime.Now;

        public bool     IsActive         { get; set; } = true;
        public bool     IsDeleted        { get; set; } = false;
        #endregion Database members

        public static Mugshot FetchOrCreate(Account account)
        {
            return Fetch(account) ?? Create(account);
        }

        public static Mugshot Fetch(Account account)
        {
            if (account == null) return null;

            return (from s in MugshotsContext.db.Mugshots
                where s.Account != null && s.Account.Id == account.Id
                select s).FirstOrDefault();
        }

        public static Mugshot Fetch(Nick nick)
        {
            if (nick.Account == null) return null;

            return (from s in MugshotsContext.db.Mugshots
                where s.Account != null && s.Account.Id == nick.Account.Id
                select s).FirstOrDefault();
        }

        public static Mugshot Create(Account account = null)
        {
            if (account != null) {
                Mugshot mugshot = MugshotsContext.db.Mugshots.Create();
                MugshotsContext.db.Mugshots.Add(mugshot);
                mugshot.AccountId = account.Id;

                return mugshot;
            }

            return null;
        }

        public void Save()
        {
            MugshotsContext.db.SaveChanges();
        }

    }

}
