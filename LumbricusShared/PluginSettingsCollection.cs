using System;
using System.Configuration;
using System.Collections.Generic;

namespace TwoWholeWorms.Lumbricus.Shared
{

    public class PluginSettingsCollection : ConfigurationElementCollection, IEnumerable<PluginSettingElement>
    {
        public PluginSettingElement this[int index]
        {
            get { return (PluginSettingElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(PluginSettingElement serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PluginSettingElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PluginSettingElement)element).Name;
        }

        public void Remove(PluginSettingElement pluginConfig)
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

        public new IEnumerator<PluginSettingElement> GetEnumerator()
        {
            int count = base.Count;
            for (int i = 0; i < count; i++)
            {
                yield return base.BaseGet(i) as PluginSettingElement;
            }
        }

    }

}

