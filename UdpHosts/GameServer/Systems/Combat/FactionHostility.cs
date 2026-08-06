using System.Collections.Generic;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.dbcharacter;
using Serilog;

namespace GameServer.Systems.Combat;

public class FactionHostility
{
    private readonly Dictionary<(uint, uint), bool> _factionFriendlyDict;
    private readonly Dictionary<(uint, uint), bool> _factionHostileDict;
    private readonly ILogger _logger;

    public FactionHostility()
    {
        _logger = Log.ForContext<FactionHostility>();
        _factionFriendlyDict = [];
        _factionHostileDict = [];
        LoadFromSDB();
    }

    public void LoadFromSDB()
    {
        var factions = SDBInterface.GetFactions();
        var relations = SDBInterface.GetFactionRelations();

        foreach (var relation in relations)
        {
            if (relation.FactionA == 0)
            {
                foreach (var primaryFaction in factions)
                {
                    if (relation.FactionB == 0)
                    {
                        foreach (var secondaryFaction in factions)
                        {
                            ProcessFactionRelation(primaryFaction, secondaryFaction, relation);
                        }
                    }
                    else
                    {
                        var secondaryFaction = factions[(int)relation.FactionB - 1];
                        ProcessFactionRelation(primaryFaction, secondaryFaction, relation);
                    }
                }
            }
            else
            {
                var primaryFaction = factions[(int)relation.FactionA - 1];
                if (relation.FactionB == 0)
                {
                    foreach (var secondaryFaction in factions)
                    {
                        ProcessFactionRelation(primaryFaction, secondaryFaction, relation);
                    }
                }
                else
                {
                    var secondaryFaction = factions[(int)relation.FactionB - 1];
                    ProcessFactionRelation(primaryFaction, secondaryFaction, relation);
                }
            }
        }

        Log.Debug($"FactionHostility initalized");
    }

    public bool IsFriendlyFaction(uint sourceFactionId, uint targetFactionId)
    {
        var key = (sourceFactionId, targetFactionId);
        var found = _factionFriendlyDict.TryGetValue(key, out bool result);
        if (found)
        {
            Log.Debug($"FactionHostility {sourceFactionId} is {(result ? string.Empty : "NOT")} Friendly with {targetFactionId}");
            return result;
        }
        else
        {
            Log.Warning($"IsFriendlyFaction Failed to get relation {sourceFactionId} - {targetFactionId}");
            return false;
        }
    }

    public bool IsHostileFaction(uint sourceFactionId, uint targetFactionId)
    {
        var key = (sourceFactionId, targetFactionId);
        var found = _factionHostileDict.TryGetValue(key, out bool result);
        if (found)
        {
            Log.Debug($"FactionHostility {sourceFactionId} is {(result ? string.Empty : "NOT")} Hostile with {targetFactionId}");
            return result;
        }
        else
        {
            Log.Warning($"IsHostileFaction Failed to get relation {sourceFactionId} - {targetFactionId}");
            return false;
        }
    }

    public HostilityStance GetFactionStance(uint sourceFactionId, uint targetFactionId)
    {
        bool isFriendly = IsFriendlyFaction(sourceFactionId, targetFactionId);
        if (isFriendly)
        {
            return HostilityStance.Friendly;
        }

        bool isHostile = IsHostileFaction(sourceFactionId, targetFactionId);
        if (isHostile)
        {
            return HostilityStance.Hostile;
        }

        return HostilityStance.Neutral;
    }

    /*
    public static void ComputePersonalFactionStance(uint factionId)
    {
        var factions = SDBInterface.GetFactions();
        var totalBytes = (((uint)factions.Count >> 6) + 1) << 3; // 8
        var byteIndex = 0;
        var bitIndex = 0;
        var friendly = new byte[totalBytes];
        var hostile = new byte[totalBytes];
        foreach (var faction in factions)
        {

        }
    }
    */

    private void ProcessFactionRelation(Faction primaryFaction, Faction secondaryFaction, FactionRelations relation)
    {
        var key = (primaryFaction.Id, secondaryFaction.Id);
        bool friendly = false;
        bool hostile = false;

        if (relation.HostilityStance >= 1)
        {
            friendly = true;
        }
        else if (primaryFaction.DefaultStance <= -1)
        {
            hostile = true;
        }

        ProcessFactionRelationSet(key, friendly, hostile);

        if (relation.HostilityBidirectional == 1)
        {
            var bikey = (secondaryFaction.Id, primaryFaction.Id);
            ProcessFactionRelationSet(bikey, friendly, hostile);
        }
    }

    private void ProcessFactionRelationSet((uint, uint) key, bool friendly, bool hostile)
    {
        if (_factionFriendlyDict.ContainsKey(key))
        {
            _factionFriendlyDict[key] = friendly;
        }
        else
        {
            _factionFriendlyDict.Add(key, friendly);
        }

        if (_factionHostileDict.ContainsKey(key))
        {
            _factionHostileDict[key] = hostile;
        }
        else
        {
            _factionHostileDict.Add(key, hostile);
        }
    }
}