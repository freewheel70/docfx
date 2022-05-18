// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Docs.Build;

namespace Microsoft.Docs.CSS;

internal class CSSGenerator
{
    private const string Filename = "code-highlighting.css";

    public static async Task GenerateCSSFile(ThemeModel model)
    {
        using var fs = new StreamWriter(Filename, true);

        foreach (var tokenRule in model.TokenColors)
        {
            var cssClassName = tokenRule.GetCSSClassName();
            var fontstyleClassName = tokenRule.GetFontstyleClassName();

            await fs.WriteLineAsync($"{cssClassName} {{");
            await fs.WriteLineAsync($"  color: {tokenRule.Settings.Foreground ?? model.GetDefaultForeground()};");
            await fs.WriteLineAsync($"  background-color: {tokenRule.Settings.Background ?? model.GetDefaultBackground()};");
            await fs.WriteLineAsync("}");

            var fontstyleSet = tokenRule.Settings.FontstyleSet;
            if (fontstyleSet is null || fontstyleSet.Count == 0)
            {
                continue;
            }

            await fs.WriteLineAsync($"{fontstyleClassName} {{");

            foreach (var fontstyle in fontstyleSet)
            {
                switch (fontstyle)
                {
                    case FontStyle.Italic:
                        await fs.WriteLineAsync("  font-style: italic;");
                        break;
                    case FontStyle.Bold:
                        await fs.WriteLineAsync("  font-weight: bold;");
                        break;
                }
            }

            if (fontstyleSet.Contains(FontStyle.Underline) && fontstyleSet.Contains(FontStyle.StrikeThrough))
            {
                await fs.WriteLineAsync("  text-decoration: line-through underline;");
            }
            else if (fontstyleSet.Contains(FontStyle.Underline))
            {
                await fs.WriteLineAsync("  text-decoration: underline;");
            }
            else if (fontstyleSet.Contains(FontStyle.StrikeThrough))
            {
                await fs.WriteLineAsync("  text-decoration: line-through;");
            }

            await fs.WriteLineAsync("}");
        }
    }
}
