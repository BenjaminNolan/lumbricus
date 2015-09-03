using System;
using System.Linq;
using System.Text.RegularExpressions;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;
using TwoWholeWorms.Lumbricus.Shared.Model;

namespace TwoWholeWorms.Lumbricus.Shared.Plugins.Core
{

    public class TrackBanPlugin : AbstractPlugin
    {

        #region AbstractPlugin implementation

        public override void RegisterPlugin(IrcConnection conn)
        {
            conn.ProcessIrcLine += DoTrackBanPlugin;
        }

        public override string Name {
            get {
                return "Track Ban Plugin";
            }
        }

        #endregion

        // This needs to be updated to properly parse mode lines.
        public void DoTrackBanPlugin(IrcConnection conn, IrcLine line)
        {
            Regex r = new Regex(@"^(?<banchannel>#[^ ]+) (?<bantype>[\+\-]b) (?<banmask>.*)$", RegexOptions.ExplicitCapture);
            Match m = r.Match(line.IrcCommandArgsRaw);

            if (m.Success) {
                string banChannel = m.Groups["banchannel"].Value;
                string banMask    = m.Groups["banmask"].Value;
                string banType    = m.Groups["bantype"].Value;

                Nick banner = conn.Server.ConnectedNicks.FirstOrDefault(x => x.Name == line.Nick.ToLower());
                Channel channel = conn.Server.ConnectedChannels.FirstOrDefault(x => x.Name == banChannel.ToLower());
                Ban ban = Ban.Fetch(channel, line.FullHost);
                if (ban == null) {
                    if (banType == "-b") {
                        return;
                    }
                    ban = new Ban();
                    if (banMask.StartsWith("$a:")) {
                        string[] maskBits = banMask.Split(" ".ToCharArray(), 2);
                        Account bannedAccount = Account.Fetch(maskBits[1], conn.Server);
                        if (bannedAccount != null) {
                            ban.Account = bannedAccount;
                        }
                    }
                } else {
                    if (banType == "+b" && ban.IsActive) {
                        // Don't overwrite an existing ban if it's still active
                        return;
                    }
                    if (banType == "+b" && ban.UnbannerAccount != null) {
                        // Create a new ban if they've been unbanned
                        ban = Ban.Create(conn.Server);
                    }
                }

                if (banType == "-b") {
                    ban.IsActive = false;
                    ban.UnbannedAt = DateTime.Now;
                }
                ban.Server = conn.Server;
                if (banner.Account != null) {
                    ban.BannerAccount = banner.Account;
                }
                ban.Channel = channel;
                ban.Mask = banMask;

                LumbricusContext.db.SaveChanges();
            }
        }

    }

}
