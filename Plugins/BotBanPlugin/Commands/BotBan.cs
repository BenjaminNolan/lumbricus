using NLog;
using System;
using TwoWholeWorms.Lumbricus.Shared;
using System.Text.RegularExpressions;
using TwoWholeWorms.Lumbricus.Shared.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.BotBanPlugin.Commands
{

    public class BotBan : AbstractCommand
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public BotBan(IrcConnection conn) : base(conn)
        {
            // …
        }

        public override string Name {
            get {
                return "Set Info Command";
            }
        }

        public override void HandleCommand(IrcLine line, Nick nick, Channel channel)
        {
            try {
                if (nick.Account == null || !isOp(nick)) {
                    conn.SendPrivmsg(nick.Name, String.Format("Sorry, {0}, but that command doesn't exist. Try !help.", nick.DisplayName));
                    return;
                }

                string target = nick.Name;
                if (channel != null && channel.AllowCommandsInChannel) {
                    target = channel.Name;
                }

                Regex r = new Regex(@"^!botban ?");
                line.Args = r.Replace(line.Args, "").Trim();
                if (line.Args.Length <= 0) { // Whaaaat??
                    conn.SendPrivmsg(target, "Usage(1): !botban <add|remove> <nick|$a:account|mask:*!*@*.*> [ban message]");
                } else {
                    string[] argBits = line.Args.Split(" ".ToCharArray(), 3);
                    if (argBits.Length > 3) {
                        conn.SendPrivmsg(target, "Usage(2): !botban <add|remove> <nick|$a:account|mask:*!*@*.*> [ban message]");
                        return;
                    }

                    string banMessage = null;
                    if (argBits.Length == 3) {
                        banMessage = argBits[2];
                    }

                    string mode = argBits[0];
                    switch (mode) {
                        case "add":
                        case "remove":
                            break;

                            //                        case "remove":
                            //                            if (banMessage != null) {
                            //                                conn.SendPrivmsg(target, "Ban message can't be set when removing a ban, you DERP!");
                            //                                return;
                            //                            }
                            //                            break;

                        default:
                            conn.SendPrivmsg(target, "Usage(3): !botban <add|remove> <nick|$a:account|mask:*!*@*.*> [ban message]");
                            break;
                    }

                    argBits = argBits[1].Split(':');
                    string search = argBits[0];
                    string searchType = "nick";
                    if (argBits.Length > 2 || (argBits.Length == 2 && argBits[0] != "$a" && argBits[0] != "mask")) {
                        conn.SendPrivmsg(target, "Usage(4): !botban <add|remove> <nick|$a:account|mask:*!*@*.*> [ban message]");
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
                            conn.SendPrivmsg(target, "Usage(5): !botban <add|remove> <nick|$a:account|mask:*!*@*.*> [ban message]");
                            return;
                        }
                    }

                    Ban ircBan = null;
                    Nick ircNick = null;
                    Account ircAccount = null;
                    switch (searchType) {
                        case "nick":
                            ircNick = Nick.Fetch(search, conn.Server);
                            if (ircNick == null) {
                                conn.SendPrivmsg(target, String.Format("{0}{2}{1}: Nick `{3}` does not exist in the database.", "\x02", "\x0f", nick.DisplayName, search));
                                return;
                            }
                            ircAccount = ircNick.Account;
                            ircBan = Ban.Fetch(ircNick, ircAccount);
                            break;

                        case "account":
                            ircAccount = Account.Fetch(search, conn.Server);
                            if (ircAccount == null) {
                                conn.SendPrivmsg(target, String.Format("{0}{2}{1}: Account `{3}` does not exist in the database.", "\x02", "\x0f", nick.DisplayName, search));
                                return;
                            }
                            ircNick = ircAccount.MostRecentNick;
                            ircBan = Ban.Fetch(ircNick, ircAccount);
                            break;

                        case "mask":
                            ircBan = Ban.Fetch(conn.Server, search);

                            if (ircBan != null) {
                                if (ircBan.Nick != null) ircNick = ircBan.Nick;
                                if (ircBan.Account != null) ircAccount = ircBan.Account;
                            }

                            break;

                        default:
                            conn.SendPrivmsg(target, "This what you're seeing here is a big fat error happening. Poke TwoWholeWorms.");
                            break;
                    }

                    if (ircBan == null) {
                        if (mode == "remove") {
                            conn.SendPrivmsg(target, String.Format("{0}{2}{1}: {3} `{4}` is not currently banned.", "\x02", "\x0f", nick.DisplayName, searchType.Capitalise(), search));
                            return;
                        }
                        ircBan = new Ban();
                    } else if (mode == "add" && ircBan.IsMugshotBan) {
                        Account banner = ircBan.BannerAccount;
                        if (banner != null) {
                            conn.SendPrivmsg(target, String.Format("{0}{2}{1}: {3} `{4}` was already banned from accessing {8} by $a:{5} on {6}: {7}", "\x02", "\x0f", nick.DisplayName, searchType.Capitalise(), search, banner.Name, ircBan.CreatedAt.ToString("u"), (ircBan.BanMessage ?? "No message"), conn.Server.BotNick));
                        } else {
                            conn.SendPrivmsg(target, String.Format("{0}{2}{1}: {3} `{4}` was already banned from accessing {7} on {5}: {6}", "\x02", "\x0f", nick.DisplayName, searchType.Capitalise(), search, ircBan.CreatedAt.ToString("u"), (ircBan.BanMessage ?? "No message"), conn.Server.BotNick));
                        }
                        return;
                    }

                    if (ircNick == null && ircAccount != null && ircAccount.MostRecentNick != null) {
                        ircNick = ircAccount.MostRecentNick;
                    }
                    if (ircAccount == null && ircNick != null && ircNick.Account != null) {
                        ircAccount = ircNick.Account;
                    }

                    ircBan.Server = conn.Server;
                    ircBan.Nick = ircNick;
                    ircBan.Account = ircAccount;
                    ircBan.IsMugshotBan = (mode == "add");
                    if (ircBan.IsActive && ircBan.Channel == null) ircBan.IsActive = false;
                    if (mode == "add") {
                        ircBan.BanMessage = banMessage;
                        ircBan.BannerAccount = nick.Account;
                        ircBan.UnbanMessage = null;
                        ircBan.UnbannerAccount = null;
                        ircBan.UnbannedAt = null;
                    } else if (mode == "remove") {
                        ircBan.UnbanMessage = banMessage;
                        ircBan.UnbannerAccount = nick.Account;
                        ircBan.UnbannedAt = DateTime.Now;
                    }
                    if (searchType == "mask") {
                        ircBan.Mask = search;
                    }
                    ircBan.Save();

                    if (mode == "add") {
                        conn.SendPrivmsg(target, String.Format("{0}{2}{1}: {3} `{4}` has been banned from accessing {5} features, and their existing mugshot has been hidden from the site.", "\x02", "\x0f", nick.DisplayName, searchType.Capitalise(), search, conn.Server.BotNick));
                    } else {
                        conn.SendPrivmsg(target, String.Format("{0}{2}{1}: {3} `{4}` ban on accessing {5} has been deactivated.", "\x02", "\x0f", nick.DisplayName, searchType.Capitalise(), search, conn.Server.BotNick));
                    }
                    return;
                }
            } catch (Exception e) {
                logger.Error(e);
                conn.SendPrivmsg(nick.Name, "Aieee! The bees! The BEEES! They're chasing me away!!! (Get TwoWholeWorms!)");
            }
        }

    }

}
