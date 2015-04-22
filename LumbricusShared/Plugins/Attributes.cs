using System;

namespace TwoWholeWorms.Lumbricus.Shared.Plugins
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class LumbricusPlugin : Attribute
    {
        readonly Type type;
        public Type Type => type;

        public LumbricusPlugin(Type type)
        {
            this.type = type;
        }
    }
}

