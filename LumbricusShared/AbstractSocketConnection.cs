/**
 * This is a bit of a monolith at the moment At some point, I'll split it out into other classes, and I'm also
 * planning to refactor it so the connection is related to a Server instance rather than the other way around.
 */

using NLog;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace TwoWholeWorms.Lumbricus.Shared
{

    public abstract class AbstractSocketConnection : AbstractConnection
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        protected NetworkStream ns;
        protected Socket socket;
        protected StreamReader sr;

        public AbstractSocketConnection(PluginConfigSettingsParametersElement config) : base(config)
        {
        }
        
        public new void Run()
        {
            base.Run();

            try {
                if (ns != null) {
                    ns.Close();
                }
            } catch (Exception e) {
                logger.Error(e);
            }

            try {
                if (socket != null) {
                    socket.Close();
                }
            } catch (Exception e) {
                logger.Error(e);
            }
        }

        //        ~IrcConnection()
        //        {
        //            ns.Close();
        //            socket.Close();
        //        }

        string GetLineFromSocket()
        {
            string line = sr.ReadLine();
            if (String.IsNullOrEmpty(line)) {
                throw new Exception("Socket disconnected.");
            }
            logger.Trace(line);
            return line;
        }

        public void Send(String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data + "\r\n");

            logger.Debug(data);

            // Begin sending the data to the remote device.
            ns.Write(byteData, 0, byteData.Length);
            ns.Flush();
        }

    }

}
