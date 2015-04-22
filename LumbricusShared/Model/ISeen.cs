using System;

namespace TwoWholeWorms.Lumbricus.Shared.Model
{

    public interface ISeen
    {
        
        Account Account { get; set; }
        Channel Channel { get; set; }
        Nick    Nick    { get; set; }
        Server  Server  { get; set; }

        DateTime FirstSeenAt { get; set; }
        DateTime LastSeenAt  { get; set; }

    }

}

