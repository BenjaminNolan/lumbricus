using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.ControlPlugin.Commands
{

    public class PartCommand : AbstractCommand
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public PartCommand(Connection conn) : base(conn)
        {
            // …
        }

        public override string Name {
            get {
                return "Part Command";
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

                Regex r = new Regex(@"^!part ?");
                line.Args = r.Replace(line.Args, "").Trim();
                if (line.Args.Length <= 1) { // Whaaaat??
                    conn.SendPrivmsg(target, String.Format("Usage(1): !part <channel>"));
                } else {
                    r = new Regex(@"^#+[a-zA-Z0-9_`\-]+$");
                    Match m = r.Match(line.Args);
                    if (!m.Success) {
                        conn.SendPrivmsg(target, String.Format("Usage(1): !part <channel> — Channel should start with a # and contain no spaces"));
                    } else {
                        Channel ircChannel = conn.Server.ConnectedChannels.FirstOrDefault(x => x.Name == line.Args.ToLower());
                        if (ircChannel == null) {
                            conn.SendPrivmsg(target, String.Format("Uh… {0}… I'm not /in/ {1}… o.o", nick.DisplayName, line.Args));
                        } else {
                            conn.SendPrivmsg(target, String.Format("Parting {0}. :(", line.Args));
                            logger.Debug("Parting " + ircChannel.Name);
                            conn.Send("PART " + ircChannel.Name);
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
