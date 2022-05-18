// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Docs.Build;

internal class TokenColorizationSetting
{
    public string? Foreground { get; init; }

    public string? Background { get; init; }

    public string? Fontstyle { get; init; }

    public HashSet<FontStyle> FontstyleSet { get; init; } = new();

    public void SetFontStyles()
    {
        if (string.IsNullOrEmpty(Fontstyle))
        {
            return;
        }

        foreach (var fontstyle in Fontstyle.Split())
        {
            if (Enum.TryParse(fontstyle.Trim(), true, out FontStyle style))
            {
                FontstyleSet.Add(style);
            }
        }
    }
}
