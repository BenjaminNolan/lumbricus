using NLog;
using System;
using System.Threading;
using System.Linq;
using System.Text.RegularExpressions;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.OpsPlugin.Commands
{

    public class OpsCommand : AbstractCommand
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public OpsCommand(IrcConnection conn) : base(conn)
        {
            // …
        }

        public override string Name {
            get {
                return "Ops Command";
            }
        }

        public override void HandleCommand(IrcLine line, Nick nick, Channel channel)
        {
            try {
                Regex r = new Regex(@"^!ops ?");
                line.Args = r.Replace(line.Args, "").Trim();

                string channelName = "a private message";
                if (channel != null) {
                    channelName = channel.Name;
                }

                Setting opsSetting = Setting.Fetch("Ops", "Nicks");
                if (opsSetting != null) {
                    string[] nicks = opsSetting.Value.Split(" ".ToArray());
                    for (int i = 0; i < nicks.Length; i++) {
                        if (i > 2) {
                            Thread.Sleep(1000);
                        }
                        conn.SendPrivmsg(nicks.GetValue(i).ToString(), string.Format("`{0}` is calling for the attention of a channel op in `{1}`.", nick.DisplayName, channelName));
                    }
                }

                Setting opsChannelSetting = Setting.Fetch("Ops", "Channels");
                if (opsChannelSetting != null) {
                    string[] channels = opsChannelSetting.Value.Split(" ".ToArray());
                    for (int i = 0; i < channels.Length; i++) {
                        if (i > 2) {
                            Thread.Sleep(1000);
                        }
                        string c = channels.GetValue(i).ToString();
                        Channel opsChannel = conn.Server.ConnectedChannels.FirstOrDefault(x => x.Name == c);
                        if (opsChannel != null) {
                            string nicks = "";
                            foreach (Nick n in opsChannel.ConnectedNicks) {
                                if (n.Name == conn.Server.BotNick.ToLower()) continue;
                                if (nicks.Length > 0) nicks += ", ";
                                nicks += n.DisplayName;
                            }
                            conn.SendPrivmsg(opsChannel.Name, string.Format("{0}: `{1}` is calling for the attention of a channel op in `{2}`.", nicks, nick.DisplayName, channelName));
                        }
                    }
                }
            } catch (Exception e) {
                logger.Error(e);
                conn.SendPrivmsg(nick.Name, "¡Ay díos! It's /not/ Lupus! D:");
            }
        }

    }

}
