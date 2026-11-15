using System;
using System.Numerics;
using AeroMessages.GSS.Character.Controller;

namespace GameServer.Entities.Character;

public class CharacterJetEnergy
{
    public const ushort EncodedFull = 25500;

    private const float _attributeCostThreshold = 1f;
    private const float _attributeClimbSpeedThreshold = 0.1f;
    private const uint _minRechargeDelayMs = 75;
    private const float _usedEnergyEpsilon = 0.001f;
    private const ushort _maxStepMs = 0x8000;

    private static JetEnergyTuning _tuning;

    private bool _hasTime;
    private ushort _shortTime;
    private uint _elapsedMs;
    private uint _lastUseMs;
    private bool _hasUsed;
    private ushort _movestate;

    public float Energy { get; private set; }

    private static JetEnergyTuning Tuning => _tuning ??= JetEnergyTuning.FromStaticDb();

    public void Reset(float energy, ushort movestate)
    {
        Energy = Math.Max(0f, energy);
        _hasTime = false;
        _elapsedMs = 0;
        _lastUseMs = 0;
        _hasUsed = false;
        _movestate = movestate;
    }

    public void ClampToMax(float max)
    {
        Energy = Math.Clamp(Energy, 0f, Math.Max(0f, max));
    }

    public ushort Encode(float max)
    {
        if (max <= 0f)
        {
            return EncodedFull;
        }

        return (ushort)Math.Clamp(MathF.Round(Energy / max * EncodedFull), 0f, EncodedFull);
    }

    public void Simulate(ushort shortTime, ushort movestate, Vector3 velocity, EnergyParamsData energyParams, JetEnergyCosts costs)
    {
        if (!_hasTime)
        {
            _hasTime = true;
            _shortTime = shortTime;
            _movestate = movestate;
            return;
        }

        var stepMs = unchecked((ushort)(shortTime - _shortTime));
        if (stepMs == 0 || stepMs >= _maxStepMs)
        {
            return;
        }

        _shortTime = shortTime;
        _elapsedMs += stepMs;
        var dt = stepMs * 0.001f;

        var before = Energy;
        Spend(movestate, velocity, costs, dt);
        _movestate = movestate;

        if (before - Energy > _usedEnergyEpsilon)
        {
            _lastUseMs = _elapsedMs;
            _hasUsed = true;
        }

        var delay = Math.Max(_minRechargeDelayMs, energyParams.Delay);
        if (Energy < energyParams.Max && (!_hasUsed || _elapsedMs - _lastUseMs > delay))
        {
            Energy = Math.Min(energyParams.Max, Energy + (energyParams.Recharge * dt));
        }
    }

    private static Movestate GetMovestate(ushort movestate) => (Movestate)((movestate & 0xF000) >> 8);

    private static MovementFlags GetMovementFlags(ushort movestate) => (MovementFlags)(movestate & 0x00FF);

    private static bool IsJetpack(Movestate state) => state is Movestate.Jetpack or Movestate.JetpackSprint;

    private void Spend(ushort movestate, Vector3 velocity, JetEnergyCosts costs, float dt)
    {
        var state = GetMovestate(movestate);
        var previousState = GetMovestate(_movestate);
        float usage;

        if (IsJetpack(state))
        {
            usage = costs.JetEnergyCost > _attributeCostThreshold ? costs.JetEnergyCost : Tuning.EnergyUsage;
            if (state == Movestate.JetpackSprint)
            {
                usage = costs.JetSprintCost > _attributeCostThreshold ? costs.JetSprintCost : Tuning.EnergyUsageSprinting;
            }

            var climbSpeed = costs.JetClimbSpeed > _attributeClimbSpeedThreshold ? costs.JetClimbSpeed : Tuning.MaxClimbSpeed;
            if (climbSpeed < velocity.Z)
            {
                usage *= Tuning.TooFastEnergyMult;
            }

            var enteredSprint = state == Movestate.JetpackSprint && !IsJetpack(previousState);
            if (enteredSprint && Energy > Tuning.SprintDashEnergy)
            {
                Energy = Math.Max(0f, Energy - Tuning.SprintDashEnergy);
            }
        }
        else if (state == Movestate.GliderThrusters)
        {
            return;
        }
        else if (GetMovementFlags(movestate).HasFlag(MovementFlags.Sprint))
        {
            usage = costs.SprintEnergyCost;
        }
        else
        {
            return;
        }

        Energy = Math.Max(0f, Energy - (usage * dt));
    }
}
