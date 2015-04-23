/**
 * This is a bit of a monolith at the moment At some point, I'll split it out into other classes, and I'm also
 * planning to refactor it so the connection is related to a Server instance rather than the other way around.
 */

using System;
using System.Collections.Generic;
using NLog;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using MySql.Data.MySqlClient;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Shared
{

    public class IrcConnection : IDisposable
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public Server Server { get; protected set; }

        protected NetworkStream ns;
        protected Socket socket;
        protected StreamReader sr;
        protected Dictionary<string, List<IrcLine>> queue = new Dictionary<string, List<IrcLine>>();

        bool disposed = false;
        public bool Disposed => disposed;

        public delegate void ProcessIrcLineDelegate(IrcConnection conn, IrcLine line);
        public ProcessIrcLineDelegate ProcessIrcLine;

        public Dictionary<string, AbstractCommand> Commands = new Dictionary<string, AbstractCommand>();

        public LumbricusConfiguration Config;

        public IrcConnection(Server server, LumbricusConfiguration config)
		{
			try {
				Server = server;
                Config = config;
            } catch (Exception e) {
                logger.Error(e);
            }
        }

        #region IDisposable implementation
        ~IrcConnection()
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

            if (!Server.Disposed) Server.Dispose();
        }
        #endregion

        public void Run()
        {
            try {
                logger.Debug("Initialising plugins");
                foreach (AbstractPlugin plugin in LumbricusConfiguration.Plugins) {
                    plugin.RegisterPlugin(this);
                }
                logger.Info("Connecting to {0}:{1}", Server.Host, Server.Port);

                socket = ConnectToNetwork();

                Send(String.Format("USER {0} 8 * :{1}", Server.BotUserName, Server.BotRealName));
                Send(String.Format("NICK {0}", Server.BotNick));

                InitialiseBot();
                HandleInput();
            } catch (Exception e) {
                logger.Error(e);
            }

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

        public void SendPrivmsg(string nick, string msg)
        {
            Send(String.Format("PRIVMSG {0} :{1}", nick, msg));
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

        void Enqueue(IrcLine line)
        {
            if (!queue.ContainsKey(line.Nick)) {
                queue.Add(line.Nick, new List<IrcLine>());
            }
            if (queue[line.Nick].Count <= 0) {
                Send(String.Format("WHO {0} %uhnfa", line.Nick));
            }
            queue[line.Nick].Add(line);
        }

        public void RegisterCommand(string command, AbstractCommand handler)
        {
            Commands.Add(command, handler);
        }

        public Nick FetchIrcNick(string nick)
        {
            Nick ircNick = Server.Nicks.FirstOrDefault(x => x.Name == nick.ToLower());
            if (ircNick != null) {
                Server.AddNick(ircNick);
            }
            return ircNick;
        }

        public Nick FetchOrCreateIrcNick(string nick)
        {
            Nick ircNick =
                Server.Nicks.FirstOrDefault(x => x.Name == nick.ToLower())
                ?? Nick.FetchOrCreate(nick, Server);
            if (ircNick != null) {
                Server.AddNick(ircNick);
            }
            return ircNick;
        }

        Socket ConnectToNetwork()
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

            return socket;
        }

        void InitialiseBot()
        {
            List<Channel> channels = new List<Channel>();
            while (socket.Connected) {
                string line = GetLineFromSocket();
                // :NickServ!NickServ@services. NOTICE Lumbricus :This nickname is registered. Please choose a different nickname, or identify via /msg NickServ identify <password>.
                if (line.StartsWith(String.Format(@":{0} NOTICE {1} :This nickname is registered", Server.NickServHost, Server.BotNick))) {
                    logger.Debug("Identifying with NickServ");
                    SendPrivmsg(Server.NickServNick, "IDENTIFY " + Server.BotNickPassword);
                }
                if (line.StartsWith(String.Format(@":{0} NOTICE {1} :You are now identified", Server.NickServHost, Server.BotNick))) {
                    logger.Debug("Identified with NickServ, joining channels");
                    foreach (Channel channel in Server.Channels) {
                        if (channel.AutoJoin) {
                            logger.Debug("Joining " + channel.Name);
                            Send("JOIN " + channel.Name);
                            channels.Add(channel);
                        }
                    }
                    break;
                }
            }

            LoadUserDataForJoinedChannels(channels);
        }

        void LoadUserDataForJoinedChannels(List<Channel> channels)
        {
            if (socket.Connected) {
                int i = 0;
                foreach (Channel channel in channels) {
                    if (i > 0) Thread.Sleep(2000); // Wait 2 secs between sending commands, in case we're in loads of channels
                    Send(String.Format("WHO {0} %cuhnfa", channel.Name)); // 1542, Channel, Nick, User, Host, Status, Account
                    i++;
                }
            }
        }

        void HandleInput()
        {
            while (socket.Connected) {
                string line = GetLineFromSocket();

                try {
                    // PING :asimov.freenode.net
                    if (line.StartsWith("PING :")) {
                        Send("PONG" + line.Substring(4));
                        continue;
                    }

                    IrcLine ircLine = new IrcLine();

                    // This /almost/ matches the entire spec. Will update it to properly track IRC Command arguments at some point, and probably change the names of variables a bit.
                    Regex r = new Regex(@"^(:((?<fullhost>(?<nick>[^!]+)!(?<user>[^@]+)@(?<host>[^ ]+))|(?<server>([^ ]+))) )?(?<irccommand>[A-Z]+|[0-9]+)( (?<target>[^ ]+))?( :(?<fullcommand>(?<command>![^ ]+)( (?<args>.*))?)|(?<trail>.*))?$", (RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnoreCase));
                    Match m = r.Match(line);
                    if (m.Success) {
                        ircLine.RawLine     = line;

                        ircLine.FullHost    = m.Groups["fullhost"].Value;
                        ircLine.Nick        = m.Groups["nick"].Value;
                        ircLine.User        = m.Groups["user"].Value;
                        ircLine.Host        = m.Groups["host"].Value;
                        ircLine.ServerHost  = m.Groups["server"].Value;
                        ircLine.IrcCommand  = m.Groups["irccommand"].Value;
                        ircLine.Target      = m.Groups["target"].Value;
                        ircLine.FullCommand = m.Groups["fullcommand"].Value;
                        ircLine.Command     = m.Groups["command"].Value;
                        ircLine.Args        = m.Groups["args"].Value;
                        ircLine.Trail       = m.Groups["trail"].Value;

                        ProcessIrcLine(this, ircLine);
                        if (!string.IsNullOrWhiteSpace(ircLine.Command)) {
                            Enqueue(ircLine);
                        }

                        // Handles responses to WHO <nick> %na requests
                        if (HandleUserAuthResponse(ircLine)) continue;
                    }

                    // Create or update tracking rows in the DB for users in each channel
                    if (HandleUserBootstrapLine(line)) continue;

                } catch (MySqlException e) {
                    logger.Error(e);
                } catch (Exception e) {
                    logger.Error(e);
                }
            }
        }

        void InsertOrUpdateUserAccountInfo(string account, string nick, string channel, string user, string host, string status)
        {
            Account ircAccount = null;
            if (account != "0") {
                ircAccount = Server.Accounts.FirstOrDefault(x => x.Name == account);
                if (ircAccount == null) {
                    ircAccount = Account.FetchOrCreate(account, Server);
                    if (ircAccount == null) {
                        throw new Exception(String.Format("Unable to fetch or create account `{0}`", account));
                    }
                }
                if (ircAccount != null) {
                    Server.AddAccount(ircAccount);
                }
            }

            Nick ircNick = Server.Nicks.FirstOrDefault(x => x.Name == nick);
            if (ircNick == null) {
                ircNick = Nick.FetchOrCreate(nick, Server);
                if (ircNick == null) {
                    throw new Exception(String.Format("Unable to fetch or create nick `{0}`", nick));
                }
                ircNick.UserName = user;
                ircNick.Host = host;
                if (ircAccount != null && ircNick.Account == null) {
                    ircNick.Account = ircAccount;
                    LumbricusContext.db.SaveChanges();
                } else {
                    if (ircNick.Account.Id != ircAccount.Id) {
                        throw new Exception(String.Format("Data error? Nick `{0}` is signed in as account `{1}` but linked to account `{2}`", ircNick.Name, ircAccount.Name, ircNick.Account.Name));
                    }
                }
            }
            if (ircNick != null) {
                Server.AddNick(ircNick);
            }

            Channel ircChannel = Server.Channels.FirstOrDefault(x => x.Name == channel);
            if (ircChannel == null) {
                ircChannel = Channel.FetchOrCreate(channel, Server);
                if (ircChannel == null) {
                    throw new Exception(String.Format("Unable to fetch or create channel `{0}`", channel));
                }
            }
            if (ircChannel != null) {
                ircChannel.AddNick(ircNick);
            }
        }

        void Handle354(IrcLine line)
        {
            if (line.AccountName == "0") {
                if (queue.ContainsKey(line.Nick)) {
                    SendPrivmsg(line.Nick, "You must be identified with nickserv to use this command.");
                    queue.Remove(line.Nick);
                }
            } else {
                Channel channel = Server.Channels.FirstOrDefault(x => x.Name == line.Target);
                Nick ircNick = FetchIrcNick(line.Nick);
                if (ircNick == null) {
                    if (queue.ContainsKey(line.Nick)) {
                        queue.Remove(line.Nick);
                    }
                    throw new Exception(String.Format("Unable to fetch or create nick `{0}`", line.Nick));
                }

                bool found = false;
                foreach (Channel c in Server.Channels) {
                    if (c.Nicks.Contains(ircNick) || ircNick.channels.Contains(c)) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    List<string> channels = new List<string>();
                    foreach (Channel c in Server.Channels) {
                        channels.Add(c.Name);
                    }
                    if (queue.ContainsKey(line.Nick)) {
                        queue.Remove(line.Nick);
                    }
                    logger.Info("Ignoring {0} because they're not in at least one of the following channels: {1}", line.Nick, string.Join(", ", channels));
                    return;
                }

                Account ircAccount = Account.FetchOrCreate(line.AccountName, Server);
                if (ircAccount == null) {
                    if (queue.ContainsKey(line.Nick)) {
                        queue.Remove(line.Nick);
                    }
                    throw new Exception(String.Format("Unable to fetch or create account `{0}`", line.AccountName));
                }

                if (ircNick.Account == null) {
                    ircNick.Account = ircAccount;
                    LumbricusContext.db.SaveChanges();
                } else if (ircNick.Account.Id != ircAccount.Id && queue.ContainsKey(line.Nick)) {
                    SendPrivmsg(line.Nick, "Sorry, but nick `{0}` is not registered to your services account. Please log in on that account and try again.");
                    SendPrivmsg("TwoWholeWorms", String.Format("Services account `{0}` attempted to access features for nick `{1}` but that nick is registered to services account `{2}`.", ircAccount.Name, ircNick.Name, ircNick.Account.Name));
                    if (queue.ContainsKey(line.Nick)) {
                        queue.Remove(line.Nick);
                    }
                    return;
                }

                if (queue.ContainsKey(line.Nick)) {
                    if (CheckBans(ircNick, ircAccount, line.User, line.Host)) {
                        if (queue.ContainsKey(line.Nick)) {
                            queue.Remove(line.Nick);
                        }
                        return;
                    }

                    foreach (IrcLine queuedLine in queue.Single(x => x.Key == line.Nick).Value) {
                        if (!string.IsNullOrWhiteSpace(queuedLine.Command)) {
                            if (Commands.ContainsKey(queuedLine.Command)) {
                                Commands.Single(x => x.Key == line.Command).Value.HandleCommand(queuedLine, ircNick, channel);
                            } else {
                                SendPrivmsg(line.Nick, String.Format("Sorry, {0}, but that command does not exist. Try !help.", line.Nick));
                            }
                        }

//                            switch (command) {
//                                case "setmugshot":
//                                    Command.HandleSetMugshot(ircNick, args, this);
//                                    break;
//
//                                case "clearmugshot":
//                                    Command.HandleClearMugshot(ircNick, args, this);
//                                    break;
//
//                                case "setinfo":
//                                    Command.HandleSetInfo(ircNick, args, this);
//                                    break;
//
//                                case "clearinfo":
//                                    Command.HandleClearInfo(ircNick, args, this);
//                                    break;
//
//                                case "seen":
//                                    Command.HandleSeen(ircNick, args, this, c);
//                                    break;
//
//                                case "help":
//                                    Command.HandleHelp(ircNick, args, this, c);
//                                    break;
//
//                                case "baninfo":
//                                    Command.HandleBanInfo(ircNick, args, this, c);
//                                    break;
//
//                                case "botban":
//                                    Command.HandleBotBan(ircNick, args, this, c);
//                                    break;
//
//                                case "searchlog":
//                                    Command.HandleSearchLog(ircNick, args, this, c);
//                                    break;
//
//                                case "restart":
//                                    Command.HandleUnwrittenAdminCommand(ircNick, args, this, c);
//                                    break;
//
                        if (channel != null && !channel.AllowCommandsInChannel && queuedLine.Target != Server.BotNick) {
                            SendPrivmsg(line.Nick, "Also, please try to only interact with me directly through this window. Bot commands in the channel are against channel policy, and some people get really annoyed about it. :(");
                        }
                    }
                    queue.Remove(line.Nick);
                }
            }
        }

        public Channel GetChannel(string name)
        {
            Channel channel = null;

            if (name.StartsWith("#")) {
                channel = Channel.Fetch(name, Server);
            }

            return channel;
        }

        long GetChannelId(string name)
        {
            Channel channel = GetChannel(name);
            return channel != null ? channel.Id : 0;
        }

        bool HandleUserBootstrapLine(string line)
        {
            Regex r = new Regex(@":(?<server>[a-z0-9]+(\.[a-z0-9]+)+) 354 ([^ ]+) (?<channel>##?[^ ]+) (?<user>[^ ]+) (?<host>[^ ]+) (?<nick>[^ ]+) (?<status>[^ ]+) (?<account>[^ ]+)\s*$", RegexOptions.ExplicitCapture);
            Match m = r.Match(line);
            if (m.Success) {
                string account = m.Groups["account"].Value;
                string nick    = m.Groups["nick"].Value;
                string user    = m.Groups["user"].Value;
                string host    = m.Groups["host"].Value;
                string status  = m.Groups["status"].Value;
                string channel = m.Groups["channel"].Value;

                InsertOrUpdateUserAccountInfo(account, nick, channel, user, host, status);
                return true;
            }
            return false;
        }

        bool HandleUserAuthResponse(IrcLine line)
        {
            Regex r = new Regex(@":(?<server>[a-z0-9]+(\.[a-z0-9]+)+) 354 ([^ ]+) (?<user>[^ ]+) (?<host>[^ ]+) (?<nick>[^ ]+) (?<status>[^ ]+) (?<account>[^ ]+)\s*$", RegexOptions.ExplicitCapture);
            Match m = r.Match(line.RawLine);
            if (m.Success) {
                line.AccountName = m.Groups["account"].Value;
                line.Nick        = m.Groups["nick"].Value;
                line.User        = m.Groups["user"].Value;
                line.Host        = m.Groups["host"].Value;

                Handle354(line);
                return true;
            }
            return false;
        }

        static bool CheckBans(Nick nick, Account account, string user, string host)
        {
            if (string.IsNullOrEmpty(nick.UserName)) {
                nick.UserName = user;
            }
            if (string.IsNullOrEmpty(nick.Host)) {
                nick.Host = host;
            }
            Ban ban = Ban.Fetch(nick, account);
            if (ban != null && ban.IsActive) {
                logger.Info("Ignoring {0} because they're banned by ban id {1}.", nick, ban.Id);
                return true;
            }
            return false;
        }

	}

}
