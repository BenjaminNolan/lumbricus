using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using TwoWholeWorms.Lumbricus.Shared;

namespace TwoWholeWorms.Lumbricus.Module
{
    
    public static class Command
    {

        private static bool isOp(IrcConnection conn, IrcNick nick)
        {
            IrcChannel opsChannel = conn.server.channels.FirstOrDefault(x => x.name == "#gaygeeks-ops");
            if (opsChannel == null) {
                throw new Exception("Unable to fetch ops channel from conn.server.channels");
            }

            return opsChannel.nicks.Contains(nick);
        }
        
        public static void HandleSetMugshot(IrcNick nick, string args, IrcConnection conn)
        {
            try {
                IrcSeen seen = IrcSeen.Fetch(nick);
                DateTime checkTime = DateTime.Now;
                checkTime.AddDays(-7);
                if (seen.firstSeenAt == DateTime.MinValue || seen.firstSeenAt > checkTime) {
                    conn.SendPrivmsg(nick.nick, String.Format("Sorry, {0}, but you aren't allowed to use the mugshots functions yet. :(", nick.displayNick));
                    return;
                }

                Regex r = new Regex(@"^!setmugshot ?");
                string imageUri = r.Replace(args, "").Trim();
                if (imageUri.Length <= 0) { // Whaaaat??
                    conn.SendPrivmsg(nick.nick, "Usage: !setmugshot <image_url_here>");
                    return;
                }

                r = new Regex(@"^https?://.*\.(png|gif|jpe?g)");
                Match m = r.Match(imageUri);
                if (!m.Success) {
                    conn.SendPrivmsg(nick.nick, "Usage: !setmugshot <image_url_here> - the image must be a PNG, GIF, or JPEG file.");
                    return;
                }

                r = new Regex(@"^https?://(www.)?dropbox.com/.*\?dl=0");
                m = r.Match(imageUri);
                if (m.Success) {
                    imageUri = imageUri.Replace("?dl=0", "?dl=1");
                }

                Image image = GetImageFromUrl(imageUri);
                if (image == null) {
                    throw new Exception(String.Format("Unable to get image from URI `{0}`", imageUri));
                }

                r = new Regex(@"\.([a-z0-9]{3})$");
                string newFileName = r.Replace(Path.GetRandomFileName(), ".png");
                Image thumb = image.GetThumbnailFixedSize(170, 170, true);

                ImageCodecInfo info = GetEncoderInfo("image/png");
                Encoder encoder = Encoder.Quality;
                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(encoder, 100);
                thumb.Save(Lumbricus.config.mugshots.thumbsPath + Path.DirectorySeparatorChar + newFileName, info, encoderParams);
                image.Save(Lumbricus.config.mugshots.largeImagePath + Path.DirectorySeparatorChar + newFileName, info, encoderParams);

                using (MySqlCommand cmd = DataConnection.CreateDbCommand()) {
                    cmd.CommandText = "SELECT * FROM `mugshots` WHERE (`account_id` = @accountId)";
                    cmd.Parameters.AddWithValue("@accountId", nick.account.id);
                    using (IDataReader reader = cmd.ExecuteReader()) {
                        if (!reader.Read()) {
                            reader.Dispose();
                            cmd.Dispose();

                            using (MySqlCommand cmd2 = DataConnection.CreateDbCommand()) {
                                cmd2.CommandText = "INSERT INTO `mugshots` (`original_uri`, `uri`, `large_uri`, `account_id`) VALUES (@originalUri, @uri, @largeUri, @accountId)";
                                cmd2.Parameters.AddWithValue("@originalUri", imageUri);
                                cmd2.Parameters.AddWithValue("@uri", Lumbricus.config.mugshots.thumbsUri + "/" + newFileName);
                                cmd2.Parameters.AddWithValue("@largeUri", Lumbricus.config.mugshots.largeImageUri + "/" + newFileName);
                                cmd2.Parameters.AddWithValue("@accountId", nick.account.id);
                                cmd2.ExecuteNonQuery();
                            }
                        } else {
                            reader.Dispose();
                            cmd.Dispose();

                            using (MySqlCommand cmd3 = DataConnection.CreateDbCommand()) {
                                cmd3.CommandText = "UPDATE `mugshots` SET `original_uri` = @originalUri, `uri` = @uri, `large_uri` = @largeUri, `is_deleted` = 0 WHERE `account_id` = @accountId";
                                cmd3.Parameters.AddWithValue("@originalUri", imageUri);
                                cmd3.Parameters.AddWithValue("@uri", Lumbricus.config.mugshots.thumbsUri + "/" + newFileName);
                                cmd3.Parameters.AddWithValue("@largeUri", Lumbricus.config.mugshots.largeImageUri + "/" + newFileName);
                                cmd3.Parameters.AddWithValue("@accountId", nick.account.id);
                                cmd3.ExecuteNonQuery();
                            }
                        }
                    }
                    conn.SendPrivmsg(nick.nick, "Woo! Your mugshot has been saved! :D");
                }

                using (MySqlCommand cmd = DataConnection.CreateDbCommand()) {
                    cmd.CommandText = "UPDATE `accounts` SET `nick_id` = @nickId WHERE `id` = @accountId";
                    cmd.Parameters.AddWithValue("@nickId", nick.id);
                    cmd.Parameters.AddWithValue("@accountId", nick.account.id);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            } catch (Exception e) {
                Logger.Log(e);
                conn.SendPrivmsg(nick.nick, "Oof… I shouldn't have eaten that pie, I can't do that right now. :(");
            }
        }

        public static void HandleClearMugshot(IrcNick nick, string args, IrcConnection conn)
        {
            try {
                IrcSeen seen = IrcSeen.Fetch(nick);
                DateTime checkTime = DateTime.Now;
                checkTime.AddDays(-7);
                if (seen.firstSeenAt == DateTime.MinValue || seen.firstSeenAt > checkTime) {
                    conn.SendPrivmsg(nick.nick, String.Format("Sorry, {0}, but you aren't allowed to use the mugshots functions yet. :(", nick.displayNick));
                    return;
                }

                using (MySqlCommand cmd = DataConnection.CreateDbCommand()) {
                    cmd.CommandText = "SELECT * FROM `mugshots` WHERE (`account_id` = @accountId AND `is_deleted` = 0)";
                    cmd.Parameters.AddWithValue("@accountId", nick.account.id);

                    using (IDataReader reader = cmd.ExecuteReader()) {
                        if (!reader.Read()) {
                            reader.Dispose();
                            cmd.Dispose();

                            conn.SendPrivmsg(nick.nick, "You don't have a mugshot in the database to clear! :o");
                        } else {
                            reader.Dispose();
                            cmd.Dispose();

                            using (MySqlCommand cmd2 = DataConnection.CreateDbCommand()) {
                                cmd2.CommandText = "UPDATE `mugshots` SET `is_deleted` = 1 WHERE `account_id` = @accountId";
                                cmd2.Parameters.AddWithValue("@accountId", nick.account.id);
                                cmd2.ExecuteNonQuery();
                            }

                            conn.SendPrivmsg(nick.nick, "Your mugshot has been cleared. :(");

                            using (MySqlCommand cmd3 = DataConnection.CreateDbCommand()) {
                                cmd3.CommandText = "UPDATE `accounts` SET `nick_id` = @nickId WHERE `id` = @accountId";
                                cmd3.Parameters.AddWithValue("@nickId", nick.id);
                                cmd3.Parameters.AddWithValue("@accountId", nick.account.id);
                                cmd3.ExecuteNonQuery();
                            }
                        }
                    }
                }
            } catch (Exception e) {
                Logger.Log(e);
                conn.SendPrivmsg(nick.nick, "Oof… I've got indigestion or something, I can't do that right now. :(");
            }
        }

        public static void HandleSetInfo(IrcNick nick, string args, IrcConnection conn)
        {
            try {
                IrcSeen seen = IrcSeen.Fetch(nick);
                DateTime checkTime = DateTime.Now;
                checkTime.AddDays(-7);
                if (seen.firstSeenAt == DateTime.MinValue || seen.firstSeenAt > checkTime) {
                    conn.SendPrivmsg(nick.nick, String.Format("Sorry, {0}, but you aren't allowed to use the mugshots functions yet. :(", nick.displayNick));
                    return;
                }

                Regex r = new Regex(@"^!setinfo ?");
                args = r.Replace(args, "").Trim();
                if (args.Length <= 0) { // Whaaaat??
                    conn.SendPrivmsg(nick.nick, "Usage: !setinfo <your message here>");
                } else {
                    using (MySqlCommand cmd = DataConnection.CreateDbCommand()) {
                        cmd.CommandText = "SELECT * FROM `info` WHERE (`account_id` = @accountId)";
                        cmd.Parameters.AddWithValue("@accountId", nick.account.id);
                        using (IDataReader reader = cmd.ExecuteReader()) {
                            if (!reader.Read()) {
                                reader.Dispose();
                                cmd.Dispose();

                                using (MySqlCommand cmd2 = DataConnection.CreateDbCommand()) {
                                    cmd2.CommandText = "INSERT INTO `info` (`account_id`, `info_txt`) VALUES (@accountId, @infoTxt)";
                                    cmd2.Parameters.AddWithValue("@accountId", nick.account.id);
                                    cmd2.Parameters.AddWithValue("@infoTxt", args);
                                    cmd2.ExecuteNonQuery();
                                }
                            } else {
                                reader.Dispose();
                                cmd.Dispose();

                                using (MySqlCommand cmd3 = DataConnection.CreateDbCommand()) {
                                    cmd3.CommandText = "UPDATE `info` SET `info_txt` = @infoTxt, `is_deleted` = 0 WHERE `account_id` = @accountId";
                                    cmd3.Parameters.AddWithValue("@accountId", nick.account.id);
                                    cmd3.Parameters.AddWithValue("@infoTxt", args);
                                    cmd3.ExecuteNonQuery();
                                }
                            }
                            conn.SendPrivmsg(nick.nick, "Woo! Your info has been saved! :)");
                        }
                    }

                    using (MySqlCommand cmd = DataConnection.CreateDbCommand()) {
                        cmd.CommandText = "UPDATE `accounts` SET `nick_id` = @nickId WHERE `id` = @accountId";
                        cmd.Parameters.AddWithValue("@nickId", nick.id);
                        cmd.Parameters.AddWithValue("@accountId", nick.account.id);
                        cmd.ExecuteNonQuery();
                    }
                }
            } catch (Exception e) {
                Logger.Log(e);
                conn.SendPrivmsg(nick.nick, "Oof… My neck is killing me, I can't do that right now. :(");
            }
        }

        public static void HandleClearInfo(IrcNick nick, string args, IrcConnection conn)
        {
            try {
                IrcSeen seen = IrcSeen.Fetch(nick);
                DateTime checkTime = DateTime.Now;
                checkTime.AddDays(-7);
                if (seen.firstSeenAt == DateTime.MinValue || seen.firstSeenAt > checkTime) {
                    conn.SendPrivmsg(nick.nick, String.Format("Sorry, {0}, but you aren't allowed to use the mugshots functions yet. :(", nick.displayNick));
                    return;
                }

                using (MySqlCommand cmd = DataConnection.CreateDbCommand()) {
                    cmd.CommandText = "SELECT * FROM `info` WHERE (`account_id` = @accountId AND `is_deleted` = 0)";
                    cmd.Parameters.AddWithValue("@accountId", nick.account.id);

                    using (IDataReader reader = cmd.ExecuteReader()) {
                        if (!reader.Read()) {
                            reader.Dispose();
                            cmd.Dispose();

                            conn.SendPrivmsg(nick.nick, "You don't have any info in the database to clear! :o");
                        } else {
                            reader.Dispose();
                            cmd.Dispose();

                            using (MySqlCommand cmd2 = DataConnection.CreateDbCommand()) {
                                cmd2.CommandText = "UPDATE `info` SET `is_deleted` = 1 WHERE `account_id` = @accountId";
                                cmd2.Parameters.AddWithValue("@accountId", nick.account.id);
                                cmd2.ExecuteNonQuery();

                                conn.SendPrivmsg(nick.nick, "Your info has been cleared. :(");
                            }

                            using (MySqlCommand cmd3 = DataConnection.CreateDbCommand()) {
                                cmd3.CommandText = "UPDATE `accounts` SET `nick_id` = @nickId WHERE `id` = @accountId";
                                cmd3.Parameters.AddWithValue("@nickId", nick.id);
                                cmd3.Parameters.AddWithValue("@accountId", nick.account.id);
                                cmd3.ExecuteNonQuery();
                            }
                        }
                    }
                }
            } catch (Exception e) {
                Logger.Log(e);
                conn.SendPrivmsg(nick.nick, "Oof… I banged my knee and it don't half hurt, I can't do that right now. :(");
            }
        }

        public static void HandleHelp(IrcNick nick, string args, IrcConnection conn, IrcChannel channel = null)
        {
            try {
                if (nick.account != null && nick.account.isOp) {
                    string target = nick.nick;
                    if (channel != null && channel.allowCommandsInChannel) {
                        target = channel.name;
                    }
                    conn.SendPrivmsg(target, String.Format("Hi, @{0}. Main help is at {1}. You also have access to these op-only commands: !seen, !baninfo, !botban, !restart", nick.displayNick, Lumbricus.config.helpUri));
                } else {
                    conn.SendPrivmsg(nick.nick, String.Format("Hi, {0}. Help is at {1}.", nick.displayNick, Lumbricus.config.helpUri));
                    if (channel != null && !channel.allowCommandsInChannel) {
                        conn.SendPrivmsg(nick.nick, "Also, please try to only interact with me directly through this window. Bot commands in the main channel are against channel policy, and some people get really annoyed about it. :(");
                    }
                }
            } catch (Exception e) {
                Logger.Log(e);
                conn.SendPrivmsg(nick.nick, "Oof… I really shouldn't have had that second slice of cake, I can't do that right now. :(");
            }
        }

        public static void HandleSeen(IrcNick nick, string args, IrcConnection conn, IrcChannel channel = null)
        {
            try {
                if (nick.account == null || (!isOp(conn, nick) && !nick.account.isOp)) {
                    conn.SendPrivmsg(nick.nick, String.Format("Sorry, {0}, but the !seen command doesn't exist. Try !help.", nick.displayNick));
                    return;
                }

                string target = nick.nick;
                if (channel != null && channel.allowCommandsInChannel) {
                    target = channel.name;
                }

                Regex r = new Regex(@"^!seen ?");
                args = r.Replace(args, "").Trim();
                if (args.Length <= 0) { // Whaaaat??
                    conn.SendPrivmsg(target, String.Format("Usage: !seen <search> - use $a:account to search for an account (eg, !seen $a:{0})", nick.account.account));
                } else {
                    string[] argBits = args.Split(' ');
                    if (argBits.Length > 1) {
                        conn.SendPrivmsg(target, String.Format("Usage: !seen <search> - use $a:account to search for an account (eg, !seen $a:{0})", nick.account.account));
                        return;
                    }

                    argBits = argBits[0].Split(':');
                    string search = argBits[0];
                    string searchType = "nick";
                    if (argBits.Length == 2) {
                        if (argBits[0] != "$a") {
                            conn.SendPrivmsg(target, String.Format("Usage: !seen <search> - use $a:account to search for an account (eg, !seen $a:{0})", nick.account.account));
                            return;
                        }
                        search = argBits[1];
                        searchType = "account";
                    }

                    IrcNick ircNick = null;
                    IrcAccount ircAccount = null;
                    IrcChannel ircChannel = null;
                    IrcSeen ircSeen = null;
                    if (searchType == "account") {
                        ircAccount = IrcAccount.Fetch(search.ToLower(), conn.server);
                        if (ircAccount != null) {
                            ircSeen = IrcSeen.FetchByAccountId(ircAccount.id);
                            if (ircSeen != null) {
                                if (ircSeen.nickId > 0) {
                                    ircNick = IrcNick.Fetch(ircSeen.nickId, conn.server);
                                }
                                if (ircSeen.channelId > 0) {
                                    ircChannel = IrcChannel.Fetch(ircSeen.channelId, conn.server);
                                }
                            }
                        }
                    } else {
                        ircNick = IrcNick.Fetch(search.ToLower(), conn.server);
                        if (ircNick != null) {
                            ircSeen = IrcSeen.FetchByNickId(ircNick.id);
                            if (ircSeen != null) {
                                if (ircSeen.accountId > 0) {
                                    ircAccount = IrcAccount.Fetch(ircSeen.accountId, conn.server);
                                }
                                if (ircSeen.channelId > 0) {
                                    ircChannel = IrcChannel.Fetch(ircSeen.channelId, conn.server);
                                }
                            }
                        }
                    }

                    if (ircSeen == null) {
                        conn.SendPrivmsg(target, String.Format("There is no seen data in the database about {0} `{1}`.", searchType, search));
                    } else {
                        string seenNick = (ircNick != null ? ircNick.displayNick : "Unknown Nick");
                        string seenAccount = (ircAccount != null ? ircAccount.account : "Unknown Account");
                        string seenChannel = (ircChannel != null ? ircChannel.name : "a private query window");

                        conn.SendPrivmsg(target, String.Format("{1}{0}{2}{1} ($a:{0}{3}{1}): First seen {0}{4}{1}, last seen {0}{5}{1} in {1}{0}{6}{1}.", "\x02", "\x0f", seenNick, seenAccount, ircSeen.firstSeenAt.ToString("u"), ircSeen.lastSeenAt.ToString("u"), seenChannel));
                    }
                }
            } catch (Exception e) {
                Logger.Log(e);
                conn.SendPrivmsg(nick.nick, "Oof… My spleen…! I can't do that right now. :(");
            }
        }

        public static void HandleBotBan(IrcNick nick, string args, IrcConnection conn, IrcChannel channel = null)
        {
            try {
                if (nick.account == null || (!isOp(conn, nick) && !nick.account.isOp)) {
                    conn.SendPrivmsg(nick.nick, String.Format("Sorry, {0}, but that command doesn't exist. Try !help.", nick.displayNick));
                    return;
                }

                string target = nick.nick;
                if (channel != null && channel.allowCommandsInChannel) {
                    target = channel.name;
                }

                Regex r = new Regex(@"^!botban ?");
                args = r.Replace(args, "").Trim();
                if (args.Length <= 0) { // Whaaaat??
                    conn.SendPrivmsg(target, "Usage(1): !botban <add|remove> <nick|$a:account|mask:*!*@*.*> [ban message]");
                } else {
                    string[] argBits = args.Split(" ".ToCharArray(), 3);
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

                    IrcBan ircBan = null;
                    IrcNick ircNick = null;
                    IrcAccount ircAccount = null;
                    switch (searchType) {
                        case "nick":
                            ircNick = IrcNick.Fetch(search, conn.server);
                            if (ircNick == null) {
                                conn.SendPrivmsg(target, String.Format("{0}{2}{1}: Nick `{3}` does not exist in the database.", "\x02", "\x0f", nick.displayNick, search));
                                return;
                            }
                            ircAccount = IrcAccount.Fetch(ircNick.accountId, conn.server);
                            ircBan = IrcBan.Fetch(ircNick, ircAccount);
                            break;
                        
                        case "account":
                            ircAccount = IrcAccount.Fetch(search, conn.server);
                            if (ircAccount == null) {
                                conn.SendPrivmsg(target, String.Format("{0}{2}{1}: Account `{3}` does not exist in the database.", "\x02", "\x0f", nick.displayNick, search));
                                return;
                            }
                            ircNick = IrcNick.Fetch(ircAccount.nickId, conn.server);
                            ircBan = IrcBan.Fetch(ircNick, ircAccount);
                            break;
                            
                        case "mask":
                            ircBan = IrcBan.Fetch(conn.server, search);

                            if (ircBan != null) {
                                if (ircBan.nickId > 0) ircNick = IrcNick.Fetch(ircBan.nickId, conn.server);
                                if (ircBan.accountId > 0) ircAccount = IrcAccount.Fetch(ircBan.accountId, conn.server);
                            }

                            break;

                        default:
                            conn.SendPrivmsg(target, "This what you're seeing here is a big fat error happening. Poke TwoWholeWorms.");
                            break;
                    }

                    if (ircBan == null) {
                        if (mode == "remove") {
                            conn.SendPrivmsg(target, String.Format("{0}{2}{1}: {3} `{4}` is not currently banned.", "\x02", "\x0f", nick.displayNick, searchType.Capitalise(), search));
                            return;
                        }
                        ircBan = new IrcBan();
                    } else if (mode == "add" && ircBan.isMugshotBan) {
                        IrcAccount banner = IrcAccount.Fetch(ircBan.bannedByAccountId, conn.server);
                        if (banner != null) {
                            conn.SendPrivmsg(target, String.Format("{0}{2}{1}: {3} `{4}` was already banned from accessing {8} by $a:{5} on {6}: {7}", "\x02", "\x0f", nick.displayNick, searchType.Capitalise(), search, banner.account, ircBan.createdAt.ToString("u"), (ircBan.banMessage ?? "No message"), conn.server.nick));
                        } else {
                            conn.SendPrivmsg(target, String.Format("{0}{2}{1}: {3} `{4}` was already banned from accessing {7} on {5}: {6}", "\x02", "\x0f", nick.displayNick, searchType.Capitalise(), search, ircBan.createdAt.ToString("u"), (ircBan.banMessage ?? "No message"), conn.server.nick));
                        }
                        return;
                    }

                    if (ircNick == null && ircAccount != null && ircAccount.nickId > 0) {
                        ircNick = IrcNick.Fetch(ircAccount.nickId, conn.server);
                    }
                    if (ircAccount == null && ircNick != null && ircNick.accountId > 0) {
                        ircAccount = IrcAccount.Fetch(ircNick.accountId, conn.server);
                    }

                    ircBan.SetServer(conn.server);
                    ircBan.SetNick(ircNick);
                    ircBan.SetAccount(ircAccount);
                    ircBan.SetIsMugshotBan((mode == "add"));
                    if (ircBan.isActive && ircBan.channelId < 1) ircBan.SetIsActive(false);
                    if (mode == "add") {
                        ircBan.SetBanMessage(banMessage);
                        ircBan.SetBannedByAccount(nick.account);
                        ircBan.SetUnbanMessage(null);
                        ircBan.SetUnbanner(null);
                        ircBan.SetUnbannedAt(null);
                    } else if (mode == "remove") {
                        ircBan.SetUnbanMessage(banMessage);
                        ircBan.SetUnbanner(nick.account);
                        ircBan.SetUnbannedAt(DateTime.Now);
                    }
                    if (searchType == "mask") {
                        ircBan.SetMask(search);
                    }
                    ircBan.Save();

                    if (mode == "add") {
                        conn.SendPrivmsg(target, String.Format("{0}{2}{1}: {3} `{4}` has been banned from accessing {5} features, and their existing mugshot has been hidden from the site.", "\x02", "\x0f", nick.displayNick, searchType.Capitalise(), search, conn.server.nick));
                    } else {
                        conn.SendPrivmsg(target, String.Format("{0}{2}{1}: {3} `{4}` ban on accessing {5} has been deactivated.", "\x02", "\x0f", nick.displayNick, searchType.Capitalise(), search, conn.server.nick));
                    }
                    return;
                }
            } catch (Exception e) {
                Logger.Log(e);
                conn.SendPrivmsg(nick.nick, "Aieee! The bees! The BEEES! They're chasing me away!!! (Get TwoWholeWorms!)");
            }
        }

        public static void HandleBanInfo(IrcNick nick, string args, IrcConnection conn, IrcChannel channel = null)
        {
            try {
                if (nick.account == null || (!isOp(conn, nick) && (nick.account != null && !nick.account.isOp))) {
                    conn.SendPrivmsg(nick.nick, String.Format("Sorry, {0}, but that command doesn't exist. Try !help.", nick.displayNick));
                    return;
                }

                string target = nick.nick;
                if (channel != null && channel.allowCommandsInChannel) {
                    target = channel.name;
                }

                Regex r = new Regex(@"^!baninfo ?");
                args = r.Replace(args, "").Trim();
                if (args.Length <= 0) { // Whaaaat??
                    conn.SendPrivmsg(target, String.Format("Usage(1): !baninfo <search> - Prefix search with $a: to search for an account, or mask: to search for a ban string, eg !baninfo $a:{0} !baninfo mask:*!*@*.no", nick.account.account));
                } else {
                    string[] argBits = args.Split(' ');
                    if (argBits.Length > 1) {
                        conn.SendPrivmsg(target, String.Format("Usage(2): !baninfo <search> - Prefix search with $a: to search for an account, or mask: to search for a ban string, eg !baninfo $a:{0} !baninfo mask:*!*@*.no", nick.account.account));
                        return;
                    }

                    argBits = argBits[0].Split(':');
                    string search = argBits[0];
                    string searchType = "nick";
                    if (argBits.Length > 2 || (argBits.Length == 2 && argBits[0] != "$a" && argBits[0] != "mask")) {
                        conn.SendPrivmsg(target, String.Format("Usage(3): !baninfo <search> - Prefix search with $a: to search for an account, or mask: to search for a ban string, eg !baninfo $a:{0} !baninfo mask:*!*@*.no", nick.account.account));
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
                            conn.SendPrivmsg(target, String.Format("Usage(4): !baninfo <search> - Prefix search with $a: to search for an account, or mask: to search for a ban string, eg !baninfo $a:{0} !baninfo mask:*!*@*.no", nick.account.account));
                            return;
                        }
                    }

                    IrcAccount bannedAccount = null;
                    IrcNick bannedNick = null;
                    List<IrcBan> ircBans = new List<IrcBan>();
                    switch (searchType) {
                        case "account":
                            bannedAccount = IrcAccount.Fetch(search.ToLower(), conn.server);
                            if (bannedAccount == null) {
                                conn.SendPrivmsg(target, String.Format("There is no account `{0}` in the database", search));
                                return;
                            }
                            bannedNick = IrcNick.Fetch(bannedAccount.nickId, conn.server);
                            bannedNick.SetAccount(bannedAccount);
                            ircBans = IrcBan.Fetch(bannedNick);
                            break;

                        case "nick":
                            bannedNick = IrcNick.Fetch(search.ToLower(), conn.server);
                            if (bannedNick == null) {
                                conn.SendPrivmsg(target, String.Format("There is no nick `{0}` in the database", search));
                                return;
                            }
                            bannedAccount = IrcAccount.Fetch(bannedNick.accountId, conn.server);
                            bannedNick.SetAccount(bannedAccount);
                            ircBans = IrcBan.Fetch(bannedNick);
                            break;

                        case "mask":
                            IrcBan ircBan = IrcBan.Fetch(conn.server, search);
                            if (ircBan == null) {
                                conn.SendPrivmsg(target, String.Format("There is no ban on mask `{0}` in the database", search));
                                return;
                            }
                            if (ircBan.accountId > 0) bannedAccount = IrcAccount.Fetch(ircBan.accountId, conn.server);
                            if (ircBan.nickId > 0) bannedNick = IrcNick.Fetch(ircBan.nickId, conn.server);

                            if (bannedAccount != null && bannedNick == null && bannedAccount.nickId > 0) {
                                bannedNick = IrcNick.Fetch(bannedAccount.nickId, conn.server);
                            }
                            if (bannedNick != null && bannedAccount == null && bannedNick.accountId > 0) {
                                bannedAccount = IrcAccount.Fetch(bannedNick.accountId, conn.server);
                            }

                            if (ircBan != null) {
                                ircBans.Add(ircBan);
                            }
                            break;
                    }

                    if (ircBans.Count < 1) {
                        if (bannedNick != null && bannedAccount != null) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: {2} ($a:{1}) is not banned.", nick.displayNick, bannedAccount.account, bannedNick.displayNick));
                        } else if (bannedAccount != null) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: $a:{1} is not banned.", nick.displayNick, bannedAccount.account));
                        } else if (bannedNick != null) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: `{1}` is not banned.", nick.displayNick, bannedNick.displayNick));
                        } else {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: This message should never appear, what the HELL did you DO to me?! D:", nick.displayNick));
                        }
                        return;
                    }

