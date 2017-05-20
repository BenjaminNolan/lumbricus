using System;
using System.Configuration;
using System.Collections.Generic;

namespace TwoWholeWorms.Lumbricus.Shared
{

    public class PluginConfigSettingsParameterElement : ConfigurationElement
    {

        public PluginConfigSettingsParameterElement(string name, string value)
        {
            Name  = name;
            Value = value;
        }

        public PluginConfigSettingsParameterElement()
        {

            Name  = null;
            Value = null;
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get { return (string)this["value"]; }
            set { this["value"] = value; }
        }
    
    }

}
