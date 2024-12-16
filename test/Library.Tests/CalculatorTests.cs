// Copyright (c) COMPANY-PLACEHOLDER. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library;
using TUnit;

public class CalculatorTests
{
    public CalculatorTests()
    {
    }

    [Test]
    public async Task AddOrSubtract()
    {
        // This tests aggregation of code coverage across test runs.
#if NET8_0_OR_GREATER
        await Assert.That(Calculator.Add(1, 2)).IsEqualTo(3);
#else
        await Assert.That(Calculator.Subtract(1, 2)).IsEqualTo(-1);
#endif
    }
}