                    int i = 0;
                    foreach (IrcBan ircBan in ircBans) {
                        string response = String.Format("\x02{0}\x0f: #{1}", nick.displayNick, ircBan.id);
                        if (searchType == "account") {
                            response += " $a:" + bannedAccount.account;
                            if (bannedNick != null) {
                                response += " (" + bannedNick.displayNick + ")";
                            }
                        } else {
                            response += " " + bannedNick.displayNick;
                            if (bannedAccount != null) {
                                response += " ($a:" + bannedAccount.account + ")";
                            }
                        }
                        
                        IrcChannel bannedChannel = IrcChannel.Fetch(ircBan.channelId, conn.server);
                        response += " " + conn.server.nick + ((ircBan.isMugshotBan || (bannedChannel != null && ircBan.isActive)) ? ":" : ":un") + "banned";
                        if (bannedChannel != null) {
                            response += " " + bannedChannel.name + (!ircBan.isActive ? ":un" : ":") + "banned";
                        }
                        response += " ban:[" + ircBan.createdAt.ToString("u");
                    
                        if (!string.IsNullOrEmpty(ircBan.mask)) {
                            response += " mask:" + ircBan.mask;
                        }

                        IrcAccount cantankerousOp = IrcAccount.Fetch(ircBan.bannedByAccountId, conn.server);
                        if (cantankerousOp != null && cantankerousOp.nickId > 0) {
                            cantankerousOp.SetNick(IrcNick.Fetch(cantankerousOp.nickId, conn.server));
                        }

                        if (cantankerousOp != null) {
                            if (cantankerousOp.currentNick != null) {
                                response += " op:" + cantankerousOp.currentNick.displayNick;
                            } else {
                                response += " op:$a:" + cantankerousOp.account;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(ircBan.banMessage)) {
                            response += " reason:" + ircBan.banMessage;
                        }
                        response += "]";

                        if (ircBan.unbannerId > 0) {
                            IrcAccount unbanner = IrcAccount.Fetch(ircBan.unbannerId, conn.server);
                            if (unbanner != null) {
                                unbanner.SetNick(IrcNick.Fetch(unbanner.nickId, conn.server));
                            }
                            
                            response += " unban:[" + ircBan.unbannedAt.Value.ToString("u");
                            if (unbanner != null) {
                                if (unbanner.currentNick != null) {
                                    response += " op:" + unbanner.currentNick.displayNick;
                                } else {
                                    response += " op:" + unbanner.currentNick.displayNick;
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(ircBan.unbanMessage)) {
                                response += " msg:" + ircBan.banMessage;
                            }
                            response += "]";
                        }
                        if (i > 2) Thread.Sleep(2000); // Wait 2 secs between sending commands after 3, in case there's loads of bans to send
                        conn.SendPrivmsg(target, response);
                        i++;
                    }
                }
            } catch (Exception e) {
                Logger.Log(e);
                conn.SendPrivmsg(nick.nick, "Oof… My spleen…! I can't do that right now. :(");
            }
        }

