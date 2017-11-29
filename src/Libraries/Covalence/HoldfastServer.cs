using HoldfastGame;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Globalization;
using System.Net;
using UnityEngine;

namespace Oxide.Game.Holdfast.Libraries.Covalence
{
    /// <summary>
    /// Represents the server hosting the game instance
    /// </summary>
    public class HoldfastServer : IServer
    {
        #region Information

        /// <summary>
        /// Gets/sets the public-facing name of the server
        /// </summary>
        public string Name
        {
            get { return "Name not found"; } // TODO
            set { } // TODO
        }

        private static IPAddress address;

        /// <summary>
        /// Gets the public-facing IP address of the server, if known
        /// </summary>
        public IPAddress Address
        {
            get
            {
                try
                {
                    if (address != null) return address;

                    var webClient = new WebClient();
                    IPAddress.TryParse(webClient.DownloadString("http://api.ipify.org"), out address);
                    return address;
                }
                catch (Exception ex)
                {
                    RemoteLogger.Exception("Couldn't get server IP address", ex);
                    return new IPAddress(0);
                }
            }
        }

        /// <summary>
        /// Gets the public-facing network port of the server, if known
        /// </summary>
        public ushort Port => 0; // TODO

        /// <summary>
        /// Gets the version or build number of the server
        /// </summary>
        public string Version => GameVersion.FullVersionText;

        /// <summary>
        /// Gets the network protocol version of the server
        /// </summary>
        public string Protocol => Version;

        /// <summary>
        /// Gets the language set by the server
        /// </summary>
        public CultureInfo Language => CultureInfo.InstalledUICulture;

        /// <summary>
        /// Gets the total of players currently on the server
        /// </summary>
        public int Players => uLinkNetworkConnectionsCollection.connections.Length;

        /// <summary>
        /// Gets/sets the maximum players allowed on the server
        /// </summary>
        public int MaxPlayers
        {
            get { return 0; } // TODO
            set { } // TODO
        }

        /// <summary>
        /// Gets/sets the current in-game time on the server
        /// </summary>
        public DateTime Time
        {
            get { return DateTime.Now; } // TODO
            set { } // TODO
        }

        /// <summary>
        /// Gets information on the currently loaded save file
        /// </summary>
        public SaveInfo SaveInfo => null; // TODO: Implement when possible

        #endregion Information

        #region Administration

        /// <summary>
        /// Bans the player for the specified reason and duration
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        /// <param name="duration"></param>
        public void Ban(string id, string reason, TimeSpan duration = default(TimeSpan))
        {
            if (IsBanned(id)) return;

            string ipAddress;
            HoldfastCore.banManager.BanPlayer(Convert.ToInt32(id), out ipAddress);
        }

        /// <summary>
        /// Gets the amount of time remaining on the player's ban
        /// </summary>
        /// <param name="id"></param>
        public TimeSpan BanTimeRemaining(string id) => TimeSpan.MaxValue;

        /// <summary>
        /// Gets if the player is banned
        /// </summary>
        /// <param name="id"></param>
        public bool IsBanned(string id)
        {
            ulong steamId;
            return HoldfastCore.gameManager.connectedPlayersSteamIDs.TryGetValue(Convert.ToInt32(id), out steamId);
        }

        /// <summary>
        /// Saves the server and any related information
        /// </summary>
        public void Save()
        {
            // TODO: Implement when possible
        }

        /// <summary>
        /// Unbans the player
        /// </summary>
        /// <param name="id"></param>
        public void Unban(string id)
        {
            if (!IsBanned(id)) return;

            HoldfastCore.connectionManager.DisconnectPlayer(Convert.ToInt32(id));
        }

        #endregion Administration

        #region Chat and Commands

        /// <summary>
        /// Broadcasts the specified chat message and prefix to all players
        /// </summary>
        /// <param name="message"></param>
        /// <param name="prefix"></param>
        /// <param name="args"></param>
        public void Broadcast(string message, string prefix, params object[] args)
        {
            // TODO: Add prefix and args formatting
            HoldfastCore.messageManager.BroadcastAdminMessage(message);
            Debug.Log($"[Chat] {message}");
        }

        /// <summary>
        /// Broadcasts the specified chat message to all players
        /// </summary>
        /// <param name="message"></param>
        public void Broadcast(string message) => Broadcast(message, null);

        /// <summary>
        /// Runs the specified server command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        public void Command(string command, params object[] args)
        {
            // TODO: Implement when possible
        }

        #endregion Chat and Commands
    }
}
