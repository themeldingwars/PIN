using System.Numerics;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.apt;
using GameServer.Systems.Aptitude;
using GameServer.Systems.Aptitude.Commands.Target;
using GameServer.Tests.Fakes;
using Xunit;

namespace GameServer.Tests;

public class TargetConeAECommandTests
{
    [Fact]
    public void Execute_TargetStraightAhead_IsAcquired()
    {
        var shard = new FakeShard();
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.UnitX);
        var target = CreateCharacter(shard, new Vector3(10, 0, 0));

        var context = new Context(shard, self);

        Assert.True(new TargetConeAECommand(Def()).Execute(context));
        Assert.Equal(new IAptitudeTarget[] { target }, context.Targets.ToArray());
    }

    [Fact]
    public void Execute_TargetBehindTheApex_IsNotAcquired()
    {
        var shard = new FakeShard();
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.UnitX);
        CreateCharacter(shard, new Vector3(-10, 0, 0));

        var context = new Context(shard, self);

        Assert.True(new TargetConeAECommand(Def()).Execute(context));
        Assert.Empty(context.Targets.ToArray());
    }

    [Fact]
    public void Execute_TargetOutsideTheOpeningAngle_IsNotAcquired()
    {
        var shard = new FakeShard();
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.UnitX);
        var inside = CreateCharacter(shard, new Vector3(10, 9, 0));
        CreateCharacter(shard, new Vector3(10, 11, 0));

        var context = new Context(shard, self);

        // 90 degrees of opening means the cone is as wide as it is long
        Assert.True(new TargetConeAECommand(Def(angle: 90f)).Execute(context));
        Assert.Equal(new IAptitudeTarget[] { inside }, context.Targets.ToArray());
    }

    [Fact]
    public void Execute_TargetBeyondTheRange_IsNotAcquired()
    {
        var shard = new FakeShard();
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.UnitX);
        CreateCharacter(shard, new Vector3(30, 0, 0));

        var context = new Context(shard, self);

        Assert.True(new TargetConeAECommand(Def(range: 20f)).Execute(context));
        Assert.Empty(context.Targets.ToArray());
    }

    [Fact]
    public void Execute_MinRadius_HitsPointBlankTargetsAtAnyAngle()
    {
        var shard = new FakeShard();
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.UnitX);
        var behind = CreateCharacter(shard, new Vector3(-1, 0, 0));

        var context = new Context(shard, self);
        Assert.True(new TargetConeAECommand(Def(minRadius: 0f)).Execute(context));
        Assert.Empty(context.Targets.ToArray());

        var bubbleContext = new Context(shard, self);
        Assert.True(new TargetConeAECommand(Def(minRadius: 2f)).Execute(bubbleContext));
        Assert.Equal(new IAptitudeTarget[] { behind }, bubbleContext.Targets.ToArray());
    }

    [Fact]
    public void Execute_MaxRadius_CapsHowWideTheConeGets()
    {
        var shard = new FakeShard();
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.UnitX);
        CreateCharacter(shard, new Vector3(18, 8, 0));

        var context = new Context(shard, self);
        Assert.True(new TargetConeAECommand(Def()).Execute(context));
        Assert.Single(context.Targets.ToArray());

        var cappedContext = new Context(shard, self);
        Assert.True(new TargetConeAECommand(Def(maxRadius: 5f)).Execute(cappedContext));
        Assert.Empty(cappedContext.Targets.ToArray());
    }

    [Fact]
    public void Execute_AimRadiusBias_WidensANarrowCone()
    {
        var shard = new FakeShard();
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.UnitX);
        CreateCharacter(shard, new Vector3(10, 1, 0));

        var context = new Context(shard, self);
        Assert.True(new TargetConeAECommand(Def(angle: 2f)).Execute(context));
        Assert.Empty(context.Targets.ToArray());

        var biasedContext = new Context(shard, self);
        Assert.True(new TargetConeAECommand(Def(angle: 2f, aimRadiusBias: 2f)).Execute(biasedContext));
        Assert.Single(biasedContext.Targets.ToArray());
    }

    [Fact]
    public void Execute_IgnorePastEndpoints_MeasuresTheRangeAlongTheAxis()
    {
        var shard = new FakeShard();
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.UnitX);

        // 20.6m away in a straight line, but only 19m along the cone axis
        CreateCharacter(shard, new Vector3(19, 8, 0));

        var sphericalCap = new Context(shard, self);
        Assert.True(new TargetConeAECommand(Def(range: 20f)).Execute(sphericalCap));
        Assert.Empty(sphericalCap.Targets.ToArray());

        var flatCap = new Context(shard, self);
        Assert.True(new TargetConeAECommand(Def(range: 20f, ignorePastEndpoints: 1)).Execute(flatCap));
        Assert.Single(flatCap.Targets.ToArray());
    }

    [Fact]
    public void Execute_SortByAngle_KeepsTheTargetsClosestToTheAxis()
    {
        var shard = new FakeShard();
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.UnitX);
        var wide = CreateCharacter(shard, new Vector3(10, 5, 0));
        var near = CreateCharacter(shard, new Vector3(5, 0.1f, 0));
        var centred = CreateCharacter(shard, new Vector3(12, 0, 0));

        var byAngle = new Context(shard, self);
        Assert.True(new TargetConeAECommand(Def(maxTargets: 2, sortByAngle: 1)).Execute(byAngle));
        Assert.Equal(new IAptitudeTarget[] { centred, near }, byAngle.Targets.ToArray());

        var byDistance = new Context(shard, self);
        Assert.True(new TargetConeAECommand(Def(maxTargets: 2)).Execute(byDistance));
        Assert.Equal(new IAptitudeTarget[] { near, wide }, byDistance.Targets.ToArray());
    }

    [Fact]
    public void Execute_BelowMinTargets_Fails()
    {
        var shard = new FakeShard();
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.UnitX);
        CreateCharacter(shard, new Vector3(10, 0, 0));

        var context = new Context(shard, self);

        Assert.False(new TargetConeAECommand(Def(minTargets: 2)).Execute(context));
        Assert.True(new TargetConeAECommand(Def(minTargets: 1)).Execute(new Context(shard, self)));
    }

    [Fact]
    public void Execute_NeverTargetsTheEntityRunningTheChain()
    {
        var shard = new FakeShard();
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.UnitX);
        var target = CreateCharacter(shard, new Vector3(5, 0, 0));

        var context = new Context(shard, self);

        Assert.True(new TargetConeAECommand(Def(minRadius: 5f)).Execute(context));
        Assert.Equal(new IAptitudeTarget[] { target }, context.Targets.ToArray());
    }

    [Fact]
    public void Execute_AppendsToTheCurrentTargetsAndKeepsThemAsFormer()
    {
        var shard = new FakeShard();
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.UnitX);
        var preexisting = CreateCharacter(shard, new Vector3(0, 100, 0));
        var acquired = CreateCharacter(shard, new Vector3(10, 0, 0));

        var context = new Context(shard, self);
        context.Targets.Push(preexisting);

        Assert.True(new TargetConeAECommand(Def()).Execute(context));

        Assert.Equal(new IAptitudeTarget[] { preexisting, acquired }, context.Targets.ToArray());
        Assert.Equal(new IAptitudeTarget[] { preexisting }, context.FormerTargets.ToArray());
    }

    [Fact]
    public void Execute_MaxTargets_CountsTheTargetsThatWereAlreadyThere()
    {
        var shard = new FakeShard();
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.UnitX);
        var preexisting = CreateCharacter(shard, new Vector3(0, 100, 0));
        var near = CreateCharacter(shard, new Vector3(5, 0, 0));
        CreateCharacter(shard, new Vector3(12, 0, 0));

        var context = new Context(shard, self);
        context.Targets.Push(preexisting);

        Assert.True(new TargetConeAECommand(Def(maxTargets: 2)).Execute(context));
        Assert.Equal(new IAptitudeTarget[] { preexisting, near }, context.Targets.ToArray());
    }

    [Fact]
    public void Execute_UseBodyOrient_PointsTheConeAlongTheBodyFacingInsteadOfTheAim()
    {
        var shard = new FakeShard();

        // A default orientation faces the model's local +Y, the aim points at +X
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.UnitX);
        var inFrontOfTheBody = CreateCharacter(shard, new Vector3(0, 10, 0));
        var inFrontOfTheAim = CreateCharacter(shard, new Vector3(10, 0, 0));

        var aiming = new Context(shard, self);
        Assert.True(new TargetConeAECommand(Def()).Execute(aiming));
        Assert.Equal(new IAptitudeTarget[] { inFrontOfTheAim }, aiming.Targets.ToArray());

        var facing = new Context(shard, self);
        Assert.True(new TargetConeAECommand(Def(useBodyOrient: 1)).Execute(facing));
        Assert.Equal(new IAptitudeTarget[] { inFrontOfTheBody }, facing.Targets.ToArray());
    }

    [Fact]
    public void Execute_WithoutAnAimDirection_FallsBackToTheBodyFacing()
    {
        var shard = new FakeShard();
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.Zero);
        var inFrontOfTheBody = CreateCharacter(shard, new Vector3(0, 10, 0));

        var context = new Context(shard, self);

        Assert.True(new TargetConeAECommand(Def()).Execute(context));
        Assert.Equal(new IAptitudeTarget[] { inFrontOfTheBody }, context.Targets.ToArray());
    }

    [Fact]
    public void Execute_UseInitPos_StartsTheConeAtTheInitiationPosition()
    {
        var shard = new FakeShard();
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.UnitX);
        var target = CreateCharacter(shard, new Vector3(110, 0, 0));

        var fromSelf = new Context(shard, self) { InitPosition = new Vector3(100, 0, 0) };
        Assert.True(new TargetConeAECommand(Def()).Execute(fromSelf));
        Assert.Empty(fromSelf.Targets.ToArray());

        var fromInit = new Context(shard, self) { InitPosition = new Vector3(100, 0, 0) };
        Assert.True(new TargetConeAECommand(Def(useInitPos: 1)).Execute(fromInit));
        Assert.Equal(new IAptitudeTarget[] { target }, fromInit.Targets.ToArray());
    }

    [Fact]
    public void Execute_WithoutRange_AcquiresNothing()
    {
        var shard = new FakeShard();
        var self = CreateCharacter(shard, Vector3.Zero, Vector3.UnitX);
        CreateCharacter(shard, new Vector3(1, 0, 0));

        var context = new Context(shard, self);

        Assert.True(new TargetConeAECommand(Def(range: 0f)).Execute(context));
        Assert.Empty(context.Targets.ToArray());
    }

    private static TargetConeAECommandDef Def(
        float range = 20f,
        float angle = 90f,
        float minRadius = 0f,
        float maxRadius = 0f,
        float aimRadiusBias = 0f,
        byte maxTargets = 0,
        byte minTargets = 0,
        byte sortByAngle = 0,
        byte useBodyOrient = 0,
        byte useInitPos = 0,
        byte ignorePastEndpoints = 0)
    {
        return new TargetConeAECommandDef
        {
            Id = 1,
            Range = range,
            Angle = angle,
            MinRadius = minRadius,
            MaxRadius = maxRadius,
            AimRadiusBias = aimRadiusBias,
            MaxTargets = maxTargets,
            MinTargets = minTargets,
            SortByAngle = sortByAngle,
            UseBodyOrient = useBodyOrient,
            UseInitPos = useInitPos,
            IgnorePastEndpoints = ignorePastEndpoints
        };
    }

    private static CharacterEntity CreateCharacter(FakeShard shard, Vector3 position, Vector3? aimDirection = null)
    {
        var character = new CharacterEntity(shard, shard.GetNextGuid(0))
        {
            Position = position,

            // An identity orientation makes the body face the model's local +Y, i.e. world +Y
            Orientation = Quaternion.Identity,
            AimDirection = aimDirection ?? Vector3.Zero
        };

        shard.Entities.Add(character.EntityId, character);

        return character;
    }
}
