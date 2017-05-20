using System.Collections.Generic;

namespace TwoWholeWorms.Lumbricus.Shared.Plugins
{

    public abstract class AbstractConnectionPlugin
    {

        public abstract void RegisterPlugin();
        public abstract List<AbstractConnection> GetConnections();
        public abstract string Name { get; }

        public abstract void Run();

    }

}
