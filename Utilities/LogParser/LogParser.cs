using NLog;
using System;
using TwoWholeWorms.Lumbricus.Shared;
using System.IO;
using System.Text.RegularExpressions;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Plugins.IrcLogPlugin.Model;

namespace TwoWholeWorms.Lumbricus.Utilities
{
    
    class LogParser
    {

        enum LogType {
            znc,
            irssi,
        };

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        static LumbricusConfiguration config;
        static LogType type;
        static DateTime? date;
        static Channel channel;
        static Server server;

        public static void Main(string[] args)
        {
            if (args.Length < 3 || args.Length > 4) DoUsage();

            try {
                type = args[0].ToEnum<LogType>();
            } catch (Exception e) {
                logger.Error(e);
                DoUsage(string.Format("Invalid log type `{0}`", args[0]));
            }
            switch (type) {
                case LogType.znc:
                    if (args.Length < 4) {
                        DoUsage("Date in YYYY-MM-DD format is required for ZNC logs.");
                    }
                    break;

                case LogType.irssi:
                    break;

                default:
                    DoUsage(string.Format("Invalid type `{0}`", type));
                    break;
            }

            long channelId = Int64.Parse(args[1]);
            if (channelId < 1) {
                DoUsage(string.Format("Invalid channel id `{0}`; must be > 0", args[1]));
            }

            config = LumbricusConfiguration.GetConfig();
            LumbricusContext.Initialise(config);

            channel = Channel.Fetch(channelId);
            if (channel == null) {
                DoUsage(string.Format("Channel id `{0}` does not exist.", channelId));
            }
            server = channel.Server;
            if (server == null) {
                throw new Exception(string.Format("Channel `{0}` is linked to a server id which does not exist or is deleted.", channelId));
            }

            string filename = args[2];
            if (string.IsNullOrWhiteSpace(filename)) {
                DoUsage(string.Format("Invalid filename `{0}`", args[2]));
            } else if (!File.Exists(filename)) {
                DoUsage(string.Format("File `{0}` does not exist", filename));
            }

            date = null;
            if (args.Length == 4 && !string.IsNullOrWhiteSpace(args[3])) {
                try {
                    date = DateTime.Parse(args[3]);
                } catch (Exception e) {
                    logger.Error(e);
                    DoUsage(string.Format("Invalid date `{0}`", args[3]));
                }
            }

            int i = 0;
            foreach (string line in File.ReadLines(filename)) {
                ProcessLine(line);

                if (i % 100 == 0) Console.Write(".");
                i++;
            }

            logger.Info("Done.");
        }

        static void DoUsage(string message = null)
        {
            if (!string.IsNullOrWhiteSpace(message)) logger.Info(message);
            logger.Error("Usage: {0} <znc|irssi> <channel> <file> [YYYY-MM-DD] — date is required for ZNC logs.", AppDomain.CurrentDomain.FriendlyName);
            Environment.Exit(0);
        }

