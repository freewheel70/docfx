// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Text;
using System.Web;
using HtmlReaderWriter;
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

    public string ApplyThemeForCodeSnippets(string html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return "";
        }
        var sb = new StringBuilder();
        var reader = new HtmlReader(html);
        var inCodeBlock = false;
        var scopeName = string.Empty;
        var highlightLines = new HashSet<int>();
        while (reader.Read(out var token))
        {
            if (token.Type == HtmlTokenType.StartTag && token.Name.ToString().Equals("code", StringComparison.OrdinalIgnoreCase))
            {
                inCodeBlock = true;
                foreach (var attr in token.Attributes.ToArray())
                {
                    if (attr.Name.ToString().Equals("class", StringComparison.OrdinalIgnoreCase))
                    {
                        scopeName = GetScopeName(attr.Value.ToString());
                    }

                    if (attr.Name.ToString().Equals("highlight-lines", StringComparison.OrdinalIgnoreCase))
                    {
                        highlightLines = GetHighlightLines(attr.Value.ToString());
                    }
                }
                sb.Append(token.RawText);
            }
            else if (inCodeBlock)
            {
                if (token.Type == HtmlTokenType.Text && !string.IsNullOrEmpty(scopeName))
                {
                    sb.Append(_textmateEngine.ApplyCSSClassForCodeSnippetAsync(
                        HttpUtility.HtmlDecode(token.RawText.ToString()), scopeName, highlightLines).Result);
                }
                else
                {
                    sb.Append(token.RawText);
                    inCodeBlock = false;
                    scopeName = string.Empty;
                    highlightLines = new HashSet<int>();
                }
            }
            else
            {
                sb.Append(token.RawText);
            }
        }

        return sb.ToString();
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
