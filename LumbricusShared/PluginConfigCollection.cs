using System;
using System.Configuration;
using System.Collections.Generic;

namespace TwoWholeWorms.Lumbricus.Shared
{

    [ConfigurationCollection(typeof(PluginConfigCollection), AddItemName = "plugin", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class PluginConfigCollection : ConfigurationElementCollection, IEnumerable<PluginConfigElement>
    {
        public PluginConfigElement this[int index]
        {
            get { return (PluginConfigElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(PluginConfigElement serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PluginConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PluginConfigElement)element).Name;
        }

        public void Remove(PluginConfigElement pluginConfig)
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

        public new IEnumerator<PluginConfigElement> GetEnumerator()
        {
            int count = base.Count;
            for (int i = 0; i < count; i++)
            {
                yield return base.BaseGet(i) as PluginConfigElement;
            }
        }

    }

}

