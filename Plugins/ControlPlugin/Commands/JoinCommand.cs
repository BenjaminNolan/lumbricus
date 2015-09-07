using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.ControlPlugin.Commands
{

    public class JoinCommand : AbstractCommand
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public JoinCommand(IrcConnection conn) : base(conn)
        {
            // …
        }

        public override string Name {
            get {
                return "Join Command";
            }
        }

        public override void HandleCommand(IrcLine line, Nick nick, Channel channel)
        {
            try {
                if (!isOp(nick)) {
                    conn.SendPrivmsg(nick.Name, String.Format("Sorry, {0}, but that command doesn't exist. Try !help.", nick.DisplayName));
                    return;
                }

                string target = nick.Name;
                if (channel != null && channel.AllowCommandsInChannel) {
                    target = channel.Name;
                }

                Regex r = new Regex(@"^!join ?");
                line.Args = r.Replace(line.Args, "").Trim();
                if (line.Args.Length <= 1) { // Whaaaat??
                    conn.SendPrivmsg(target, String.Format("Usage(1): !join <channel>"));
                } else {
                    r = new Regex(@"^#+[a-zA-Z0-9_`\-]+$");
                    Match m = r.Match(line.Args);
                    if (!m.Success) {
                        conn.SendPrivmsg(target, String.Format("Usage(1): !join <channel> — Channel should start with a # and contain no spaces"));
                    } else {
                        Channel ircChannel = conn.Server.ConnectedChannels.FirstOrDefault(x => x.Name == line.Args.ToLower());
                        if (ircChannel != null) {
                            conn.SendPrivmsg(target, String.Format("Uh… {0}… I'm already /in/ {1}… o.o", nick.DisplayName, line.Args));
                        } else {
                            ircChannel = Channel.FetchOrCreate(line.Args, conn.Server);
                            if (ircChannel != null) {
                                List<Channel> channels = new List<Channel>();
                                logger.Debug("Joining " + ircChannel.Name);
                                conn.SendPrivmsg(target, String.Format("Joining {0}! :D", line.Args));
                                conn.Send("JOIN " + ircChannel.Name);
                                channels.Add(ircChannel);
                                conn.LoadUserDataForJoinedChannels(channels);
                            } else {
                                conn.SendPrivmsg(target, String.Format("Sorry, {0}, but I couldn't join `{1}` as I couldn't fetch it from the database.", nick.DisplayName, line.Args));
                            }
                        }
                    }
                }
            } catch (Exception e) {
                logger.Error(e);
                conn.SendPrivmsg(nick.Name, "¡Ay caray! It's Lupus! D:");
            }
        }

    }

}
