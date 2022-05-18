// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Docs.Build;
using Newtonsoft.Json;

namespace Microsoft.Docs.CSS;

public static class Program
{
    public static async Task Main()
    {
        var filePath = "C:/workspace/work/docfx/src/docfx/lib/code-highlighting/theme/dark_plus.json";
        var model = JsonConvert.DeserializeObject<ThemeModel>(File.ReadAllText(filePath));
        await CSSGenerator.GenerateCSSFile(model!);
    }
}
