namespace TwoWholeWorms.Lumbricus.Shared
{

    public struct IrcLine
    {
        public string   RawLine;
        public string   FullHost;
        public string   AccountName;
        public string   Nick;
        public string   User;
        public string   Host;
        public string   ServerHost;
        public string   IrcCommand;
        public string[] IrcCommandArgs;
        public string   IrcCommandArgsRaw;
        public string   FullCommand;
        public string   Command;
        public string   Args;
        public string   Trail;
    }

}
