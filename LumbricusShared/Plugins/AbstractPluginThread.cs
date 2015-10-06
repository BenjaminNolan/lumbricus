//using NLog;
using System;
using System.Linq;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Shared.Plugins
{

    abstract public class AbstractPluginThread : IDisposable
    {

//        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        bool disposed = false;
        public bool Disposed => disposed;

        #region IDisposable implementation
        ~AbstractPluginThread()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            disposed = true;

            // Unlink me from ALL THE THINGS!
        }
        #endregion

        abstract public void Run();

    }

}
