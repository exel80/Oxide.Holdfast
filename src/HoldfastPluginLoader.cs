using Oxide.Core.Plugins;
using System;

namespace Oxide.Game.Holdfast
{
    /// <summary>
    /// Responsible for loading core plugins for "Holdfast: Nations at War"
    /// </summary>
    public class HoldfastPluginLoader : PluginLoader
    {
        public override Type[] CorePlugins => new[] { typeof(HoldfastCore) };
    }
}
