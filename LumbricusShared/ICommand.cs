using TwoWholeWorms.Lumbricus.Shared.Model;

namespace TwoWholeWorms.Lumbricus.Shared
{
    
    public interface ICommand
    {
        string Name { get; }
        void HandleCommand(IrcLine line, Nick nick, Account account);
    }

}
