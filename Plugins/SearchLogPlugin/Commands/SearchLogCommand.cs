using NLog;
using System;
using System.Linq;
using TwoWholeWorms.Lumbricus.Shared;
using System.Text.RegularExpressions;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Plugins.IrcLogPlugin.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.SearchLogPlugin.Commands
{

    public class SearchLogCommand : AbstractCommand
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public SearchLogCommand(IrcConnection conn) : base(conn)
        {
            // …
        }

        public override string Name {
            get {
                return "Search Log Command";
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

                Regex r = new Regex(@"^!searchlog ?");
                line.Args = r.Replace(line.Args, "").Trim();
                if (line.Args.Length <= 0) { // Whaaaat??
                    conn.SendPrivmsg(target, String.Format("Usage(1): !searchlog <search> - search examples: id:1 nick:{0} $a:{1}, #lumbricus, \"Magnus Magnusson\"", nick.DisplayName, nick.Account.Name));
                } else {
                    string[] argBits;
                    string search = line.Args;
                    string searchType = "string";
                    long searchId = 0;

                    if (line.Args.StartsWith("id:")) {
                        if (line.Args.Contains(" ")) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: ID searches cannot contain spaces.", nick.DisplayName));
                            return;
                        }
                        argBits = line.Args.Split(':');
                        search = argBits[1];
                        Int64.TryParse(search, out searchId);
                        if (searchId < 1) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: ID must be > 0.", nick.DisplayName));
                            return;
                        }
                        searchType = "id";
                    }

                    if (line.Args.StartsWith("nick:")) {
                        if (line.Args.Contains(" ")) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: Nick searches cannot contain spaces.", nick.DisplayName));
                            return;
                        }
                        argBits = line.Args.Split(':');
                        search = argBits[1];
                        searchType = "nick";
                    }

                    if (line.Args.StartsWith("$a:")) {
                        if (line.Args.Contains(" ")) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: Account searches cannot contain spaces.", nick.DisplayName));
                            return;
                        }
                        argBits = line.Args.Split(':');
                        search = argBits[1];
                        searchType = "account";
                    }

                    if (line.Args.StartsWith("#")) {
                        if (line.Args.Contains(" ")) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: Channel searches cannot contain spaces.", nick.DisplayName));
                            return;
                        }
                        search = line.Args;
                        searchType = "channel";
                    }

                    if (searchType == "string") {
                        if (!line.Args.StartsWith("\"") || !line.Args.EndsWith("\"") || line.Args.Count(f => f == '\"') != 2) {
                            conn.SendPrivmsg(target, String.Format("Usage(2): !searchlog <search> - search examples: id:1 nick:{0} $a:{1}, #lumbricus, \"Magnus Magnusson\"", nick.DisplayName, nick.Account.Name));
                            return;
                        }

                        if (line.Args.Trim().Replace("%", "").Length < 7) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: Text searches must be at least 5 characters long, not including wildcards.", nick.DisplayName));
                            return;
                        }

                        search = line.Args.Substring(1, (line.Args.Length - 2));
                    }

                    Log ignoreLogLine = Log.Fetch(nick);

                    long totalLines = 0;
                    Log logLine = null;
                    Account searchAccount = null;
                    Nick searchNick = null;
                    Channel searchChannel = null;
                    switch (searchType) {
                        case "id":
                            logLine = Log.Fetch(searchId);
                            totalLines = (logLine == null ? 0 : 1);
                            if (logLine != null) {
                                if (logLine.Channel != null) {
                                    searchChannel = Channel.Fetch(logLine.Channel.Id);
                                }
                            } else {
                                conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: Log entry id `{1}` does not exist.", nick.DisplayName, searchId));
                                return;
                            }
                            break;

                        case "string":
                            if (search.Contains("\"")) search = search.Replace("\"", "");
                            totalLines = Log.FetchTotal(search);
                            logLine = Log.Fetch(search, ignoreLogLine, channel);
                            if (logLine != null) {
                                if (logLine.Channel != null) {
                                    searchChannel = Channel.Fetch(logLine.Channel.Id);
                                }
                            } else {
                                conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: No results for search `{1}`.", nick.DisplayName, search));
                                return;
                            }
                            break;

                        case "account":
                            searchAccount = Account.Fetch(search.ToLower(), conn.Server);
                            if (searchAccount == null) {
                                conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: There is no account `{1}` in the database", nick.DisplayName, search));
                                return;
                            }
                            if (searchAccount.MostRecentNick != null) {
                                searchNick = searchAccount.MostRecentNick;
                                totalLines = Log.FetchTotal(searchNick);
                                logLine = Log.Fetch(searchNick, ignoreLogLine, channel);
                            } else {
                                totalLines = Log.FetchTotal(searchAccount);
                                logLine = Log.Fetch(searchAccount, ignoreLogLine, channel);
                            }
                            if (logLine != null) {
                                if (logLine.Channel != null) {
                                    searchChannel = Channel.Fetch(logLine.Channel.Id);
                                }
                            } else {
                                conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: $a:{1} is not in the log (?!)", nick.DisplayName, searchAccount.Name));
                                return;
                            }
                            break;

                        case "nick":
                            searchNick = Nick.Fetch(search.ToLower(), conn.Server);
                            if (searchNick == null) {
                                conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: There is no nick `{1}` in the database", nick.DisplayName, search));
                                return;
                            }
                            if (searchNick.Account != null) {
                                searchAccount = searchNick.Account;
                            }
                            totalLines = Log.FetchTotal(searchNick);
                            logLine = Log.Fetch(searchNick, ignoreLogLine, channel);
                            if (logLine != null) {
                                if (logLine.Channel != null) {
                                    searchChannel = Channel.Fetch(logLine.Channel.Id);
                                }
                            } else {
                                conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: nick:{1} is not in the log (?!)", nick.DisplayName, searchNick.DisplayName));
                                return;
                            }
                            break;

                        case "channel":
                            searchChannel = Channel.Fetch(search.ToLower(), conn.Server);
                            if (searchChannel == null) {
                                conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: There is no channel `{1}` in the database", nick.DisplayName, search));
                                return;
                            }
                            totalLines = Log.FetchTotal(searchChannel);
                            logLine = Log.Fetch(searchChannel, ignoreLogLine);
                            if (logLine == null) {
                                conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: {1} is not in the log (?!)", nick.DisplayName, searchChannel.Name));
                                return;
                            }
                            break;
                    }

                    if (logLine == null) {
                        conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: This message should never appear, what the HELL did you DO to me?! D:", nick.DisplayName));
                        return;
                    }

                    string response = totalLines + " found, latest: " + (searchChannel != null ? searchChannel.Name : conn.Server.BotNick) + " [" + logLine.LoggedAt.ToString("u") + "] " + logLine.Line;
                    conn.SendPrivmsg(target, response);
                }
            } catch (Exception e) {
                logger.Error(e);
                conn.SendPrivmsg(nick.Name, "Oof… My spleen…! I can't do that right now. :(");
            }
        }

    }

}