        static void ProcessLine(string line)
        {
            string logNick = null;
            string logTime = null;
            string logDate = null;
            string message = null;
            IrcCommand ircCommand = IrcCommand.UNKNOWN;

            Regex r;
            Match m;
            switch (type) {
                case LogType.znc:
                    logDate = (date.HasValue ? date.Value.ToString("yyyy-MM-dd") : null);
                    r = new Regex(@"^\[(?<time>\d\d:\d\d:\d\d)\] <(?<nick>[^>]+)> (?<message>.*)$", RegexOptions.ExplicitCapture);
                    m = r.Match(line);
                    if (m.Success) {
                        logTime = m.Groups["time"].Value;
                        logNick = m.Groups["nick"].Value;
                        ircCommand = IrcCommand.PRIVMSG;
                        message = m.Groups["message"].Value;
                    }

                    r = new Regex(@"^\[(?<time>\d\d:\d\d:\d\d)\] (?<message>\* (?<nick>[^ ]+) .*)$", RegexOptions.ExplicitCapture);
                    m = r.Match(line);
                    if (m.Success) {
                        logTime = m.Groups["time"].Value;
                        logNick = m.Groups["nick"].Value;
                        ircCommand = IrcCommand.PRIVMSG;
                        message = m.Groups["message"].Value;
                    }

                    r = new Regex(@"^\[(?<time>\d\d:\d\d:\d\d)\] \*\*\* Joins: (?<nick>[^ ]+) ", RegexOptions.ExplicitCapture);
                    m = r.Match(line);
                    if (m.Success) {
                        logTime = m.Groups["time"].Value;
                        logNick = m.Groups["nick"].Value;
                        ircCommand = IrcCommand.JOIN;
                        message = null;
                    }

                    r = new Regex(@"^\[(?<time>\d\d:\d\d:\d\d)\] \*\*\* Quits: (?<nick>[^ ]+) \(.*\) \(Quit: (?<message>.*)\)$", RegexOptions.ExplicitCapture);
                    m = r.Match(line);
                    if (m.Success) {
                        logTime = m.Groups["time"].Value;
                        logNick = m.Groups["nick"].Value;
                        ircCommand = IrcCommand.QUIT;
                        message = m.Groups["message"].Value;
                    }

                    r = new Regex("^\\[(?<time>\\d\\d:\\d\\d:\\d\\d)\\] \\*\\*\\* Parts: (?<nick>[^ ]+) \\(.*\\) \\(\"?(?<message>.*)\"?\\)$", RegexOptions.ExplicitCapture);
                    m = r.Match(line);
                    if (m.Success) {
                        logTime = m.Groups["time"].Value;
                        logNick = m.Groups["nick"].Value;
                        ircCommand = IrcCommand.PART;
                        message = m.Groups["message"].Value;
                    }

                    if (logNick == null || logTime == null) {
                        // Fuck-all to do here!
                        return;
                    }
                    break;

                case LogType.irssi:
                    // Irssi uses a spastic date format:
                    // --- Log opened Wed Jun 20 16:11:34 2012
                    r = new Regex(@"^--- Log opened ([^ ]+) (?<month>[^ ]+) (?<day>[^ ]+) (?<time>[^ ]+) (?<year>.*)$", RegexOptions.ExplicitCapture);
                    m = r.Match(line);
                    if (m.Success) {
                        string lineDate = string.Format("{0} {1} {2} {3}", m.Groups["day"].Value, m.Groups["month"].Value, m.Groups["year"].Value, m.Groups["time"].Value);
                        date = DateTime.Parse(lineDate);
                        return;
                    }

                    r = new Regex(@"^--- Day changed ([^ ]+) (?<month>[^ ]+) (?<day>[^ ]+) (?<time>[^ ]+) (?<year>.*)$", RegexOptions.ExplicitCapture);
                    m = r.Match(line);
                    if (m.Success) {
                        string lineDate = string.Format("{0} {1} {2} {3}", m.Groups["day"].Value, m.Groups["month"].Value, m.Groups["year"].Value, m.Groups["time"].Value);
                        date = DateTime.Parse(lineDate);
                        return;
                    }

                    if (date != null) {
                        logDate = date.Value.ToString("yyyy-MM-dd");
                    }

                    if (string.IsNullOrWhiteSpace(logDate)) {
                        throw new Exception("Date not yet set, unable to continue processing log file.");
                    }

                    r = new Regex(@"^(?<time>\d\d:\d\d) <.(?<nick>[^>]+)> (?<message>.*)$", RegexOptions.ExplicitCapture);
                    m = r.Match(line);
                    if (m.Success) {
                        logTime = m.Groups["time"].Value + ":00";
                        logNick = m.Groups["nick"].Value;
                        ircCommand = IrcCommand.PRIVMSG;
                        message = m.Groups["message"].Value;
                    }

                    r = new Regex(@"^(?<time>\d\d:\d\d) (?<message>\* (?<nick>[^ ]+)> .*)$", RegexOptions.ExplicitCapture);
                    m = r.Match(line);
                    if (m.Success) {
                        logTime = m.Groups["time"].Value + ":00";
                        logNick = m.Groups["nick"].Value;
                        ircCommand = IrcCommand.PRIVMSG;
                        message = m.Groups["message"].Value;
                    }

                    r = new Regex(@"^(?<time>\d\d:\d\d) -!- (?<nick>[^ ]+) .* has joined #", RegexOptions.ExplicitCapture);
                    m = r.Match(line);
                    if (m.Success) {
                        logTime = m.Groups["time"].Value + ":00";
                        logNick = m.Groups["nick"].Value;
                        ircCommand = IrcCommand.JOIN;
                        message = null;
                    }

                    r = new Regex(@"^(?<time>\d\d:\d\d) -!- (?<nick>[^ ]+) .* has quit \[Quit: (?<message>.*)\]", RegexOptions.ExplicitCapture);
                    m = r.Match(line);
                    if (m.Success) {
                        logTime = m.Groups["time"].Value + ":00";
                        logNick = m.Groups["nick"].Value;
                        ircCommand = IrcCommand.QUIT;
                        message = m.Groups["message"].Value;
                    }

                    r = new Regex("^(?<time>\\d\\d:\\d\\d) -!- (?<nick>[^ ]+) .* has left #([^ ]+) \\[\"?(?<message>.*)\"?\\]", RegexOptions.ExplicitCapture);
                    m = r.Match(line);
                    if (m.Success) {
                        logTime = m.Groups["time"].Value + ":00";
                        logNick = m.Groups["nick"].Value;
                        ircCommand = IrcCommand.PART;
                        message = m.Groups["message"].Value;
                    }

                    if (logNick == null || logTime == null) {
                        // Fuck-all to do here!
                        return;
                    }
                    break;

                 default:
                    throw new Exception(string.Format("Unknown log type `{0}`", type));
            }

            Nick nick = Nick.FetchOrCreate(logNick, server);
            Account account = nick.Account;
            Seen seen = Seen.Fetch(nick) ?? new Seen();

            DateTime logDateTime = DateTime.Parse(logDate + " " + logTime);
            if (seen.Nick == null) seen.Nick = nick;
            if (seen.Account == null) seen.Account = account;
            if (seen.Channel == null) seen.Channel = channel;
            if (seen.Server == null) seen.Server = server;
            if (seen.FirstSeenAt == DateTime.MinValue || logDateTime < seen.FirstSeenAt) seen.FirstSeenAt = logDateTime;
            if (seen.LastSeenAt == DateTime.MaxValue || logDateTime > seen.LastSeenAt) seen.LastSeenAt = logDateTime;

            seen.Save();

            Log log = Log.Create(server);
            log.Nick = nick;
            log.Account = account;
            log.Channel = channel;
            log.IrcCommand = ircCommand;
            log.LoggedAt = logDateTime;
            log.Trail = message;
            log.Line = line;
            log.Save();
        }

    }

}
