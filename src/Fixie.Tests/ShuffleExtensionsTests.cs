namespace Fixie.Tests;

using System;
using Assertions;

public class ShuffleExtensionsTests
{
    const int Seed = 42;

    public void ShouldProvideCollectionItemsInRandomOrder()
    {
        new[] {1, 2, 3, 4, 5}
            .Shuffle(new Random(Seed))
            .ShouldBe(3, 2, 5, 1, 4);

        "Hello World"
            .Shuffle(new Random(Seed))
            .ShouldBe('d', ' ', 'H', 'l', 'l', 'W', 'r', 'o', 'l', 'e', 'o');
    }
}