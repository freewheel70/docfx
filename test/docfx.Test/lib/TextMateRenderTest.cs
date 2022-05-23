// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.Docs.Build;

public static class TextMateRenderTest
{
    [Theory]
    [InlineData("<pre><code class=\"lang-csharp\">var sb = new StringBuilder();</code></pre>", @"<pre><code class=""lang-csharp""><span class=""css-editor-background""><span class=""css-d6d198dd68bbff3e3fef3ae8aa7c4d96"">var</span><span class=""css-default-foreground-color""> </span><span class=""css-variable-and-parameter-name"">sb</span><span class=""css-default-foreground-color""> </span><span class=""css-8a4f8efb5683a91db560e0a499163ac1"">=</span><span class=""css-default-foreground-color""> </span><span class=""css-d6d198dd68bbff3e3fef3ae8aa7c4d96"">new</span><span class=""css-default-foreground-color""> </span><span class=""css-types-declaration-and-references"">StringBuilder</span><span class=""css-default-foreground-color"">(</span><span class=""css-default-foreground-color"">)</span><span class=""css-default-foreground-color"">;</span><br></span></code></pre>")]
    public static void ApplyThemeForCodeSnippetsTes(string html, string expected)
    {
        var actual = new CodeSnippetHelper(new ScopedErrorBuilder()).ApplyThemeForCodeSnippets(html);
        Assert.Equal(expected, actual);
    }
}
