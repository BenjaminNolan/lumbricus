using System;
using System.Configuration;
using System.Collections.Generic;

namespace TwoWholeWorms.Lumbricus.Shared
{

    [ConfigurationCollection(typeof(PluginConfigCollection), AddItemName = "plugin", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class PluginConfigCollection : ConfigurationElementCollection, IEnumerable<PluginConfigurationElement>
    {
        public PluginConfigurationElement this[int index]
        {
            get { return (PluginConfigurationElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(PluginConfigurationElement serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PluginConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PluginConfigurationElement)element).Name;
        }

        public void Remove(PluginConfigurationElement pluginConfig)
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

        public new IEnumerator<PluginConfigurationElement> GetEnumerator()
        {
            int count = base.Count;
            for (int i = 0; i < count; i++)
            {
                yield return base.BaseGet(i) as PluginConfigurationElement;
            }
        }

    }

}

