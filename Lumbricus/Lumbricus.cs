using NLog;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Plugins;
using TwoWholeWorms.Lumbricus.Shared.Utilities;

namespace TwoWholeWorms.Lumbricus
{

	public static class Lumbricus
	{

        public delegate AbstractConnectionPlugin ConnectionPluginDelegate();
        public static ConnectionPluginDelegate ConnectionPlugins;

        static List<AbstractConnectionPlugin> connectionPlugins;
        static List<Thread> threads;

        public static LumbricusConfiguration config;

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

		public static void Main(string[] args)
		{
            try {
                connectionPlugins = new List<AbstractConnectionPlugin>();
                threads           = new List<Thread>();

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

            logger.Debug("Initialising connection plugins");
            foreach (ConnectionPluginDelegate connectionPlugin in ConnectionPlugins.GetInvocationList()) {
                try {
                    AbstractConnectionPlugin plugin = connectionPlugin();
                    connectionPlugins.Add(plugin);
                } catch (Exception e) {
                    logger.Error(e);
                }
            }

            logger.Debug("Starting threads");
            foreach (AbstractConnectionPlugin connectionPlugin in connectionPlugins) {
                try {
                    Thread t = new Thread(() => RunThread(connectionPlugin));
                    t.Start();

                    threads.Add(t);
                } catch (Exception e) {
                    logger.Error(e);
                }
            }
        }

        public static void InitialisePlugins(LumbricusConfiguration config)
        {
            logger.Info("Initialising plugins");
            string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

            // Add the plugin initialiser to the AssemblyLoad delegate so they're handled correctly when loaded
            AppDomain.CurrentDomain.AssemblyLoad += PluginInitialiser;

            PluginConfigElement plugin = null;
            
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
                } catch (Exception e) {
                    logger.Trace("`{0}` generated an unknown exception whilst attempting to load it.", dll);
                    throw e;
                }
            }

            // Remove the plugin initialiser as it's no longer needed
            AppDomain.CurrentDomain.AssemblyLoad -= PluginInitialiser;

            logger.Info("{0} plugins enabled", LumbricusConfiguration.Plugins.Count);
        }

        static void PluginInitialiser(object sender, AssemblyLoadEventArgs args)
        {
            logger.Trace("{0} loaded plugin assembly {1}", sender.ToString(), args.LoadedAssembly.GetName());
            // Call the static constructors on each plugin class to register them with the main thread
            foreach (LumbricusPlugin attr in args.LoadedAssembly.GetCustomAttributes(typeof(LumbricusPlugin), false)) {
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(attr.Type.TypeHandle);
            }
        }

        public static void RunThread(AbstractConnectionPlugin plugin)
        {
            try {
                plugin.Run();
            } catch (Exception e) {
                logger.Error(e);
            }
        }

	}

}
