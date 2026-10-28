using System;

namespace Shared.Content;

/// <summary>Both ends included. Written as a single number when both are the same, e.g. <c>3</c> or <c>[3, 5]</c>.</summary>
public readonly record struct NumberRange(uint Min, uint Max)
{
    public uint Pick(Random random) => (uint)random.NextInt64(Min, (long)Max + 1);
}
