using System.Numerics;

namespace GameServer.Systems.WeaponSim;

public class WeaponModeState
{
    public uint WeaponId;
    public string StateKey;

    public Vector3 LastSpreadDirection;
    public uint LastSpreadTime;
    public uint LastBurstTime;

    public float AccumulatedSpread;
    public float AccumulatedSpreadWhenReturnStarted;

    public float SpreadHeat;
    public float SpreadHeatWhenReturn;

    public uint LastRecoilUpdate;

    public ushort CurrentMovementFlags;
    public ushort PreviousMovementFlags;

    public float AgilityTarget = 1f;
    public float AgilityCurrent = 1f;
    public uint AgilityLastTime;

    public byte SpreadMovementState = 2;
    public byte OldSpreadMovementState;
    public uint SpreadStateLastTime;
    public float PreviousSpreadFloor;
}
