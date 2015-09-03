using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Plugins.InfoPlugin;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwoWholeWorms.Lumbricus.Plugins.InfoPlugin.Model
{

    [Table("Info")] 
    public class Info
    {

        #region Database members
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long      Id              { get; set; }

        public long      AccountId       { get; set; }
        [ForeignKey("AccountId")]
        public Account   Account         { get; set; }

        public string    InfoTxt         { get; set; }

        public DateTime  CreatedAt       { get; set; } = DateTime.Now;
        public DateTime  LastModifiedAt  { get; set; } = DateTime.Now;

        public bool      IsActive        { get; set; } = true;
        public bool      IsDeleted       { get; set; } = false;
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
            if (account != null) {
                Info info = InfoContext.db.Infos.Create();
                InfoContext.db.Infos.Add(info);
                info.AccountId = account.Id;
                info.Save();
                return info;
            }

            return null;
        }

        public void Save()
        {
            InfoContext.db.SaveChanges();
        }

    }

}
