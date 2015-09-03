using System;
using System.Linq;
using System.Text.RegularExpressions;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

namespace TwoWholeWorms.Lumbricus.Shared.Plugins.Core
{

    public class TrackKickPlugin : AbstractPlugin
    {

        #region AbstractPlugin implementation

        public override void RegisterPlugin(IrcConnection conn)
        {
            conn.ProcessIrcLine += DoTrackKickPlugin;
        }

        public override string Name {
            get {
                return "Track Kick Plugin";
            }
        }

        #endregion

        public void DoTrackKickPlugin(IrcConnection conn, IrcLine line)
        {
            Regex r = new Regex(@"^(?<channel>#[^ ]+) (?<idiot>.*)$", RegexOptions.ExplicitCapture);
            Match m = r.Match(line.IrcCommandArgsRaw);

            if (m.Success) {
                string channelName = m.Groups["channel"].Value;
                string idiot       = m.Groups["idiot"].Value;

                Nick banner = conn.Server.ConnectedNicks.FirstOrDefault(x => x.Name == line.Nick.ToLower());
                Nick idiotNick = conn.Server.ConnectedNicks.FirstOrDefault(x => x.Name == idiot.ToLower());
                Channel channel = conn.Server.ConnectedChannels.FirstOrDefault(x => x.Name == channelName.ToLower());

                Ban ban = Ban.Fetch(channel, idiotNick);
                if (ban != null) {
                    if (ban.BannerAccount == null && banner.Account != null) {
                        ban.BannerAccount = banner.Account;
                    }
                    ban.Nick = idiotNick;
                    if (idiotNick.Account != null) ban.Account = idiotNick.Account;
                    ban.Channel = channel;
                    ban.BanMessage = line.Trail;

                    LumbricusContext.db.SaveChanges();
                }
            }
        }

    }

}
