using NLog;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using TwoWholeWorms.Lumbricus.Shared;
using System.Text.RegularExpressions;
using TwoWholeWorms.Lumbricus.Shared.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.BanInfoPlugin.Commands
{

    public class BanInfoCommand : AbstractCommand
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public BanInfoCommand(IrcConnection conn) : base(conn)
        {
            // …
        }

        public override string Name {
            get {
                return "Ban Info Command";
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

                Regex r = new Regex(@"^!baninfo ?");
                line.Args = r.Replace(line.Args, "").Trim();
                if (line.Args.Length <= 0) { // Whaaaat??
                    conn.SendPrivmsg(target, String.Format("Usage(1): !baninfo <search> - Prefix search with $a: to search for an account, or mask: to search for a ban string, eg !baninfo $a:{0} !baninfo mask:*!*@*.no", nick.Account.Name));
                } else {
                    string[] argBits = line.Args.Split(' ');
                    if (argBits.Length > 1) {
                        conn.SendPrivmsg(target, String.Format("Usage(2): !baninfo <search> - Prefix search with $a: to search for an account, or mask: to search for a ban string, eg !baninfo $a:{0} !baninfo mask:*!*@*.no", nick.Account.Name));
                        return;
                    }

                    argBits = argBits[0].Split(':');
                    string search = argBits[0];
                    string searchType = "nick";
                    if (argBits.Length > 2 || (argBits.Length == 2 && argBits[0] != "$a" && argBits[0] != "mask")) {
                        conn.SendPrivmsg(target, String.Format("Usage(3): !baninfo <search> - Prefix search with $a: to search for an account, or mask: to search for a ban string, eg !baninfo $a:{0} !baninfo mask:*!*@*.no", nick.Account.Name));
                        return;
                    }
                    if (argBits.Length == 2) {
                        if (argBits[0] == "mask") {
                            search = argBits[1];
                            searchType = "mask";
                        } else if (argBits[0] == "$a") {
                            search = argBits[1];
                            searchType = "account";
                        } else {
                            conn.SendPrivmsg(target, String.Format("Usage(4): !baninfo <search> - Prefix search with $a: to search for an account, or mask: to search for a ban string, eg !baninfo $a:{0} !baninfo mask:*!*@*.no", nick.Account.Name));
                            return;
                        }
                    }

                    Account bannedAccount = null;
                    Nick bannedNick = null;
                    List<Ban> ircBans = new List<Ban>();
                    switch (searchType) {
                        case "account":
                            bannedAccount = Account.Fetch(search.ToLower(), conn.Server);
                            if (bannedAccount == null) {
                                conn.SendPrivmsg(target, String.Format("There is no account `{0}` in the database", search));
                                return;
                            }
                            bannedNick = bannedAccount.MostRecentNick;
                            bannedNick.Account = bannedAccount;
                            ircBans = Ban.Fetch(bannedNick).ToList();
                            break;

                        case "nick":
                            bannedNick = Nick.Fetch(search.ToLower(), conn.Server);
                            if (bannedNick == null) {
                                conn.SendPrivmsg(target, String.Format("There is no nick `{0}` in the database", search));
                                return;
                            }
                            bannedAccount = bannedNick.Account;
                            ircBans = Ban.Fetch(bannedNick).ToList();
                            break;

                        case "mask":
                            Ban ircBan = Ban.Fetch(conn.Server, search);
                            if (ircBan == null) {
                                conn.SendPrivmsg(target, String.Format("There is no ban on mask `{0}` in the database", search));
                                return;
                            }
                            if (ircBan.Account != null) bannedAccount = ircBan.Account;
                            if (ircBan.Nick != null) bannedNick = ircBan.Nick;

                            if (bannedAccount != null && bannedNick == null && bannedAccount.MostRecentNick != null) {
                                bannedNick = bannedAccount.MostRecentNick;
                            }
                            if (bannedNick != null && bannedAccount == null && bannedNick.Account != null) {
                                bannedAccount = bannedNick.Account;
                            }

                            if (ircBan != null) {
                                ircBans.Add(ircBan);
                            }
                            break;
                    }

                    if (ircBans.Count < 1) {
                        if (bannedNick != null && bannedAccount != null) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: {2} ($a:{1}) is not banned.", nick.DisplayName, bannedAccount.Name, bannedNick.DisplayName));
                        } else if (bannedAccount != null) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: $a:{1} is not banned.", nick.DisplayName, bannedAccount.Name));
                        } else if (bannedNick != null) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: `{1}` is not banned.", nick.DisplayName, bannedNick.DisplayName));
                        } else {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: This message should never appear, what the HELL did you DO to me?! D:", nick.DisplayName));
                        }
                        return;
                    }

                    int i = 0;
                    foreach (Ban ircBan in ircBans) {
                        string response = String.Format("\x02{0}\x0f: #{1}", nick.DisplayName, ircBan.Id);
                        if (searchType == "account") {
                            response += " $a:" + bannedAccount.Name;
                            if (bannedNick != null) {
                                response += " (" + bannedNick.DisplayName + ")";
                            }
                        } else {
                            response += " " + bannedNick.DisplayName;
                            if (bannedAccount != null) {
                                response += " ($a:" + bannedAccount.Name + ")";
                            }
                        }

                        Channel bannedChannel = ircBan.Channel;
                        response += " " + conn.Server.BotNick + ((ircBan.IsMugshotBan || (bannedChannel != null && ircBan.IsActive)) ? ":" : ":un") + "banned";
                        if (bannedChannel != null) {
                            response += " " + bannedChannel.Name + (!ircBan.IsActive ? ":un" : ":") + "banned";
                        }
                        response += " ban:[" + ircBan.CreatedAt.ToString("u");

                        if (!string.IsNullOrEmpty(ircBan.Mask)) {
                            response += " mask:" + ircBan.Mask;
                        }

                        Account cantankerousOp = ircBan.BannerAccount;
                        if (cantankerousOp != null) {
                            if (cantankerousOp.MostRecentNick != null) {
                                response += " op:" + cantankerousOp.MostRecentNick.DisplayName;
                            } else {
                                response += " op:$a:" + cantankerousOp.Name;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(ircBan.BanMessage)) {
                            response += " reason:" + ircBan.BanMessage;
                        }
                        response += "]";

                        if (ircBan.UnbannerAccount != null) {
                            Account unbanner = ircBan.UnbannerAccount;

                            response += " unban:[" + ircBan.UnbannedAt.Value.ToString("u");
                            if (unbanner != null) {
                                if (unbanner.MostRecentNick != null) {
                                    response += " op:" + unbanner.MostRecentNick.DisplayName;
                                } else {
                                    response += " op:$a:" + unbanner.Name;
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(ircBan.UnbanMessage)) {
                                response += " msg:" + ircBan.UnbanMessage;
                            }
                            response += "]";
                        }
                        if (i > 2) Thread.Sleep(2000); // Wait 2 secs between sending commands after 3, in case there's loads of bans to send
                        conn.SendPrivmsg(target, response);
                        i++;

                        if (i > 5) break;
                    }
                }
            } catch (Exception e) {
                logger.Error(e);
                conn.SendPrivmsg(nick.Name, "Oof… My spleen…! I can't do that right now. :(");
            }
        }

    }

}
