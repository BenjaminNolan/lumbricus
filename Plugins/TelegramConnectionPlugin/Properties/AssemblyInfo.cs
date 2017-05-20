using System.Resources;
using System.Reflection;
using TwoWholeWorms.Lumbricus.Plugins.TelegramConnectionPlugin;
using TwoWholeWorms.Lumbricus.Shared.Plugins;

[assembly: AssemblyTitle("TelegramConnectionPlugin")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("TelegramConnectionPlugin")]
[assembly: AssemblyCopyright("Copyright © Benjamin Nolan 2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en")]

// This tells the core system which class to initialise when the assembly is loaded.
[assembly: LumbricusPlugin(typeof(TelegramPlugin))]

[assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyFileVersion("1.0.*")]
