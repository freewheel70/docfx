// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using HtmlAgilityPack;

namespace Microsoft.Docs.Build;

internal class CodeSnippetHelper
{
    public static List<(string code, string type)> GetCodeSnippets(string html)
    {
        var results = new List<(string code, string type)>();

        if (string.IsNullOrEmpty(html))
        {
            return results;
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
                    results.Add((code: code.InnerHtml, type: code.GetAttributeValue("class", "plaintext")));
                }
            }
        }

        return results;
    }
}
