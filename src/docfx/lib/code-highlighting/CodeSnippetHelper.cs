// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using HtmlAgilityPack;
using Microsoft.Docs.MarkdigExtensions;

namespace Microsoft.Docs.Build;

internal class CodeSnippetHelper
{
    private const string LanguageScopenameConfigPath = "./lib/code-highlighting/lang-scope.json";

    private readonly TextmateEngine _textmateEngine;

    private readonly List<LanguageScopeMappingModel> _langScopeMapping;

    private static readonly Dictionary<string, HashSet<string>> s_grammarAlias = new()
    {
        { "javascript", new HashSet<string>() { "actionscript", "brainscript" } },
        { "cpp", new HashSet<string>() { "arduino", "assembly", "cuda", "matlab" } },
        { "cshtml", new HashSet<string>() { "vbhtml" } },
        { "csharp", new HashSet<string>() { "powerapps" } },
    };

    public CodeSnippetHelper(ErrorBuilder errors)
    {
        _textmateEngine = new TextmateEngine(errors);
        _langScopeMapping = JsonUtility.Deserialize<List<LanguageScopeMappingModel>>(
            errors, File.ReadAllText(LanguageScopenameConfigPath), new FilePath(LanguageScopenameConfigPath));
    }

    public async Task<string?> ApplyThemeForCodeSnippetsAsync(string html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return "";
        }

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        var preNodes = htmlDocument.DocumentNode.Descendants("pre");

        foreach (var node in preNodes)
        {
            foreach (var code in node.ChildNodes)
            {
                if (code.Name == "code")
                {
                    var scopeName = GetScopeName(code.GetAttributeValue("class", "plaintext"));
                    var highlightLines = GetHighlightLines(code.GetAttributeValue("highlight-lines", string.Empty));
                    if (!string.IsNullOrEmpty(scopeName))
                    {
                        code.InnerHtml = await _textmateEngine.ApplyCSSClassForCodeSnippetAsync(code.InnerHtml, scopeName, highlightLines);
                    }
                }
            }
        }

        return htmlDocument.Text;
    }

    private string? GetScopeName(string? attr)
    {
        var lang = GetLanguage(attr);
        foreach (var mapping in _langScopeMapping)
        {
            if (lang?.Equals(mapping.Lang, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                return mapping.Scope;
            }
        }

        return string.Empty;
    }

    private static string? GetLanguage(string? attr)
    {
        if (string.IsNullOrEmpty(attr))
        {
            return null;
        }

        var lang = attr.StartsWith("lang-") ? attr[5..] : attr;

        foreach (var item in s_grammarAlias)
        {
            if (item.Value.Contains(lang, StringComparer.OrdinalIgnoreCase))
            {
                return item.Key;
            }
        }

        foreach (var item in HtmlCodeSnippetRenderer.LanguageAlias)
        {
            foreach (var alias in item.Value)
            {
                if (lang.Equals(alias, StringComparison.OrdinalIgnoreCase) || lang.Equals(item.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return item.Key;
                }
            }
        }
        return null;
    }

    private static HashSet<int> GetHighlightLines(string highlightLines)
    {
        var result = new HashSet<int>();

        if (string.IsNullOrEmpty(highlightLines))
        {
            return result;
        }

        var ranges = highlightLines.Split(",");
        foreach (var range in ranges)
        {
            var indexes = range.Split("-");
            if (indexes.Length == 1)
            {
                result.Add(int.Parse(indexes[0]));
            }
            else
            {
                for (var i = int.Parse(indexes[0]); i <= int.Parse(indexes[1]); ++i)
                {
                    result.Add(i);
                }
            }
        }

        return result;
    }
}