        public static void HandleSearchLog(IrcNick nick, string args, IrcConnection conn, IrcChannel channel = null)
        {
            try {
                if (nick.account == null || (!isOp(conn, nick) && (nick.account != null && !nick.account.isOp))) {
                    conn.SendPrivmsg(nick.nick, String.Format("Sorry, {0}, but that command doesn't exist. Try !help.", nick.displayNick));
                    return;
                }

                string target = nick.nick;
                if (channel != null && channel.allowCommandsInChannel) {
                    target = channel.name;
                }

                Regex r = new Regex(@"^!searchlog ?");
                args = r.Replace(args, "").Trim();
                if (args.Length <= 0) { // Whaaaat??
                    conn.SendPrivmsg(target, String.Format("Usage(1): !searchlog <search> - search examples: id:1 nick:{0} $a:{1}, #gaygeeks-ops, \"Piers Morgan\"", nick.displayNick, nick.account.account));
                } else {
                    string[] argBits;
                    string search = args;
                    string searchType = "string";
                    long searchId = 0;

                    if (args.StartsWith("id:")) {
                        if (args.Contains(" ")) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: ID searches cannot contain spaces.", nick.displayNick));
                            return;
                        }
                        argBits = args.Split(':');
                        search = argBits[1];
                        Int64.TryParse(search, out searchId);
                        if (searchId < 1) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: ID must be > 0.", nick.displayNick));
                            return;
                        }
                        searchType = "id";
                    }

                    if (args.StartsWith("nick:")) {
                        if (args.Contains(" ")) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: Nick searches cannot contain spaces.", nick.displayNick));
                            return;
                        }
                        argBits = args.Split(':');
                        search = argBits[1];
                        searchType = "nick";
                    }

