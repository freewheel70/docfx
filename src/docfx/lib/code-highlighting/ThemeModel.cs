// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[assembly: InternalsVisibleTo("Microsoft.Docs.CSS")]

namespace Microsoft.Docs.Build;

internal class ThemeModel
{
    public string? Name { get; init; }

    public List<TokenRule> TokenColors { get; set; } = new List<TokenRule>();

    public Dictionary<string, string> Colors { get; init; } = new Dictionary<string, string>();

    public string? Include { get; init; }

    public JObject? ExtensionData { get; init; }

    public string? GetDefaultBackground() => GetDefaultColorByKey("editor.background") ?? "#ffffff";

    public string? GetDefaultForeground() => GetDefaultColorByKey("editor.foreground") ?? "#000000";

    public static string GetSelectionBackground() => "#264F78";

    [OnDeserialized]
    internal void PostProcess(StreamingContext context)
    {
        MergeIncludedThemeModel();
        SetFontstyles();
    }

    private void SetFontstyles()
    {
        foreach (var item in TokenColors)
        {
            item.Settings.SetFontStyles();
        }
    }

    private void MergeIncludedThemeModel()
    {
        if (string.IsNullOrWhiteSpace(Include))
        {
            return;
        }

        var themeModel = JsonConvert.DeserializeObject<ThemeModel>(File.ReadAllText(Path.Combine("./lib/code-highlighting/theme", Include)));

        if (themeModel?.TokenColors is not null)
        {
            TokenColors.InsertRange(0, themeModel?.TokenColors ?? new List<TokenRule>());
        }

        if (themeModel?.Colors is not null)
        {
            foreach (var color in themeModel?.Colors!)
            {
                if (!Colors.ContainsKey(color.Key))
                {
                    Colors.Add(color.Key, color.Value);
                }
            }
        }
    }

    private string? GetDefaultColorByKey(string key)
    {
        if (Colors.TryGetValue(key, out var color))
        {
            return color;
        }

        return string.Empty;
    }
}
