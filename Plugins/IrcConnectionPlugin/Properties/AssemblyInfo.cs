using System.Resources;
using System.Reflection;
using TwoWholeWorms.Lumbricus.Plugins.IrcConnectionPlugin;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

[assembly: AssemblyTitle("IrcConnectionPlugin")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("IrcConnectionPlugin")]
[assembly: AssemblyCopyright("Copyright © Benjamin Nolan 2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en")]

// This tells the core system which class to initialise when the assembly is loaded.
[assembly: LumbricusPlugin(typeof(IrcPlugin))]

[assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyFileVersion("1.0.*")]
