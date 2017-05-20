namespace TwoWholeWorms.Lumbricus.Shared.Plugins
{

    public abstract class AbstractPlugin
    {

        public abstract void RegisterPlugin();
        public abstract string Name { get; }

    }

}
