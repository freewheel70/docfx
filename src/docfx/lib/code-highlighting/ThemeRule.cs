// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Docs.Build;

internal class ThemeRule
{
    private readonly string _rawSelector;

    public TokenRule TokenRule { get; init; }

    public string Scope { get; init; } = string.Empty;

    public string[] ParentScopes { get; init; } = Array.Empty<string>();

    public ThemeRule(string rawSelector, TokenRule tokenRule)
    {
        _rawSelector = rawSelector;
        TokenRule = tokenRule;
        var rawSelectorPieces = _rawSelector.Split();
        Scope = rawSelectorPieces[^1];
        ParentScopes = rawSelectorPieces[0..(rawSelectorPieces.Length - 1)];
    }

    public bool Matches(string scope, string[] parentScopes)
    {
        return MatchesCore(Scope, ParentScopes, scope, parentScopes);
    }

    public bool IsMoreSpecific(ThemeRule? other)
    {
        return Compare(this, other) > 0;
    }

    private static int Compare(ThemeRule? a, ThemeRule? b)
    {
        if (a is null && b is null)
        {
            return 0;
        }

        if (a is null)
        {
            // b > a
            return -1;
        }

        if (b is null)
        {
            // a > b
            return 1;
        }

        if (a.Scope.Length != b.Scope.Length)
        {
            return a.Scope.Length - b.Scope.Length;
        }

        var aParentScopesLength = a.ParentScopes.Length;
        var bParentScopesLength = b.ParentScopes.Length;

        if (aParentScopesLength != bParentScopesLength)
        {
            return aParentScopesLength - bParentScopesLength;
        }

        for (var i = 0; i < aParentScopesLength; i++)
        {
            var aLen = a.ParentScopes[i].Length;
            var bLen = b.ParentScopes[i].Length;
            if (aLen != bLen)
            {
                return aLen - bLen;
            }
        }

        return 0;
    }

    private static bool MatchesCore(string selectorScope, string[] selectorParentScopes, string scope, string[] parentScopes)
    {
        if (!MatchesOne(selectorScope, scope))
        {
            return false;
        }

        var selectorParentIndex = selectorParentScopes.Length - 1;
        var parentIndex = parentScopes.Length - 1;

        while (selectorParentIndex >= 0 && parentIndex >= 0)
        {
            if (MatchesOne(selectorParentScopes[selectorParentIndex], parentScopes[parentIndex]))
            {
                selectorParentIndex--;
            }
            parentIndex--;
        }

        if (selectorParentIndex == -1)
        {
            return true;
        }
        return false;
    }

    private static bool MatchesOne(string selectorScope, string scope)
    {
        var selectorPrefix = $"{selectorScope}.";
        if (selectorScope.Equals(scope, StringComparison.Ordinal) ||
            (selectorPrefix.Length <= scope.Length && scope[0..selectorPrefix.Length].Equals(selectorPrefix, StringComparison.Ordinal)))
        {
            return true;
        }
        return false;
    }
}
