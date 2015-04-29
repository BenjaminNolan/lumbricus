using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Plugins.MugshotsPlugin;

namespace TwoWholeWorms.Lumbricus.Plugins.MugshotsPlugin.Model
{

    public class Mugshot
    {
        
        #region Database members
        [Key]
        public virtual Account Account { get; set; }

        public string OriginalImageUri { get; set; }
        public string LargeUri         { get; set; }
        public string ThumbnailUri     { get; set; }

        public DateTime CreatedAt      { get; set; }
        public DateTime LastModifiedAt { get; set; }

        public bool IsActive  { get; set; }
        public bool IsDeleted { get; set; }
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
            Mugshot mugshot = MugshotsContext.db.Mugshots.Create();
            MugshotsContext.db.Mugshots.Attach(mugshot);
            if (account != null) {
                mugshot.Account = account;
                mugshot.Save();
            }

            return mugshot;
        }

        public void Save()
        {
            MugshotsContext.db.SaveChanges();
        }

    }

}
