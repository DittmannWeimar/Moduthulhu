﻿using Discord;
using Lomztein.Moduthulhu.Core.Module;
using Lomztein.Moduthulhu.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding;
using Lomztein.Moduthulhu.Core.Extensions;

namespace Lomztein.Moduthulhu.Modules.Meta.Extensions
{
    public static class ModuleExtensions
    {
        public static Embed GetModuleEmbed (this IModule module) {
            EmbedBuilder builder = new EmbedBuilder ()
                .WithTitle (module.Name)
                .WithDescription (module.Description)
                .WithAuthor ("Module Information")
                .WithFooter ("Created by " + module.Author + " - " + module.AuthorURL)
                .AddField ("Multiserver", module.Multiserver, true)
                .AddField ("Autopatching", Uri.IsWellFormedUriString (module.PatchURL, UriKind.Absolute), true);

            AddDependanciesInline ("Prerequisite Modules", module.GetDependencyNames ());

            void AddDependanciesInline (string header, string[] dependancies) { // Never did I ever say I knew how to spell.

                if (dependancies.Length > 0) {

                    string content = "";
                    foreach (string dep in dependancies)
                        content += dep + "\n";

                    builder.AddField (header, content);
                }

            }

            return builder.Build ();
        }

        public static Embed GetModuleListEmbed (this ModuleContainer container) {
            IModule[] allModules = container.Modules.ToArray ();

            EmbedBuilder builder = new EmbedBuilder ()
                .WithAuthor ("Module Information")
                .WithTitle ("All Active Modules")
                .WithDescription ("All currently installed, enabled and active modules.")
                .WithFooter (allModules.Length + " modules active.");

            foreach (IModule module in allModules) {
                builder.AddField (module.Name, module.Description);
            }

            return builder.Build ();
        }
    }
}