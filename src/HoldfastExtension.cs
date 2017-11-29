using HoldfastGame;
using Oxide.Core;
using Oxide.Core.Extensions;
using Oxide.Core.RemoteConsole;
using Oxide.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Oxide.Game.Holdfast
{
    /// <summary>
    /// The extension class that represents this extension
    /// </summary>
    public class HoldfastExtension : Extension
    {
        internal static Assembly Assembly = Assembly.GetExecutingAssembly();
        internal static AssemblyName AssemblyName = Assembly.GetName();
        internal static VersionNumber AssemblyVersion = new VersionNumber(AssemblyName.Version.Major, AssemblyName.Version.Minor, AssemblyName.Version.Build);
        internal static string AssemblyAuthors = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly, typeof(AssemblyCompanyAttribute), false)).Company;

        /// <summary>
        /// Gets whether this extension is for a specific game
        /// </summary>
        public override bool IsGameExtension => true;

        /// <summary>
        /// Gets the name of this extension
        /// </summary>
        public override string Name => "Holdfast";

        /// <summary>
        /// Gets the author of this extension
        /// </summary>
        public override string Author => AssemblyAuthors;

        /// <summary>
        /// Gets the version of this extension
        /// </summary>
        public override VersionNumber Version => AssemblyVersion;

        /// <summary>
        /// Default game-specific references for use in plugins
        /// </summary>
        internal static readonly HashSet<string> DefaultReferences = new HashSet<string>
        {
        };

        /// <summary>
        /// List of assemblies allowed for use in plugins
        /// </summary>
        public override string[] WhitelistAssemblies => new[]
        {
            "Assembly-CSharp", "mscorlib", "Oxide.Core", "System", "System.Core", "UnityEngine"
        };

        /// <summary>
        /// List of namespaces allowed for use in plugins
        /// </summary>
        public override string[] WhitelistNamespaces => new[]
        {
            "HoldfastGame", "Steamworks", "System.Collections", "System.Security.Cryptography", "System.Text", "UnityEngine"
        };

        /// <summary>
        /// List of filter matches to apply to console output
        /// </summary>
        public static string[] Filter =
        {
            "uLink doesn't need a license key in this version"
        };

        /// <summary>
        /// Initializes a new instance of the HoldfastExtension class
        /// </summary>
        /// <param name="manager"></param>
        public HoldfastExtension(ExtensionManager manager) : base(manager)
        {
        }

        /// <summary>
        /// Loads this extension
        /// </summary>
        public override void Load()
        {
            Manager.RegisterPluginLoader(new HoldfastPluginLoader());
        }

        /// <summary>
        /// Loads plugin watchers used by this extension
        /// </summary>
        /// <param name="directory"></param>
        public override void LoadPluginWatchers(string directory)
        {
        }

        /// <summary>
        /// Called when all other extensions have been loaded
        /// </summary>
        public override void OnModLoad()
        {
            CSharpPluginLoader.PluginReferences.UnionWith(DefaultReferences);

            Application.logMessageReceived += HandleLog;

            if (Interface.Oxide.EnableConsole()) Interface.Oxide.ServerConsole.Input += ServerConsoleOnInput;
        }

        internal static void ServerConsole()
        {
            if (Interface.Oxide.ServerConsole == null) return;

            Interface.Oxide.ServerConsole.Title = () => $"{uLink.Network.connections.Length} | {HoldfastCore.serverConfig.ServerName ?? "Unnamed"}";

            Interface.Oxide.ServerConsole.Status1Left = () => HoldfastCore.serverConfig.ServerName ?? "Unnamed";
            Interface.Oxide.ServerConsole.Status1Right = () =>
            {
                var fps = Mathf.RoundToInt(1f / Time.smoothDeltaTime);
                var seconds = TimeSpan.FromSeconds(Time.realtimeSinceStartup);
                var uptime = $"{seconds.TotalHours:00}h{seconds.Minutes:00}m{seconds.Seconds:00}s".TrimStart(' ', 'd', 'h', 'm', 's', '0');
                return string.Concat(fps, "fps, ", uptime);
            };

            Interface.Oxide.ServerConsole.Status2Left = () => $"{uLink.Network.connections.Length}/{HoldfastCore.gameDetails.MaxPlayers} players";
            Interface.Oxide.ServerConsole.Status2Right = () =>
            {
                if (uLink.NetworkTime.serverTime <= 0) return "not connected";

                double bytesReceived = 0;
                double bytesSent = 0;
                foreach (var connection in uLink.Network.connections)
                {
                    var stats = connection.statistics;
                    if (stats == null) continue;

                    bytesReceived += stats.bytesReceivedPerSecond;
                    bytesSent += stats.bytesSentPerSecond;
                }
                return $"{Utility.FormatBytes(bytesReceived)}/s in, {Utility.FormatBytes(bytesSent)}/s out";
            };

            Interface.Oxide.ServerConsole.Status3Left = () =>
            {
                // TODO: Return game time and/or other game/round info
                return $"{HoldfastCore.gameDetails.GameMode} [{HoldfastCore.gameDetails.Map}]";
            };
            Interface.Oxide.ServerConsole.Status3Right = () => $"Oxide.Holdfast {AssemblyVersion} for {GameVersion.FullVersionText}";
            Interface.Oxide.ServerConsole.Status3RightColor = ConsoleColor.Yellow;
        }

        private static void ServerConsoleOnInput(string input)
        {
            input = input.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                // TODO: Implement console input
            }
        }

        private void HandleLog(string message, string stackTrace, LogType type)
        {
            if (string.IsNullOrEmpty(message) || Filter.Any(message.StartsWith)) return;

            // TODO: Strip color/formatting tags from message

            var color = ConsoleColor.Gray;
            var remoteType = "generic";
            if (type == LogType.Warning)
            {
                color = ConsoleColor.Yellow;
                remoteType = "warning";
            }
            else if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                color = ConsoleColor.Red;
                remoteType = "error";
            }

            Interface.Oxide.ServerConsole.AddMessage(message, color);
            Interface.Oxide.RemoteConsole.SendMessage(new RemoteMessage
            {
                Message = message,
                Identifier = 0,
                Type = remoteType,
                Stacktrace = stackTrace
            });
        }
    }
}
