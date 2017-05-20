using System;
using System.Configuration;
using System.Collections.Generic;

namespace TwoWholeWorms.Lumbricus.Shared
{

    [ConfigurationCollection(typeof(PluginConfigSettingsCollection), AddItemName = "group", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class PluginConfigSettingsCollection : ConfigurationElementCollection, IEnumerable<PluginConfigSettingsParametersElement>
    {
        public PluginConfigSettingsParametersElement this[int index]
        {
            get { return (PluginConfigSettingsParametersElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(PluginConfigSettingsParametersElement serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PluginConfigSettingsParametersElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PluginConfigSettingsParametersElement)element).Name;
        }

        public void Remove(PluginConfigSettingsParametersElement pluginConfig)
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

        public new IEnumerator<PluginConfigSettingsParametersElement> GetEnumerator()
        {
            int count = base.Count;
            for (int i = 0; i < count; i++)
            {
                yield return base.BaseGet(i) as PluginConfigSettingsParametersElement;
            }
        }

    }

}

