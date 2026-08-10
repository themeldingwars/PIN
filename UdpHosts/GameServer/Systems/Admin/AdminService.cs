using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GameServer.Data;
using GameServer.Entities;
using Serilog;

namespace GameServer.Systems.Admin;

public class AdminService
{
    private static readonly ILogger _logger = Log.ForContext<AdminService>();

    private readonly Dictionary<string, Type> _commandDictionary;
    private readonly Dictionary<INetworkPlayer, IEntity> _targetDictionary;
    private readonly Dictionary<INetworkPlayer, Dictionary<LoadoutSlotType, uint>> _equipmentOverrides;
    private readonly Shard _shard;

    public AdminService(Shard shard)
    {
        _shard = shard;
        _commandDictionary = [];
        _targetDictionary = [];
        _equipmentOverrides = [];

        LoadCommands();
    }

    public string GetCommandList()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("Available Commands:");

        foreach (var commandType in _commandDictionary.Values.Distinct())
        {
            var command = Activator.CreateInstance(commandType) as ServerCommand;
            var attribute = commandType.GetCustomAttribute<ServerCommandAttribute>();
            stringBuilder.AppendLine($"{attribute.Names[0]}: {attribute.Description}\n\tUsage: {attribute.Usage}\n\tAliases: {string.Join(", ", attribute.Names)}");
        }

        return stringBuilder.ToString();
    }

    public void ExecuteCommand(string input, INetworkPlayer sourcePlayer)
    {
        var (commandName, parameters) = ParseCommand(input);

        if (_commandDictionary.TryGetValue(commandName.ToLower(), out var commandType))
        {
            var command = Activator.CreateInstance(commandType) as ServerCommand;
            command.Execute(parameters, new() { Service = this, Shard = _shard, SourcePlayer = sourcePlayer, Target = _targetDictionary.GetValueOrDefault(sourcePlayer) });
        }
        else
        {
            _logger.Information("Unknown command: {commandName} from {SourcePlayer}", commandName, sourcePlayer);
            sourcePlayer?.SendDebugChat($"Unknown command: {commandName}");
        }
    }

    public void SetTarget(INetworkPlayer player, IEntity target)
    {
        _targetDictionary[player] = target;
    }

    public void ClearTarget(INetworkPlayer player)
    {
        _targetDictionary.Remove(player);
    }

    public void SetEquipmentOverride(INetworkPlayer player, LoadoutSlotType slot, uint itemId)
    {
        if (!_equipmentOverrides.TryGetValue(player, out var overrides))
        {
            overrides = [];
            _equipmentOverrides[player] = overrides;
        }

        overrides[slot] = itemId;
        _logger.Information("Player {Player} set equipment override {Slot} = {ItemId}", player, slot, itemId);
    }

    public bool RemoveEquipmentOverride(INetworkPlayer player, LoadoutSlotType slot)
    {
        if (_equipmentOverrides.TryGetValue(player, out var overrides) && overrides.Remove(slot))
        {
            _logger.Information("Player {Player} removed equipment override {Slot}", player, slot);
            return true;
        }

        return false;
    }

    public int ClearEquipmentOverrides(INetworkPlayer player)
    {
        if (!_equipmentOverrides.TryGetValue(player, out var overrides))
        {
            return 0;
        }

        var count = overrides.Count;
        _equipmentOverrides.Remove(player);
        _logger.Information("Player {Player} cleared {Count} equipment overrides", player, count);
        return count;
    }

    public Dictionary<LoadoutSlotType, uint> GetEquipmentOverrides(INetworkPlayer player)
    {
        return _equipmentOverrides.TryGetValue(player, out var overrides) && overrides.Count > 0 ? overrides : null;
    }

    /// <summary>
    /// Applies the player's temporary equipment overrides to a loadout being applied to their character.
    /// </summary>
    public void ApplyEquipmentOverrides(INetworkPlayer player, CharacterLoadout loadout)
    {
        if (player == null || loadout == null || !_equipmentOverrides.TryGetValue(player, out var overrides) || overrides.Count == 0)
        {
            return;
        }

        foreach (var (slot, itemId) in overrides)
        {
            loadout.SlottedItems[slot] = itemId;
        }

        _logger.Information("Player {Player} applied {Count} equipment overrides to loadout {LoadoutId}: {Overrides}", player, overrides.Count, loadout.LoadoutID, string.Join(", ", overrides.Select(o => $"{o.Key}={o.Value}")));
    }

    public void ClearPlayer(INetworkPlayer player)
    {
        _targetDictionary.Remove(player);
        _equipmentOverrides.Remove(player);
    }

    private (string commandName, string[] parameters) ParseCommand(string input)
    {
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var commandName = parts[0];
        var parameters = parts.Skip(1).ToArray();

        return (commandName, parameters);
    }

    private void LoadCommands()
    {
        var commandTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ServerCommand)))
            .ToList();

        foreach (var commandType in commandTypes)
        {
            var attribute = commandType.GetCustomAttribute<ServerCommandAttribute>();
            if (attribute != null)
            {
                foreach (var name in attribute.Names)
                {
                    _commandDictionary.Add(name.ToLower(), commandType);
                }
            }
        }
    }
}