namespace BananaPlugin.Schematics;

using BananaPlugin.API.Utils;
using SLEditor.API;
using SLEditor.API.ReadWriters;
using System;
using System.Collections.Generic;

/// <summary>
/// The main class responsible for handling loading of schematics.
/// </summary>
[Obsolete("Not implemented yet.", true)]
public static class SchematicLoader
{
    /// <summary>
    /// A dictionary containing all loaded schematics by their name.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, SchematicInstance> AllSchematics;

    static SchematicLoader()
    {
        string[] resources = typeof(SchematicLoader).Assembly.GetManifestResourceNames();

        Dictionary<string, SchematicInstance> schems = new();

        for (int i = 0; i < resources.Length; i++)
        {
            if (!resources[i].StartsWith("BananaPlugin.Schematics."))
            {
                continue;
            }

            string name = resources[i].Substring(24);
            name = name.Remove(name.IndexOf(".txt"), 4);
            BPLogger.Debug(name);

            using SchematicReader reader = new(typeof(SchematicLoader).Assembly.GetManifestResourceStream(resources[i]));

            schems.Add(name, reader.ReadSchematic());
        }

        AllSchematics = schems;
    }
}
