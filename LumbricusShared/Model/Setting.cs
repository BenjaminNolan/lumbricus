using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TwoWholeWorms.Lumbricus.Shared;

namespace TwoWholeWorms.Lumbricus.Shared.Model
{

    public class Setting
	{
        #region Database members
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long     Id             { get; set; }

        [Required]
        public string   Section        { get; set; }
        [Required]
        public string   Name           { get; set; }
        [Required]
        public string   Value          { get; set; }
        [Required]
        public string   DefaultValue   { get; set; }
        #endregion Database members

        public static Setting Create(string section, string name, string value)
        {
            Setting setting = LumbricusContext.db.Settings.Create();
            setting.Section = section;
            setting.Name = name;
            setting.Value = value;
            setting.DefaultValue = value;

            LumbricusContext.db.Settings.Attach(setting);
            LumbricusContext.db.SaveChanges();

            return setting;
        }

        public static Setting Fetch(string section, string name)
        {
            if (section == null || name == null) return null;

            return (from s in LumbricusContext.db.Settings
                    where s.Section == section
                        && s.Name == name
                    select s).FirstOrDefault();
        }

        public static List<Setting> Fetch(string section)
        {
            if (section == null) return new List<Setting>();

            return (from s in LumbricusContext.db.Settings
                    where s.Section == section
                    select s).ToList();
        }

	}

}
