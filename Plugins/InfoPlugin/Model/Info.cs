using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Plugins.InfoPlugin;

namespace TwoWholeWorms.Lumbricus.Plugins.InfoPlugin.Model
{

    public class Info
    {

        #region Database members
        [Key]
        public virtual Account Account { get; set; }

        public string InfoTxt { get; set; }

        public DateTime CreatedAt      { get; set; }
        public DateTime LastModifiedAt { get; set; }

        public bool IsActive  { get; set; }
        public bool IsDeleted { get; set; }
        #endregion Database members

        public static Info FetchOrCreate(Account account)
        {
            return Fetch(account) ?? Create(account);
        }

        public static Info Fetch(Account account)
        {
            if (account == null) return null;

            return (from s in InfoContext.db.Infos
                where s.Account != null && s.Account.Id == account.Id
                select s).FirstOrDefault();
        }

        public static Info Fetch(Nick nick)
        {
            if (nick.Account == null) return null;

            return (from s in InfoContext.db.Infos
                where s.Account != null && s.Account.Id == nick.Account.Id
                select s).FirstOrDefault();
        }

        public static Info Create(Account account = null)
        {
            Info mugshot = InfoContext.db.Infos.Create();
            InfoContext.db.Infos.Attach(mugshot);
            if (account != null) {
                mugshot.Account = account;
                mugshot.Save();
            }

            return mugshot;
        }

        public void Save()
        {
            InfoContext.db.SaveChanges();
        }

    }

}
