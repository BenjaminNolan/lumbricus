/**
 * This is a bit of a monolith at the moment At some point, I'll split it out into other classes, and I'm also
 * planning to refactor it so the connection is related to a Server instance rather than the other way around.
 */

using NLog;
using System;

namespace TwoWholeWorms.Lumbricus.Shared
{

    public abstract class AbstractConnection : IDisposable
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();
        
        bool disposed = false;
        public bool Disposed => disposed;

        public delegate void ProcessLineDelegate(AbstractConnection conn);
        public ProcessLineDelegate ProcessLines;

        protected PluginConfigSettingsParametersElement config;

        public AbstractConnection(PluginConfigSettingsParametersElement config)
		{
			try {
				loadServerDetails();
                this.config = config;
            } catch (Exception e) {
                logger.Error(e);
            }
        }

        #region IDisposable implementation
        ~AbstractConnection()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        abstract protected void Dispose(bool disposing);
        #endregion

        public void Run()
        {
            try {
                // TODO: Make this reconnection HAQUE less HAQUI. o.o
                //INITIALISE_BOT:
                connect();

                initialiseConnection();
                handleInput();

                //if (lastLine.IrcCommand == "ERROR") {
                //    logger.Error("Disconnected by server, so reconnecting.");
                //    goto INITIALISE_BOT;
                //}
            } catch (Exception e) {
                logger.Error(e);
            }
		}

//        ~IrcConnection()
//        {
//            ns.Close();
//            socket.Close();
//        }

        protected abstract void loadServerDetails();
        protected abstract void initialiseConnection();
        protected abstract void connect();
        protected abstract void handleInput();

    }

}
