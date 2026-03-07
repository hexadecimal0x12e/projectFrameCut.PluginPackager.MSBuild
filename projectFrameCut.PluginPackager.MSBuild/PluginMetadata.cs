using System;
using System.Collections.Generic;
using System.Text;

namespace projectFrameCut.PluginPackager.MSBuild
{
    public class PluginMetadata
    {
        /// <summary>
        /// The unique identifier of the plugin. Must equal to the full name of the main class implementing IPluginBase.
        /// </summary>
        public string PluginID { get; set; }
        /// <summary>
        /// The supported API version of the plugin.
        /// </summary>
        public int PluginAPIVersion { get; set; }
        /// <summary>
        /// The plugin's name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Plugin's author.
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The version of the plugin.
        /// </summary>
        public Version Version { get; set; }
        /// <summary>
        /// Author or project's homepage.
        /// </summary>
        public string AuthorUrl { get; set; }
        /// <summary>
        /// The publish page of the plugin. Used for update checking.
        /// </summary>
        public string PublishingUrl { get; set; }

        /// <summary>
        /// The encrypt key for a .NET Assembly-based plugin.
        /// For more information, see the bundler's documentation.
        /// </summary>
        public string PluginKey { get; set; }

        /// <summary>
        /// Hash of the plugin's source or assembly file.
        /// </summary>
        public string PluginHash { get; set; }

   
    }

}

