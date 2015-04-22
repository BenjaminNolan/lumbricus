using System;
using System.Linq;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Shared.Plugins.Core
{

    public class TrackUserPlugin : AbstractPlugin
    {

        #region AbstractPlugin implementation

        public override void RegisterPlugin(IrcConnection conn)
        {
            conn.ProcessIrcLine += DoTrackUserPlugin;
        }

        public override string Name {
            get {
                return "Track User Plugin";
            }
        }

        #endregion

        public void DoTrackUserPlugin(IrcConnection conn, IrcLine line)
        {
            Channel c = null;
            Nick nick;
            IrcCommand ircCommand = line.IrcCommand.ToEnum<IrcCommand>();
            switch (ircCommand) {
                case IrcCommand.JOIN:
                    nick = conn.FetchOrCreateIrcNick(line.Nick);
                    c = conn.Server.Channels.FirstOrDefault(x => x.Name == line.Target);
                    if (c != null) {
                        c.AddNick(nick);
                        nick.User = line.User;
                        nick.Host = line.Host;
                        if (nick.Account != null) {
                            conn.Server.AddAccount(nick.Account);
                        }
                        if (line.Nick != conn.Server.BotNick && nick.Account == null) {
                            conn.Send(String.Format("WHO {0} %uhnfa", line.Nick));
                        }
                    }
                    break;
                case IrcCommand.PART:
                    nick = conn.FetchIrcNick(line.Nick);
                    c = conn.Server.Channels.FirstOrDefault(x => x.Name == line.Target);
                    if (c != null) {
                        c.RemoveNick(nick);
                    }
                    if (nick.channels.Count <= 0) {
                        nick.Dispose();
                    }
                    break;
                case IrcCommand.QUIT:
                    nick = conn.FetchIrcNick(line.Nick);
                    nick.Dispose();
                    break;
                case IrcCommand.NICK:
                    nick = conn.FetchIrcNick(line.Nick);
                    Nick newNick = conn.FetchOrCreateIrcNick(line.Trail);
                    if (newNick != null) {
                        foreach (Channel channel in nick.channels) {
                            newNick.AddChannel(channel);
                        }
                        newNick.User = line.User;
                        newNick.Host = line.Host;
                        conn.Server.AddNick(newNick);
                        if (nick.Account != null && newNick.Account != null && nick.Account.Id != newNick.Account.Id) {
                            Setting opsChannelSetting = Setting.Fetch("ops", "channel");
                            if (opsChannelSetting != null) {
                                conn.SendPrivmsg(opsChannelSetting.Value, String.Format("\x02`{0}`\x0f ($a:\x02{1}\x0f) changed their nick to \x02`{2}`\x0f ($a:\x02{3}\x0f).", nick.Name, nick.Account.Name, newNick.Name, newNick.Account.Name));
                            }
                        }
                        if (newNick.Account != null) {
                            conn.Server.AddAccount(newNick.Account);
                        } else {
                            conn.Send(String.Format("WHO {0} %uhnfa", line.Nick));
                        }
                    }
                    nick.Dispose();
                    break;
            }
        }

    }

}
