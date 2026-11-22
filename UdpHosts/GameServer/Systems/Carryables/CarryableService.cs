using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using GameServer.Entities;
using GameServer.Entities.Carryable;
using GameServer.Entities.Character;
using GameServer.StaticDB;
using GameServer.Systems.Aptitude;
using GameServer.Systems.CharacterLifecycle;
using GameServer.Systems.SystemEvents;
using Serilog;
using CarriedObject = AeroMessages.GSS.Character.CarryableObject;

namespace GameServer.Systems.Carryables;

public sealed class CarryableService
{
    public const float PickupTolerance = 2.5f;

    public const float CarryHeight = 1.2f;

    private readonly IShard _shard;
    private readonly ILogger _logger = Log.ForContext<CarryableService>();
    private readonly Dictionary<ulong, CarryableEntity> _carried = [];

    public CarryableService(IShard shard, EventBus eventBus)
    {
        _shard = shard;
        eventBus.Subscribe<CharacterDiedEvent>(e => Drop(e.Target));
        eventBus.Subscribe<CharacterEnteredBleedoutEvent>(e => Drop(e.Target));
    }

    public CarryableEntity CarriedBy(CharacterEntity character)
    {
        return character != null && _carried.TryGetValue(character.EntityId, out var carryable) ? carryable : null;
    }

    public bool TryPickup(CharacterEntity character, CarryableEntity carryable, bool checkDistance = true)
    {
        if (character == null || carryable == null || carryable.Carrier != null || _carried.ContainsKey(character.EntityId))
        {
            return false;
        }

        if (!_shard.Entities.ContainsKey(carryable.EntityId) || _shard.CharacterLifecycle.GetState(character) != CharacterLifecycleState.Living)
        {
            return false;
        }

        var info = SDBInterface.GetCarryableObject(carryable.Type);
        if (info == null || info.AllowFriendlyPickup == 0)
        {
            return false;
        }

        if (checkDistance && Vector3.Distance(character.Position, carryable.Position) > info.PickupRadius + PickupTolerance)
        {
            _logger.Debug("{Character} is too far from {Carryable} to pick it up", character, carryable);
            return false;
        }

        _carried[character.EntityId] = carryable;
        carryable.SetCarrier(character);
        carryable.CarryableObject_ObserverView.ForcedMovementProp = null;
        SetCarriedView(character, new CarriedObject { Target = carryable.AeroEntityId, TypeId = carryable.Type });
        RemoveEffect(carryable, info.CreatedStatusEffect);
        ApplyEffect(character, info.PickedUpStatusEffectFriendly);
        _logger.Information("{Character} picked up {Carryable}", character, carryable);
        return true;
    }

    public void Drop(CharacterEntity character)
    {
        if (character == null || !_carried.TryGetValue(character.EntityId, out var carryable))
        {
            return;
        }

        Release(character, carryable);
        if (_shard.Entities.ContainsKey(carryable.EntityId))
        {
            var info = SDBInterface.GetCarryableObject(carryable.Type);
            carryable.SetPosition(character.Position);
            ApplyEffect(carryable, info?.CreatedStatusEffect ?? 0);
            ApplyEffect(carryable, info?.DroppedStatusEffectFriendly ?? 0);
            _logger.Information("{Character} dropped {Carryable}", character, carryable);
        }
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        foreach (var (characterId, carryable) in _carried.ToList())
        {
            if (!_shard.Entities.TryGetValue(characterId, out var entity) || entity is not CharacterEntity character)
            {
                _carried.Remove(characterId);
                carryable.ClearCarrier();
                continue;
            }

            if (!_shard.Entities.ContainsKey(carryable.EntityId))
            {
                Release(character, carryable);
                continue;
            }

            carryable.SetPosition(character.Position + new Vector3(0f, 0f, CarryHeight));
        }
    }

    private void Release(CharacterEntity character, CarryableEntity carryable)
    {
        _carried.Remove(character.EntityId);
        carryable.ClearCarrier();
        SetCarriedView(character, null);
        var info = SDBInterface.GetCarryableObject(carryable.Type);
        RemoveEffect(character, info?.PickedUpStatusEffectFriendly ?? 0);
    }

    private void SetCarriedView(CharacterEntity character, CarriedObject? carried)
    {
        if (character.Character_ObserverView != null)
        {
            character.Character_ObserverView.CarryableObjects_0Prop = carried;
        }

        if (character.Character_BaseController != null)
        {
            character.Character_BaseController.CarryableObjects_0Prop = carried;
        }
    }

    private void ApplyEffect(BaseAptitudeEntity target, uint effectId)
    {
        if (effectId != 0)
        {
            _shard.Abilities.DoApplyEffect(effectId, target, new Context(_shard, target) { InitTime = _shard.CurrentTime, CurrentTime = _shard.CurrentTime });
        }
    }

    private void RemoveEffect(BaseAptitudeEntity target, uint effectId)
    {
        if (effectId != 0)
        {
            _shard.Abilities.DoRemoveEffect(target, effectId, _shard.CurrentTime);
        }
    }
}
