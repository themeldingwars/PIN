using System.Linq;
using GameServer.StaticDB.Records.customdata;
using GameServer.StaticDB.Records.customdata.Encounters;
using Shared.Content.Zones;
using Shared.Content.Zones.Encounters;

namespace GameServer.Systems.Content;

/// <summary>Turns content entries into the records the entities and encounters were built on.</summary>
public static class ContentRecords
{
    public static Outpost ToRecord(this ZoneOutpost outpost, uint zoneId) => new()
    {
        Id = outpost.Id,
        ZoneId = zoneId,
        OutpostName = outpost.NameTextId,
        Position = outpost.Position,
        SpawnPoints = [.. outpost.SpawnPoints.Select(p => new SpawnPoint { Position = p.Position, Orientation = p.Orientation, AimDirection = p.AimDirection })],
        LevelBandId = outpost.LevelBandId,
        SinUnlockIndex = outpost.SinUnlockIndex,
        TeleportCost = outpost.TeleportCost,
        FactionId = outpost.FactionId,
        OutpostType = outpost.OutpostType,
        PossibleBuffsId = outpost.PossibleBuffsId,
        Radius = outpost.Radius,
        MarkerType = outpost.MarkerType,
    };

    public static MeldingRepulsorDef ToRecord(this ZoneMeldingRepulsor repulsor, uint zoneId) => new()
    {
        ZoneId = zoneId,
        PerimiterSetName = repulsor.PerimeterSet,
        Repulsor = repulsor.Repulsor.ToRecord(zoneId),
        Terminal = repulsor.Terminal.ToRecord(zoneId),
        MeldingPosition = new StaticDB.Records.customdata.Encounters.MeldingPosition
        {
            ControlPointIndex = repulsor.MeldingPosition.ControlPointIndex,
            Position = repulsor.MeldingPosition.Position,
        },
    };

    public static LgvRaceDef ToRecord(this ZoneLgvRace race, uint zoneId) => new()
    {
        ZoneId = zoneId,
        LeaderboardId = race.LeaderboardId,
        Label = race.LabelTextId,
        StartDialog = race.StartDialog,
        Start = race.Start.ToRecord(),
        Finish = race.Finish.ToRecord(),
        Terminal = race.Terminal.ToRecord(),
        TimeLimitMs = race.TimeLimitMs,
        BonusTimeLimitMs = race.BonusTimeLimitMs,
    };

    private static Deployable ToRecord(this PlacedObject placed, uint zoneId) => new()
    {
        ZoneId = zoneId,
        Type = placed.Type,
        Position = placed.Position,
        Orientation = placed.Orientation,
    };

    private static SpawnPose ToRecord(this Pose pose) => new() { Position = pose.Position, Orientation = pose.Orientation };
}
