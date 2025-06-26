using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GameServer.Systems.Chat;

public class ChatCommandService
{
    private readonly Dictionary<string, Type> _commandDictionary;
    private readonly IShard _shard;

    public ChatCommandService(IShard shard)
    {
        _shard = shard;
        _commandDictionary = new Dictionary<string, Type>();

        LoadCommands();
    }

    public string GetCommandList()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("Available Commands:");

        foreach (var commandType in _commandDictionary.Values.Distinct())
        {
            var command = Activator.CreateInstance(commandType) as ChatCommand;
            var attribute = commandType.GetCustomAttribute<ChatCommandAttribute>();
            stringBuilder.AppendLine($"{attribute.Names[0]}: {attribute.Description}\n\tUsage: {attribute.Usage}\n\tAliases: {string.Join(", ", attribute.Names)}");
        }

        return stringBuilder.ToString();
    }

    public void ExecuteCommand(string input, INetworkPlayer sourcePlayer)
    {
        var (commandName, parameters) = ParseCommand(input);

        if (_commandDictionary.TryGetValue(commandName.ToLower(), out var commandType))
        {
            var command = Activator.CreateInstance(commandType) as ChatCommand;
            command.Execute(parameters, new() { Service = this, Shard = _shard, SourcePlayer = sourcePlayer });
        }
        else
        {
            Console.WriteLine($"Unknown command: {commandName}");
            sourcePlayer?.SendDebugChat($"Unknown command: {commandName}");
        }
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
            .Where(t => t.IsSubclassOf(typeof(ChatCommand)))
            .ToList();

        foreach (var commandType in commandTypes)
        {
            var attribute = commandType.GetCustomAttribute<ChatCommandAttribute>();
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