                    if (args.StartsWith("$a:")) {
                        if (args.Contains(" ")) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: Account searches cannot contain spaces.", nick.displayNick));
                            return;
                        }
                        argBits = args.Split(':');
                        search = argBits[1];
                        searchType = "account";
                    }

                    if (args.StartsWith("#")) {
                        if (args.Contains(" ")) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: Channel searches cannot contain spaces.", nick.displayNick));
                            return;
                        }
                        search = args;
                        searchType = "channel";
                    }

                    if (searchType == "string") {
                        if (!args.StartsWith("\"") || !args.EndsWith("\"") || args.Count(f => f == '\"') != 2) {
                            conn.SendPrivmsg(target, String.Format("Usage(2): !searchlog <search> - search examples: id:1 nick:{0} $a:{1}, #gaygeeks-ops, \"Piers Morgan\"", nick.displayNick, nick.account.account));
                            return;
                        }

                        if (args.Trim().Replace("%", "").Length < 7) {
                            conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: Text searches must be at least 5 characters long, not including wildcards.", nick.displayNick));
                            return;
                        }

                        search = args.Substring(1, (args.Length - 2));
                    }

                    IrcLog ignoreLogLine = IrcLog.Fetch(nick);

                    long totalLines = 0;
                    IrcLog logLine = null;
                    IrcAccount searchAccount = null;
                    IrcNick searchNick = null;
                    IrcChannel searchChannel = null;
                    switch (searchType) {
                        case "id":
                            logLine = IrcLog.Fetch(searchId);
                            totalLines = (logLine == null ? 0 : 1);
                            if (logLine != null) {
                                searchChannel = IrcChannel.Fetch(logLine.channelId, conn.server);
                            } else {
                                conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: Log entry id `{0}` does not exist.", nick.displayNick, searchId));
                                return;
                            }
                            break;

                        case "string":
                            if (search.Contains("\"")) search = search.Replace("\"", "");
                            totalLines = IrcLog.FetchTotal(search);
                            logLine = IrcLog.Fetch(search, ignoreLogLine, channel);
                            if (logLine != null) {
                                searchChannel = IrcChannel.Fetch(logLine.channelId, conn.server);
                            } else {
                                conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: No results for search `{1}`.", nick.displayNick, search));
                                return;
                            }
                            break;

                        case "account":
                            searchAccount = IrcAccount.Fetch(search.ToLower(), conn.server);
                            if (searchAccount == null) {
                                conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: There is no account `{1}` in the database", nick.displayNick, search));
                                return;
                            }
                            if (searchAccount.nickId > 0) {
                                searchNick = IrcNick.Fetch(searchAccount.nickId, conn.server);
                                searchNick.SetAccount(searchAccount);
                                totalLines = IrcLog.FetchTotal(searchNick);
                                logLine = IrcLog.Fetch(searchNick, ignoreLogLine, channel);
                            } else {
                                totalLines = IrcLog.FetchTotal(searchAccount);
                                logLine = IrcLog.Fetch(searchAccount, ignoreLogLine, channel);
                            }
                            if (logLine != null) {
                                searchChannel = IrcChannel.Fetch(logLine.channelId, conn.server);
                            } else {
                                conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: $a:{1} is not in the log (?!)", nick.displayNick, searchAccount.account));
                                return;
                            }
                            break;

                        case "nick":
                            searchNick = IrcNick.Fetch(search.ToLower(), conn.server);
                            if (searchNick == null) {
                                conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: There is no nick `{1}` in the database", nick.displayNick, search));
                                return;
                            }
                            if (searchNick.accountId > 0) {
                                searchAccount = IrcAccount.Fetch(searchNick.accountId, conn.server);
                                searchAccount.SetNick(searchNick);
                            }
                            totalLines = IrcLog.FetchTotal(searchNick);
                            logLine = IrcLog.Fetch(searchNick, ignoreLogLine, channel);
                            if (logLine != null) {
                                searchChannel = IrcChannel.Fetch(logLine.channelId, conn.server);
                            } else {
                                conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: nick:{1} is not in the log (?!)", nick.displayNick, searchNick.displayNick));
                                return;
                            }
                            break;

                        case "channel":
                            searchChannel = IrcChannel.Fetch(search.ToLower(), conn.server);
                            if (searchChannel == null) {
                                conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: There is no channel `{1}` in the database", nick.displayNick, search));
                                return;
                            }
                            totalLines = IrcLog.FetchTotal(searchChannel);
                            logLine = IrcLog.Fetch(searchChannel, ignoreLogLine, (channel != null && channel.id != searchChannel.id ? channel : null));
                            if (logLine == null) {
                                conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: {1} is not in the log (?!)", nick.displayNick, searchChannel.name));
                                return;
                            }
                            break;
                    }

                    if (logLine == null) {
                        conn.SendPrivmsg(target, String.Format("\x02{0}\x0f: This message should never appear, what the HELL did you DO to me?! D:", nick.displayNick));
                        return;
                    }

                    string response = totalLines + " found, latest: " + (searchChannel != null ? searchChannel.name : conn.server.nick) + " [" + logLine.loggedAt.ToString("u") + "] " + logLine.line;
                    conn.SendPrivmsg(target, response);
                }
            } catch (Exception e) {
                Logger.Log(e);
                conn.SendPrivmsg(nick.nick, "Oof… My spleen…! I can't do that right now. :(");
            }
        }

        public static void HandleUnwrittenAdminCommand(IrcNick nick, string args, IrcConnection conn, IrcChannel channel = null)
        {
            try {
                if (nick.account == null || (!isOp(conn, nick) && !nick.account.isOp)) {
                    conn.SendPrivmsg(nick.nick, String.Format("Sorry, {0}, but that command doesn't exist. Try !help.", nick.nick));
                    return;
                }

                string target = nick.nick;
                if (channel != null && channel.allowCommandsInChannel) {
                    target = channel.name;
                }

                conn.SendPrivmsg(target, String.Format("Sorry, {0}, but that command hasn't been written yet. Poke TwoWholeWorms into finishing it.", nick.displayNick));
            } catch (Exception e) {
                Logger.Log(e);
                conn.SendPrivmsg(nick.nick, "Oof… Nnnnyeeeeurghnghpf………! I can't do that right now. :(");
            }
        }

        public static Image GetImageFromUrl(string url)
        {
            using (var webClient = new WebClient()) {
                return ByteArrayToImage(webClient.DownloadData(url));
            }
        }

        public static Image ByteArrayToImage(byte[] fileBytes)
        {
            using (var stream = new MemoryStream(fileBytes)) {
                return Image.FromStream(stream);
            }
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs) {
                if (codec.MimeType == mimeType) {
                    return codec;
                }
            }
            return null;
        }
    
    }

}
