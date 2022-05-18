// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Docs.Build;

internal class LineToken
{
    public int[] Index { get; init; } = new int[2];

    public string[]? Scopes { get; init; }

    public string Content { get; init; } = string.Empty;
}
