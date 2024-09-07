using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Command;
using AeroMessages.GSS.V66.Character.Event;
using AeroMessages.GSS.V66.Generic;
using GameServer.Admin;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Enums;

namespace GameServer;

public class AdminService
{
    private readonly Dictionary<string, Type> _commandDictionary;
    private Dictionary<INetworkPlayer, IEntity> _targetDictionary;
    private Shard _shard;

    public AdminService(Shard shard)
    {
        _shard = shard;
        _commandDictionary = new Dictionary<string, Type>();
        _targetDictionary = new Dictionary<INetworkPlayer, IEntity>();

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
            Console.WriteLine($"Unknown command: {commandName}");
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