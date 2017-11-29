using Oxide.Core.Libraries.Covalence;

namespace Oxide.Game.Holdfast.Libraries.Covalence
{
    /// <summary>
    /// Provides Covalence functionality for the game "Holdfast: Nations at War"
    /// </summary>
    public class HoldfastCovalenceProvider : ICovalenceProvider
    {
        /// <summary>
        /// Gets the name of the game for which this provider provides
        /// </summary>
        public string GameName => "Holdfast: Nations at War";

        /// <summary>
        /// Gets the Steam app ID of the game's client, if available
        /// </summary>
        public uint ClientAppId => 589290;

        /// <summary>
        /// Gets the Steam app ID of the game's server, if available
        /// </summary>
        public uint ServerAppId => 589290;

        /// <summary>
        /// Gets the singleton instance of this provider
        /// </summary>
        internal static HoldfastCovalenceProvider Instance { get; private set; }

        public HoldfastCovalenceProvider()
        {
            Instance = this;
        }

        /// <summary>
        /// Gets the player manager
        /// </summary>
        public HoldfastPlayerManager PlayerManager { get; private set; }

        /// <summary>
        /// Gets the command system provider
        /// </summary>
        public HoldfastCommandSystem CommandSystem { get; private set; }

        /// <summary>
        /// Creates the game-specific server object
        /// </summary>
        /// <returns></returns>
        public IServer CreateServer() => new HoldfastServer();

        /// <summary>
        /// Creates the game-specific player manager object
        /// </summary>
        /// <returns></returns>
        public IPlayerManager CreatePlayerManager()
        {
            PlayerManager = new HoldfastPlayerManager();
            PlayerManager.Initialize();
            return PlayerManager;
        }

        /// <summary>
        /// Creates the game-specific command system provider object
        /// </summary>
        /// <returns></returns>
        public ICommandSystem CreateCommandSystemProvider() => CommandSystem = new HoldfastCommandSystem();

        /// <summary>
        /// Formats the text with markup as specified in Oxide.Core.Libraries.Covalence.Formatter
        /// into the game-specific markup language
        /// </summary>
        /// <param name="text">text to format</param>
        /// <returns>formatted text</returns>
        public string FormatText(string text) => Formatter.ToPlaintext(text);
    }
}
