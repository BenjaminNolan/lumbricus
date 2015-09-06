using NLog;
using System;
using System.Text.RegularExpressions;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;

using SeenModel = TwoWholeWorms.Lumbricus.Shared.Model.Seen;

namespace TwoWholeWorms.Lumbricus.Shared.Plugins.Core.Commands
{

    public class SeenCommand : AbstractCommand
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public SeenCommand(IrcConnection conn) : base(conn)
        {
            // …
        }

        public override string Name {
            get {
                return "Seen Command";
            }
        }

        public override void HandleCommand(IrcLine line, Nick nick, Channel channel)
        {
            try {
                if (nick.Account == null || !isOp(nick)) {
                    conn.SendPrivmsg(nick.Name, String.Format("Sorry, {0}, but the !seen command doesn't exist. Try !help.", nick.DisplayName));
                    return;
                }

                string target = nick.Name;
                if (channel != null && channel.AllowCommandsInChannel) {
                    target = channel.Name;
                }

                Regex r = new Regex(@"^!seen ?");
                line.Args = r.Replace(line.Args, "").Trim();
                if (line.Args.Length <= 0) { // Whaaaat??
                    conn.SendPrivmsg(target, String.Format("Usage: !seen <search> - use $a:account to search for an account (eg, !seen $a:{0})", nick.Account.Name));
                } else {
                    string[] argBits = line.Args.Split(' ');
                    if (argBits.Length > 1) {
                        conn.SendPrivmsg(target, String.Format("Usage: !seen <search> - use $a:account to search for an account (eg, !seen $a:{0})", nick.Account.Name));
                        return;
                    }

                    argBits = argBits[0].Split(':');
                    string search = argBits[0];
                    string searchType = "nick";
                    if (argBits.Length == 2) {
                        if (argBits[0] != "$a") {
                            conn.SendPrivmsg(target, String.Format("Usage: !seen <search> - use $a:account to search for an account (eg, !seen $a:{0})", nick.Account.Name));
                            return;
                        }
                        search = argBits[1];
                        searchType = "account";
                    }

                    Nick ircNick = null;
                    Account ircAccount = null;
                    Channel ircChannel = null;
                    SeenModel ircSeen = null;
                    if (searchType == "account") {
                        ircAccount = Account.Fetch(search.ToLower(), conn.Server);
                        if (ircAccount != null) {
                            ircSeen = SeenModel.FetchByAccountId(ircAccount.Id);
                            if (ircSeen != null) {
                                if (ircSeen.Nick != null) {
                                    ircNick = ircSeen.Nick;
                                }
                                if (ircSeen.Channel != null) {
                                    ircChannel = ircSeen.Channel;
                                }
                            }
                        }
                    } else {
                        ircNick = Nick.Fetch(search.ToLower(), conn.Server);
                        if (ircNick != null) {
                            ircSeen = SeenModel.FetchByNickId(ircNick.Id);
                            if (ircSeen != null) {
                                if (ircSeen.Account != null) {
                                    ircAccount = ircSeen.Account;
                                }
                                if (ircSeen.Channel != null) {
                                    ircChannel = ircSeen.Channel;
                                }
                            }
                        }
                    }

                    if (ircSeen == null) {
                        conn.SendPrivmsg(target, String.Format("There is no seen data in the database about {0} `{1}`.", searchType, search));
                    } else {
                        string seenNick = (ircNick != null ? ircNick.DisplayName : "Unknown Nick");
                        string seenAccount = (ircAccount != null ? ircAccount.Name : "Unknown Account");
                        string seenChannel = (ircChannel != null ? ircChannel.Name : "a private query window");

                        conn.SendPrivmsg(target, String.Format("{1}{0}{2}{1} ($a:{0}{3}{1}): First seen {0}{4}{1}, last seen {0}{5}{1} in {1}{0}{6}{1}.", "\x02", "\x0f", seenNick, seenAccount, ircSeen.FirstSeenAt.ToString("u"), ircSeen.LastSeenAt.ToString("u"), seenChannel));
                    }
                }
            } catch (Exception e) {
                logger.Error(e);
                conn.SendPrivmsg(nick.Name, "Oof… My spleen…! I can't do that right now. :(");
            }
        }

    }

}
