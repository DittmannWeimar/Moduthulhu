﻿using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugin;
using Lomztein.Moduthulhu.Core.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild
{
    public class PluginManager
    {
        private readonly List<IPlugin> _activePlugins = new List<IPlugin>();
        public IPlugin[] ActivePlugins => _activePlugins.ToArray();

        private GuildHandler _parentHandler;

        private CachedValue<List<string>> _enabledPlugins;
        private Dictionary<string, Func<object, object>> _messageEndpoints = new Dictionary<string, Func<object, object>>();

        public PluginManager (GuildHandler parent)
        {
            _parentHandler = parent;
            _enabledPlugins = new CachedValue<List<string>>(new IdentityKeyJsonRepository("plugindata"), _parentHandler.GuildId, "EnabledPlugins", () => PluginLoader.GetAllPlugins ().Where (x => PluginLoader.IsStandard (x)).Select (y => Plugin.Framework.Plugin.CompactizeName (y)).ToList ());
            _enabledPlugins.SetValue (PluginLoader.GetAllPlugins().Where(x => PluginLoader.IsStandard(x)).Select(y => Plugin.Framework.Plugin.CompactizeName(y)).ToList ()); // This forces the manager to load all standard plugins at startup, remove later and replace with togglability.
        }

        public void ShutdownPlugins ()
        {
            Log.Write(Log.Type.PLUGIN, "Shutting down plugins for guild " + _parentHandler.GetGuild().Name);
            foreach (IPlugin plugin in _activePlugins)
            {
                Log.Write(Log.Type.PLUGIN, "Shutting down plugin " + Plugin.Framework.Plugin.CompactizeName(plugin.GetType()));
                plugin.Shutdown();
            }
            _activePlugins.Clear();
        }

        public void ReloadPlugins ()
        {
            ShutdownPlugins();
            Log.Write(Log.Type.PLUGIN, "Reloading plugins for guild " + _parentHandler.GetGuild().Name);
            string[] toLoad = _enabledPlugins.GetValue().ToArray ();

            foreach (string name in toLoad)
            {
                Type pluginType = PluginLoader.GetPluginType(name);
                if (pluginType == null)
                {
                    Log.Write(Log.Type.WARNING, $"Attempted to instantiate unloaded/unknown plugin type {name}");
                }
                else
                {
                    IPlugin plugin = AssemblyLoader.Instantiate<IPlugin>(pluginType);
                    _activePlugins.Add(plugin);
                }
            }

            foreach (IPlugin plugin in _activePlugins)
            {
                Log.Write(Log.Type.PLUGIN, "Pre-initializing plugin " + Plugin.Framework.Plugin.CompactizeName(plugin.GetType()));
                plugin.PreInitialize(_parentHandler);
            }

            foreach (IPlugin plugin in _activePlugins)
            {
                Log.Write(Log.Type.PLUGIN, "Initializng plugin " + Plugin.Framework.Plugin.CompactizeName(plugin.GetType()));
                plugin.Initialize ();
            }

            foreach (IPlugin plugin in _activePlugins)
            {
                Log.Write(Log.Type.PLUGIN, "Post-initializing plugin " + Plugin.Framework.Plugin.CompactizeName(plugin.GetType()));
                plugin.PostInitialize ();
            }
        }

        public object SendMessage (string target, object value = null)
        {
            if (_messageEndpoints.ContainsKey (target))
            {
                return _messageEndpoints[target](value);
            }
            else
            {
                Log.Write(Log.Type.WARNING, $"Tried to call message function {target}, but it hasn't been registered.");
                return null;
            }
        }

        public T SendMessage<T>(string target, object value = null) => (T)SendMessage(target, value);

        public void RegisterMessageFunction (string identifier, Func<object, object> function)
        {
            Log.Write(Log.Type.CONFIG, $"Registering message function {identifier}..");
            if (_messageEndpoints.ContainsKey (identifier))
            {
                Log.Write(Log.Type.WARNING, $"Attempted to register message function {identifier}, but it already exists.");
            }
            else
            {
                _messageEndpoints.Add(identifier, function);
            }
        }

        public void UnregisterMessageFunction (string identifier)
        {
            _messageEndpoints.Remove(identifier);
        }
        public void AddPlugin (string pluginName)
        {
            _enabledPlugins.GetValue().Add(pluginName);
            _enabledPlugins.Store();
        }

        public void RemovePlugin (string pluginName)
        {
            _enabledPlugins.GetValue().Remove(pluginName);
            _enabledPlugins.Store();
        }
    }
}
