// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Docs.Build;

internal class TextmateEngine
{
    private const string ScriptPath = "./lib/code-highlighting/script/textmate.js";
    private static readonly FilePath s_themeFilePath = new("./lib/code-highlighting/theme/dark_plus.json");
    private static readonly INodeJSService s_nodeService =
        new ServiceCollection()
        .AddNodeJS()
        .Configure<NodeJSProcessOptions>(options => options.NodeAndV8Options = "--inspect-brk")
        .Configure<OutOfProcessNodeJSServiceOptions>(options => options.TimeoutMS = -1)
        .BuildServiceProvider()
        .GetRequiredService<INodeJSService>();

    private readonly ThemeModel _themeModel;

    public TextmateEngine(ErrorBuilder errors)
    {
        _themeModel = JsonUtility.Deserialize<ThemeModel>(errors, File.ReadAllText(s_themeFilePath.Path), s_themeFilePath);
    }

    public async Task<string> ApplyCSSClassForCodeSnippetAsync(
        string code,
        string scopeName,
        HashSet<int> highlightLines)
    {
        var curr = Directory.GetCurrentDirectory();
        var tokenWithScopes = await s_nodeService.InvokeFromFileAsync<TextmateToken[]>("./lib/code-highlighting/script/textmate.js", args: new string[] { code, scopeName });
        var contentStringBuilder = new StringBuilder();

        if (tokenWithScopes is null)
        {
            return contentStringBuilder.ToString();
        }

        contentStringBuilder.Append($@"<span class=""css-editor-background"">");

        for (var i = 0; i < tokenWithScopes.Length; i++)
        {
            var tokens = tokenWithScopes[i];
            var cur = 0;

            if (highlightLines.Contains(i + 1))
            {
                contentStringBuilder.Append(@"<span class=""css-editor-selectionbackground"">");
            }

            if (tokens.LineTokens is null || tokens.LineTokens.Length == 0)
            {
                continue;
            }

            foreach (var token in tokens.LineTokens)
            {
                var start = token.Index[0];
                var end = token.Index[1];
                if (cur < start)
                {
                    contentStringBuilder.Append(new StringBuilder().Insert(0, "&nbsp;", start - cur));
                }

                var colorRule = ThemeMatching.FindMatchingRule(_themeModel, token.Scopes!, true);
                var colorClass = colorRule?.TokenRule.GetCSSClassName() ?? ".css-default-foreground-color";

                var style = new StringBuilder(colorClass);

                var fontstyleRule = ThemeMatching.FindMatchingRule(_themeModel, token.Scopes!, false);
                if (fontstyleRule is not null && fontstyleRule.TokenRule.Settings.FontstyleSet.Count != 0)
                {
                    style.Append($" {fontstyleRule.TokenRule.GetFontstyleClassName()}");
                }

                contentStringBuilder.Append(
                    $@"<span class=""{style.ToString()}"">{token.Content.Replace(" ", "&nbsp;").Replace("<", "&lt;").Replace(">", "&gt;")}</span>");
                cur = end;
            }
            if (highlightLines.Contains(i + 1))
            {
                contentStringBuilder.Append("</span>");
            }
            contentStringBuilder.Append("<br>");
        }

        contentStringBuilder.Append("</span>");

        return contentStringBuilder.ToString();
    }
}
