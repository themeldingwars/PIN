using System;
using System.Numerics;
using GameServer.Systems.Ai;
using Xunit;

namespace GameServer.Tests;

public class AiVectorsTests
{
    [Fact]
    public void HorizontalDistance_IgnoresZ()
    {
        float distance = AiVectors.HorizontalDistance(new Vector3(0f, 0f, -50f), new Vector3(3f, 4f, 900f));

        Assert.Equal(5f, distance, 3);
    }

    [Fact]
    public void HorizontalDistance_OfSamePoint_IsZero()
    {
        var point = new Vector3(12.5f, -3f, 7f);

        Assert.Equal(0f, AiVectors.HorizontalDistance(point, point), 5);
    }

    [Theory]
    [InlineData(1f, 0f)]
    [InlineData(-1f, 0f)]
    [InlineData(0f, 1f)]
    [InlineData(0f, -1f)]
    [InlineData(0.7071068f, 0.7071068f)]
    [InlineData(-0.6f, 0.8f)]
    public void OrientationFacing_PointsLocalPlusYAlongTheRequestedDirection(float x, float y)
    {
        // CharacterEntity resolves its facing as
        // QuaternionEx.Transform(new Vector3(0, 1, 0), QuaternionEx.Inverse(Orientation))
        // (local +Y is forward, local +Z is up), so feed the produced orientation
        // back through exactly that formula.
        var forward = new Vector3(x, y, 0f);

        var orientation = AiVectors.OrientationFacing(forward);
        var resolved = Vector3.Transform(new Vector3(0f, 1f, 0f), Quaternion.Conjugate(orientation));

        Assert.Equal(Vector3.Normalize(forward).X, resolved.X, 3);
        Assert.Equal(Vector3.Normalize(forward).Y, resolved.Y, 3);
        Assert.Equal(0f, resolved.Z, 3);
    }

    [Theory]
    [InlineData(1f, 0f)]
    [InlineData(-1f, 0f)]
    [InlineData(0f, 1f)]
    [InlineData(0f, -1f)]
    [InlineData(0.7071068f, 0.7071068f)]
    [InlineData(-0.6f, 0.8f)]
    public void OrientationFacing_KeepsTheCharacterUpright(float x, float y)
    {
        // The orientation must be a yaw-only rotation about world +Z so the model's
        // up axis (local +Z) keeps pointing at world +Z (up), otherwise the mob is
        // rendered lying on its side or face up.
        var orientation = AiVectors.OrientationFacing(new Vector3(x, y, 0f));

        var up = Vector3.Transform(new Vector3(0f, 0f, 1f), Quaternion.Conjugate(orientation));

        Assert.Equal(0f, up.X, 3);
        Assert.Equal(0f, up.Y, 3);
        Assert.Equal(1f, up.Z, 3);
    }

    [Fact]
    public void OrientationFacing_IgnoresVerticalComponent()
    {
        var withPitch = AiVectors.OrientationFacing(new Vector3(0f, 1f, 5f));
        var flat = AiVectors.OrientationFacing(new Vector3(0f, 1f, 0f));

        Assert.Equal(flat.W, withPitch.W, 5);
        Assert.Equal(flat.X, withPitch.X, 5);
        Assert.Equal(flat.Y, withPitch.Y, 5);
        Assert.Equal(flat.Z, withPitch.Z, 5);
    }

    [Fact]
    public void OrientationFacing_OfDegenerateDirection_IsIdentity()
    {
        Assert.Equal(Quaternion.Identity, AiVectors.OrientationFacing(Vector3.Zero));
        Assert.Equal(Quaternion.Identity, AiVectors.OrientationFacing(new Vector3(0f, 0f, 10f)));
    }
}
