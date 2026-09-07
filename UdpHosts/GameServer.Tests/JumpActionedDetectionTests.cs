using GameServer.Systems.MovementRelay;
using Xunit;

namespace GameServer.Tests;

/// <summary>
///     The server tells a client that its character jumped (<c>JumpActioned</c>, which is what commits the launch
///     of a boost panel and the wings deployment that follows it) by watching the client's "time since last jump"
///     counter drop back to zero. The counter is a 16 bit millisecond value that runs past its positive range
///     while a player is in the air, so the comparison has to be made in modular terms: comparing the signed
///     values reported a jump at every wrap, about 33 seconds of air time, which is exactly what a boost panel
///     launch that never turned into a glide produces.
/// </summary>
public class JumpActionedDetectionTests
{
    [Theory]
    [InlineData((short)1000, (short)1020, false)]
    [InlineData((short)0, (short)32767, false)]
    [InlineData((short)32767, (short)(-32768), false)] // the counter wrapping into the negative range
    [InlineData((short)(-1), (short)0, false)] // 65535 ms of air time running over into zero again
    [InlineData((short)31761, (short)0, true)] // a jump after 31.7 s in the air
    [InlineData((short)5000, (short)0, true)]
    [InlineData((short)5, (short)0, true)]
    [InlineData((short)1000, (short)20, true)]
    public void IsJumpCounterReset(short previous, short current, bool expected)
    {
        Assert.Equal(expected, MovementRelay.IsJumpCounterReset(previous, current));
    }

    [Fact]
    public void IsJumpCounterReset_IgnoresTheCounterStandingStill()
    {
        Assert.False(MovementRelay.IsJumpCounterReset(12345, 12345));
    }
}
