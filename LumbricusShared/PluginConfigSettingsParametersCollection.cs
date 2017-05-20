using System;
using System.Configuration;
using System.Collections.Generic;

namespace TwoWholeWorms.Lumbricus.Shared
{

    public class PluginConfigSettingsParametersCollection : ConfigurationElementCollection, IEnumerable<PluginConfigSettingsParameterElement>
    {
        public PluginConfigSettingsParameterElement this[int index]
        {
            get { return (PluginConfigSettingsParameterElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(PluginConfigSettingsParameterElement serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PluginConfigSettingsParameterElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PluginConfigSettingsParameterElement)element).Name;
        }

        public void Remove(PluginConfigSettingsParameterElement pluginConfig)
        {
            BaseRemove(pluginConfig.Name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(String name)
        {
            BaseRemove(name);
        }

        public new IEnumerator<PluginConfigSettingsParameterElement> GetEnumerator()
        {
            int count = base.Count;
            for (int i = 0; i < count; i++)
            {
                yield return base.BaseGet(i) as PluginConfigSettingsParameterElement;
            }
        }

    }

}

