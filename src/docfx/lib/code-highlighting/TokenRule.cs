// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Docs.Build;

internal class TokenRule
{
    public string? Name { get; init; }

    [JsonConverter(typeof(OneOrManyConverter))]
    public string[] Scope { get; init; } = Array.Empty<string>();

    public TokenColorizationSetting Settings { get; init; } = new TokenColorizationSetting();

    public string GetCSSClassName()
    {
        if (string.IsNullOrEmpty(Name))
        {
            return $"css-{HashUtility.GetSha256HashShort(string.Join(",", Scope))}";
        }

        return $"css-{NormalizeCSSClassName(Name)}";
    }

    public string GetFontstyleClassName()
        => $"{GetCSSClassName()}-fontstyle";

    private static string NormalizeCSSClassName(string s)
    {
        var sb = new StringBuilder();
        foreach (var c in s.ToCharArray())
        {
            var ch = char.ToLowerInvariant(c);
            if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
            {
                sb.Append(ch);
            }
            else
            {
                if (sb.Length > 0 && sb[^1] != '-')
                {
                    sb.Append('-');
                }
            }
        }
        return sb.ToString();
    }
}
