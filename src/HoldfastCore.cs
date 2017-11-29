using HoldfastGame;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Game.Holdfast.Libraries.Covalence;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxide.Game.Holdfast
{
    /// <summary>
    /// The core Holdfast: Nations at War plugin
    /// </summary>
    public partial class HoldfastCore : CSPlugin
    {
        #region Initialization

        /// <summary>
        /// Initializes a new instance of the HoldfastCore class
        /// </summary>
        public HoldfastCore()
        {
            // Set plugin info attributes
            Title = "Holdfast: Nations at War";
            Author = HoldfastExtension.AssemblyAuthors;
            Version = HoldfastExtension.AssemblyVersion;
        }

        // Libraries
        //internal readonly Command cmdlib = Interface.Oxide.GetLibrary<Command>();
        internal readonly Lang lang = Interface.Oxide.GetLibrary<Lang>();
        internal readonly Permission permission = Interface.Oxide.GetLibrary<Permission>();

        // Game instances
        internal static ServerAdminBroadcastMessageManager messageManager;
        internal static ServerBannedPlayersManager banManager;
        internal static ServerConfigurationFile serverConfig;
        internal static ServerConnectionManager connectionManager;
        internal static GameDetails gameDetails;
        internal static ServerGameManager gameManager;

        // Oxide instances
        internal static readonly HoldfastCovalenceProvider Covalence = HoldfastCovalenceProvider.Instance;
        internal readonly PluginManager pluginManager = Interface.Oxide.RootPluginManager;
        internal readonly IServer Server = Covalence.CreateServer();

        // Commands that a plugin can't override
        internal static IEnumerable<string> RestrictedCommands => new[]
        {
            ""
        };

        private bool serverInitialized;

        #endregion Initialization

        #region Core Hooks

        [HookMethod("Init")]
        private void Init()
        {
            // Configure remote error logging
            RemoteLogger.SetTag("game", Title.ToLower());
            RemoteLogger.SetTag("game version", Server.Version);

            // Add core plugin commands
            AddCovalenceCommand(new[] { "oxide.plugins", "o.plugins", "plugins" }, "PluginsCommand", "oxide.plugins");
            AddCovalenceCommand(new[] { "oxide.load", "o.load", "plugin.load" }, "LoadCommand", "oxide.load");
            AddCovalenceCommand(new[] { "oxide.reload", "o.reload", "plugin.reload" }, "ReloadCommand", "oxide.reload");
            AddCovalenceCommand(new[] { "oxide.unload", "o.unload", "plugin.unload" }, "UnloadCommand", "oxide.unload");

            // Add core permission commands
            AddCovalenceCommand(new[] { "oxide.grant", "o.grant", "perm.grant" }, "GrantCommand", "oxide.grant");
            AddCovalenceCommand(new[] { "oxide.group", "o.group", "perm.group" }, "GroupCommand", "oxide.group");
            AddCovalenceCommand(new[] { "oxide.revoke", "o.revoke", "perm.revoke" }, "RevokeCommand", "oxide.revoke");
            AddCovalenceCommand(new[] { "oxide.show", "o.show", "perm.show" }, "ShowCommand", "oxide.show");
            AddCovalenceCommand(new[] { "oxide.usergroup", "o.usergroup", "perm.usergroup" }, "UserGroupCommand", "oxide.usergroup");

            // Add core misc commands
            AddCovalenceCommand(new[] { "oxide.lang", "o.lang" }, "LangCommand");
            AddCovalenceCommand(new[] { "oxide.version", "o.version" }, "VersionCommand");

            // Register messages for localization
            foreach (var language in Core.Localization.languages) lang.RegisterMessages(language.Value, this, language.Key);

            // Setup default permission groups
            if (permission.IsLoaded)
            {
                var rank = 0;
                foreach (var defaultGroup in Interface.Oxide.Config.Options.DefaultGroups)
                    if (!permission.GroupExists(defaultGroup)) permission.CreateGroup(defaultGroup, defaultGroup, rank++);

                permission.RegisterValidate(s =>
                {
                    ulong temp;
                    if (!ulong.TryParse(s, out temp)) return false;
                    var digits = temp == 0 ? 1 : (int)Math.Floor(Math.Log10(temp) + 1);
                    return digits >= 17;
                });

                permission.CleanUp();
            }
        }

        [HookMethod("OnPluginLoaded")]
        private void OnPluginLoaded(Plugin plugin)
        {
            // Call OnServerInitialized for hotloaded plugins
            if (serverInitialized) plugin.CallHook("OnServerInitialized");
        }

        [HookMethod("OnServerInitialized")]
        private void OnServerInitialized()
        {
            if (serverInitialized) return;

            Analytics.Collect();
            HoldfastExtension.ServerConsole();
            SteamGameServer.SetGameTags("modded,oxide");

            messageManager = ServerComponentReferenceManager.ServerInstance.serverAdminBroadcastMessageManager;
            banManager = ServerComponentReferenceManager.ServerInstance.serverBannedPlayersManager;
            serverConfig = ServerConfigurationFileManager.CurrentConfigurationFile;
            connectionManager = ServerComponentReferenceManager.ServerInstance.serverConnectionManager;
            gameDetails = ServerComponentReferenceManager.ServerInstance.serverGameManager.CreateGameDetailsFromConfigFile(0);
            gameManager = ServerComponentReferenceManager.ServerInstance.serverGameManager;

            serverInitialized = true;
        }

        [HookMethod("OnServerShutdown")]
        private void OnServerShutdown() => Interface.Oxide.OnShutdown();

        #endregion Core Hooks

        #region Command Handling

        /// <summary>
        /// Parses the specified chat command
        /// </summary>
        /// <param name="argstr"></param>
        /// <param name="cmd"></param>
        /// <param name="args"></param>
        private void ParseCommand(string argstr, out string cmd, out string[] args)
        {
            var arglist = new List<string>();
            var sb = new StringBuilder();
            var inlongarg = false;
            foreach (var c in argstr)
            {
                if (c == '"')
                {
                    if (inlongarg)
                    {
                        var arg = sb.ToString().Trim();
                        if (!string.IsNullOrEmpty(arg)) arglist.Add(arg);
                        sb = new StringBuilder();
                        inlongarg = false;
                    }
                    else
                        inlongarg = true;
                }
                else if (char.IsWhiteSpace(c) && !inlongarg)
                {
                    var arg = sb.ToString().Trim();
                    if (!string.IsNullOrEmpty(arg)) arglist.Add(arg);
                    sb = new StringBuilder();
                }
                else
                    sb.Append(c);
            }
            if (sb.Length > 0)
            {
                var arg = sb.ToString().Trim();
                if (!string.IsNullOrEmpty(arg)) arglist.Add(arg);
            }
            if (arglist.Count == 0)
            {
                cmd = null;
                args = null;
                return;
            }
            cmd = arglist[0];
            arglist.RemoveAt(0);
            args = arglist.ToArray();
        }

        // TODO: Add hook wrapper for command handling

        #endregion Command Handling

        #region Helpers

        /// <summary>
        /// Checks if the permission system has loaded, shows an error if it failed to load
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private bool PermissionsLoaded(IPlayer player)
        {
            if (permission.IsLoaded) return true;
            player.Reply(string.Format(lang.GetMessage("PermissionsNotLoaded", this, player.Id), permission.LastException.Message));
            return false;
        }

        #endregion Helpers
    }
}
