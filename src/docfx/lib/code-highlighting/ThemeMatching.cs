// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Docs.Build;

internal class ThemeMatching
{
    public static ThemeRule? FindMatchingRule(ThemeModel theme, string[] scopes, bool onlyColorRules = false)
    {
        for (var i = scopes.Length - 1; i >= 0; i--)
        {
            var parentScopes = scopes[0..i];
            var scope = scopes[i];
            var result = FindMatchingRuleCore(theme, scope, parentScopes, onlyColorRules);
            if (result is not null)
            {
                return result;
            }
        }
        return null;
    }

    public static ThemeRule? FindMatchingRuleCore(ThemeModel theme, string scope, string[] parentScopes, bool onlyColorRules = false)
    {
        ThemeRule? result = null;
        for (var i = theme.TokenColors.Count - 1; i >= 0; i--)
        {
            var rule = theme.TokenColors[i];

            if (onlyColorRules && string.IsNullOrEmpty(rule.Settings.Foreground))
            {
                continue;
            }

            var selectors = rule.Scope;

            for (var j = 0; j < selectors.Length; j++)
            {
                var rawSelector = selectors[j];
                var themeRule = new ThemeRule(rawSelector, rule);
                if (themeRule.Matches(scope, parentScopes))
                {
                    if (themeRule.IsMoreSpecific(result))
                    {
                        result = themeRule;
                    }
                }
            }
        }

        return result;
    }
}
