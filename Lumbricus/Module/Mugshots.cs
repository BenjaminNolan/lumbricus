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
    
    public static class Mugshots : AbstractModule
    {



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
    
    }

}
