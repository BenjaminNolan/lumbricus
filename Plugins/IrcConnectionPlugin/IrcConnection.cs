using NLog;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Plugins.IrcConnectionPlugin.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.IrcConnectionPlugin
{

    class IrcConnection : AbstractSocketConnection
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public Server Server;
        protected PluginConfigSettingsParametersCollection settings;

        public IrcConnection(PluginConfigSettingsParametersElement config) : base(config)
        {
            settings = config.Settings;
        }

        protected override void connect()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Server.Host);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            var remoteEP = new IPEndPoint(ipAddress, Server.Port);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(remoteEP);
            if (!socket.Connected) {
                logger.Error("The socket disconnected.");
                throw new Exception("Remote socket disconnected during initialisation");
            }

            ns = new NetworkStream(socket, true);
            sr = new StreamReader(ns);
        }

        public void SendPrivmsg(string nick, string msg)
        {
            Send(String.Format("PRIVMSG {0} :{1}", nick, msg));
        }

        protected override void loadServerDetails()
        {
            Server = new Server() {
                Name = config.Name,
                Host = settings.Single(p => p.Name == "Host").Value,
                Port = int.Parse(settings.Single(p => p.Name == "Port").Value),
                AutoConnect = settings.Single(p => p.Name == "AutoConnect").Value == "true",
                BotNick = settings.Single(p => p.Name == "BotNick").Value,
                BotNickPassword = settings.Single(p => p.Name == "BotNickPassword").Value,
                BotRealName = settings.Single(p => p.Name == "BotRealName").Value,
                BotUserName = settings.Single(p => p.Name == "BotUserName").Value,
                NickServHost = settings.Single(p => p.Name == "NickServHost").Value,
                NickServNick = settings.Single(p => p.Name == "NickServNick").Value,
            };
        }

        protected override void Dispose(bool disposing)
        {

        }

        protected override void initialiseConnection()
        {

        }

        protected override void handleInput()
        {

        }

    }

}
