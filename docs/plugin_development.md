# Lumbricus plugin development

Lumbricus is designed with modularity in mind. Commands are implemented as
plugins, most of which can be enabled and disabled for specific servers,
channels, or in certain cases even users.

Building a plugin for Lumbricus is pretty easy. Just follow this guide:

Step one is to create a new portable .NET library project.

Next, add a reference to LumbricusShared.dll. This will add in all the standard
models, etc, which you'll need in order to build your plugin.

Now, create a class for your plugin. This needs to implement
`TwoWholeWorms.Lumbricus.Shared.Plugins.AbstractPlugin` and additionally
contain a static constructor which is used in the initialisation process.

Par exemple:

    using TwoWholeWorms.Lumbricus.Shared;
    using TwoWholeWorms.Lumbricus.Shared.Plugins;

    namespace MyLumbricusPlugin
    {

        public class MyLumbricusPlugin : AbstractPlugin
        {

            static MyLumbricusPlugin()
            {
                LumbricusConfiguration.AddPlugin(new MyLumbricusPlugin());
            }

            #region AbstractPlugin implementation
            public override string Name {
                get {
                    return "My Lumbricus Plugin";
                }
            }

            public override void RegisterPlugin(IrcConnection conn)
            {
                conn.RegisterCommand("!mycommand", new Commands.MyCommand(conn));
            }
            #endregion

        }

    }

First, you need a static constructor. This constructor will be
called automatically by the bot's plugin initialisation code when it loads the
assembly file, and it should contain a call to `LumbricusConfiguration.AddPlugin`
to add a new instance of your plugin to the registry.

`public override string Name` should return the human-readable name of your
plugin. This will be used in log files, referenced in some administrative bot
commands, and will be displayed on the plugins page of the bot's web interface
if you have this module enabled.

Finally, is `public override void RegisterPlugin`, which will be called when the
IrcConnection is in its initialisation stages and registers the command with its
various handlers.

To register a command, call:

    conn.RegisterCommand(string commandName, AbstractCommand command)

To add a callback which processes IRC lines directly, add it to the
ProcessIrcLine delegate, eg:

    conn.ProcessIrcLine += MyLineProcessorMethod;

Now you need to add the following line to `Properties/AssemblyInfo.cs`:

    [assembly: LumbricusPlugin(typeof(MyLumbricusPlugin))]

You'll also need a `using TwoWholeWorms.Lumbricus.Shared.Plugins;` statement,
and probably one for the namespace of your plugin class as well.

The `LumbricusPlugin` attribute tells the core programme that the type specified
is the initialisation point for a plugin. An assembly can contain several
plugins, so if you want to implement multiple plugins in a single assembly then
just duplicate this line once for each plugin class.
