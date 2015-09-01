using NLog;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Shared.Plugins;
using TwoWholeWorms.Lumbricus.Shared.Plugins.Core;
using TwoWholeWorms.Lumbricus.Shared.Utilities;

namespace TwoWholeWorms.Lumbricus
{

	public static class Lumbricus
	{

        static List<IrcConnection> connections;
        static List<Thread> threads;

        public static LumbricusConfiguration config;

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

		public static void Main(string[] args)
		{
            try {
                connections = new List<IrcConnection>();
                threads     = new List<Thread>();

                config = LumbricusConfiguration.GetConfig();

                logger.Info("Lumbricus v{0}", CoreAssembly.Version);
                logger.Info("==========={0}\n", new String('=', CoreAssembly.Version.ToString().Length));

                Initialise();

                while (threads.Count > 0) {
                    foreach (Thread thread in threads) {
                        if (!thread.IsAlive) threads.Remove(thread);
                    }
                    Thread.Sleep(1000);
                }

                LumbricusContext.Obliterate();

                logger.Info("All connections closed, threads finished. BAIBAI! :D");
            } catch (Exception e) {
                logger.Fatal(e);
            }
        }

        public static void Initialise()
        {
            InitialisePlugins(config);

            LumbricusContext.Initialise(config);

            logger.Debug("Initialising server connections");
            var servers = Server.Fetch();
            foreach (Server server in servers) {
                IrcConnection conn = new IrcConnection(server, config);
                connections.Add(conn);
            }

            foreach (IrcConnection conn in connections) {
                var channels = Channel.Fetch(conn.Server);
                conn.Server.SetChannels(channels.ToList());
            }

            logger.Debug("Starting connections");
            foreach (IrcConnection conn in connections) {
                Thread t = new Thread(() => RunThread(conn));
                t.Start();

                threads.Add(t);
            }
		}

        public static void InitialisePlugins(LumbricusConfiguration config)
        {
            logger.Info("Initialising plugins");
            string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

            // Add the plugin initialiser
            AppDomain.CurrentDomain.AssemblyLoad += PluginInitializer;

            PluginConfigurationElement plugin = null;
            foreach (string dll in Directory.GetFiles(pluginsPath, "*.dll", SearchOption.TopDirectoryOnly)) {
                try {
                    string pluginFileName = Path.GetFileNameWithoutExtension(dll);
                    plugin = config.PluginConfigs.SingleOrDefault(x => x.Name == pluginFileName);
                    if (plugin == null) {
                        logger.Trace("Skipping assembly `{0}` as it is not mentioned in the configuration", dll);
                        continue;
                    }
                    if (!plugin.Enabled) {
                        logger.Trace("Skipping assembly `{0}` as it is disabled in the configuration", dll);
                        continue;
                    }
                    Assembly loadedAssembly = Assembly.LoadFile(dll);
                    logger.Trace("Loaded plugin `{0}`", loadedAssembly.GetName());
                } catch (FileLoadException e) {
                    logger.Trace("Plugin file `{0}` has already been loaded? O.o", dll);
                    logger.Trace(e);
                } catch (BadImageFormatException e) {
                    logger.Trace("`{0}` is not a valid Lumbricus plugin file.", dll);
                    throw e;
                }
            }

            // Remove the plugin initialiser as it's no longer needed
            AppDomain.CurrentDomain.AssemblyLoad -= PluginInitializer;

            plugin = config.PluginConfigs.SingleOrDefault(x => x.Name == "HelpPlugin");
            if (plugin != null && plugin.Enabled) {
                LumbricusConfiguration.AddPlugin(new HelpPlugin());
            } else {
                logger.Trace("Skipping core plugin `HelpPlugin`");
            }

            plugin = config.PluginConfigs.SingleOrDefault(x => x.Name == "SeenPlugin");
            if (plugin != null && plugin.Enabled) {
                LumbricusConfiguration.AddPlugin(new SeenPlugin());
            } else {
                logger.Trace("Skipping core plugin `SeenPlugin`");
            }

            plugin = config.PluginConfigs.SingleOrDefault(x => x.Name == "TrackBanPlugin");
            if (plugin != null && plugin.Enabled) {
                LumbricusConfiguration.AddPlugin(new TrackBanPlugin());
            } else {
                logger.Trace("Skipping core plugin `TrackBanPlugin`");
            }

            plugin = config.PluginConfigs.SingleOrDefault(x => x.Name == "TrackKickPlugin");
            if (plugin != null && plugin.Enabled) {
                LumbricusConfiguration.AddPlugin(new TrackKickPlugin());
            } else {
                logger.Trace("Skipping core plugin `TrackKickPlugin`");
            }

            plugin = config.PluginConfigs.SingleOrDefault(x => x.Name == "TrackUserPlugin");
            if (plugin != null && plugin.Enabled) {
                LumbricusConfiguration.AddPlugin(new TrackUserPlugin());
            } else {
                logger.Trace("Skipping core plugin `TrackUserPlugin`");
            }

            logger.Debug("{0} plugins enabled", LumbricusConfiguration.Plugins.Count);
        }

        static void PluginInitializer(object sender, AssemblyLoadEventArgs args)
        {
            logger.Trace("{0} loaded plugin assembly {1}", sender.ToString(), args.LoadedAssembly.GetName());
            // Call the static constructors on each plugin class
            foreach (LumbricusPlugin attr in args.LoadedAssembly.GetCustomAttributes(typeof(LumbricusPlugin), false)) {
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(attr.Type.TypeHandle);
            }
        }

        public static void RunThread(IrcConnection conn)
        {
            try {
                conn.Run();
            } catch (Exception e) {
                logger.Error(e);
            }
        }

	}

}
