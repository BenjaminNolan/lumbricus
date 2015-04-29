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
        [MaxLength(32)]
        public string   Section        { get; set; }
        [Required]
        [MaxLength(32)]
        public string   Name           { get; set; }
        [Required]
        [MaxLength(512)]
        public string   Value          { get; set; }
        [Required]
        [MaxLength(512)]
        public string   DefaultValue   { get; set; }
        #endregion Database members

        public static Setting Create(string section, string name, string value)
        {
            Setting setting = new Setting() {
                Section = section,
                Name = name,
                Value = value,
                DefaultValue = value,
            };

            LumbricusContext.db.Settings.Add(setting);
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
