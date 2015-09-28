using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.TellPlugin.Commands
{

    public class TellCommand : AbstractCommand
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public TellCommand(IrcConnection conn) : base(conn)
        {
            // …
        }

        public override string Name {
            get {
                return "Tell Command";
            }
        }

        public override void HandleCommand(IrcLine line, Nick nick, Channel channel)
        {
            try {
            } catch (Exception e) {
                logger.Error(e);
                conn.SendPrivmsg(nick.Name, "¡Ay caray! It's Lupus! D:");
            }
        }

    }

}
