using GameServer.Entities.Character;
using GameServer.StaticDB.Records.apt;
using GameServer.Systems.Aptitude;
using GameServer.Systems.Aptitude.Commands.Register;
using GameServer.Tests.Fakes;
using Xunit;

namespace GameServer.Tests;

/// <summary>
///     <c>apt::LoadRegisterFromNamedVarCommandDef</c> (type 240) loads the value of a named variable into the
///     aptitude register. GameServer has no named-variable store yet, so the row's <c>undecl_value</c> fallback
///     is the value a row receives — the shared glider pad launch ability (chain 1001671, row 1001663) depends
///     on that fallback of 1.0: the register comparisons in the launch effects that follow select the pad's
///     effect level through it, and when the register was never loaded an unset register and a real level were
///     indistinguishable. Loading the fallback keeps the register pipeline working the way the row describes it.
/// </summary>
public class LoadRegisterFromNamedVarCommandTests
{
    [Fact]
    public void LoadsTheUndeclaredVariableFallbackIntoTheRegister()
    {
        var shard = new FakeShard();
        var character = new CharacterEntity(shard, shard.GetNextGuid(0));
        shard.Entities.Add(character.EntityId, character);

        var context = new Context(shard, character);

        var command = new LoadRegisterFromNamedVarCommand(new LoadRegisterFromNamedVarCommandDef
        {
            Id = 1001663,
            UndeclValue = 1.0f,
            Regop = 0, // ASSIGN: the register takes the fallback value.
        });

        Assert.True(command.Execute(context));
        Assert.Equal(1.0f, context.Register);
    }

    [Fact]
    public void CombinesTheFallbackWithTheCurrentRegisterThroughTheRegop()
    {
        var shard = new FakeShard();
        var character = new CharacterEntity(shard, shard.GetNextGuid(0));
        shard.Entities.Add(character.EntityId, character);

        var context = new Context(shard, character);
        context.Register = 3.0f;

        var command = new LoadRegisterFromNamedVarCommand(new LoadRegisterFromNamedVarCommandDef
        {
            Id = 1001663,
            UndeclValue = 2.0f,
            Regop = 1, // ADD.
        });

        Assert.True(command.Execute(context));
        Assert.Equal(5.0f, context.Register);
    }

    [Fact]
    public void DoesNotFailAChainWhenTheRegisterWasNeverSet()
    {
        var shard = new FakeShard();
        var character = new CharacterEntity(shard, shard.GetNextGuid(0));
        shard.Entities.Add(character.EntityId, character);

        // The register defaults to NaN; loading into it must not poison the rest of the chain.
        var context = new Context(shard, character);

        var command = new LoadRegisterFromNamedVarCommand(new LoadRegisterFromNamedVarCommandDef
        {
            Id = 1001663,
            UndeclValue = 1.0f,
            Regop = 1, // ADD onto NaN behaves like a plain assignment.
        });

        Assert.True(command.Execute(context));
        Assert.Equal(1.0f, context.Register);
    }
}
