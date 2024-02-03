using System;
using System.Collections.Generic;
using System.Numerics;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Data.SDB;
using GameServer.Data.SDB.Records.apt;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Data.SDB.Records.customdata;
using GameServer.Enums.GSS.Character;

namespace GameServer.Aptitude;

public class Factory
{
    public Effect LoadEffect(uint effectId)
    {
        var statusEffectData = SDBInterface.GetStatusEffectData(effectId);
        var effect = new Effect()
        {
            Data = statusEffectData,
        };

        if (statusEffectData.ApplyChain != 0)
        {
            effect.ApplyChain = LoadChain(statusEffectData.ApplyChain);
        }

        if (statusEffectData.RemoveChain != 0)
        {
            effect.RemoveChain = LoadChain(statusEffectData.RemoveChain);
        }

        if (statusEffectData.UpdateChain != 0)
        {
            effect.UpdateChain = LoadChain(statusEffectData.UpdateChain);
        }

        if (statusEffectData.DurationChain != 0)
        {
            effect.DurationChain = LoadChain(statusEffectData.DurationChain);
        }

        return effect;
    }

    public Chain LoadChain(uint chainId)
    {
        var chain = new Chain();
        chain.Id = chainId;
        chain.Commands = new List<ICommand>();

        uint next = chainId;
        while (next != 0)
        {
            var baseCommandDef = SDBInterface.GetBaseCommandDef(next);
            var command = LoadCommand(baseCommandDef.Id, baseCommandDef.Subtype);
            chain.Commands.Add(command);
            next = baseCommandDef.Next;
        }

        if (chain.Commands.Count == 0)
        {
            Console.WriteLine($"Loaded empty chain {chainId}");
        }

        return chain;
    }

    public ICommand LoadCommand(uint commandId, uint typeId)
    {
        var commandTypeRec = SDBInterface.GetCommandType(typeId);
        var commandType = (CommandType)commandTypeRec.Id;

        // :) Fix this null terminator later
        if (commandTypeRec.Environment == "client\0")
        {
            // Far as I know we don't care about client commands on the server, though the params can be helpful.
            return new CustomNOOPCommand(commandType.ToString(), commandId);
        }

        switch ((CommandType)commandTypeRec.Id)
        {
            case CommandType.ImpactApplyEffect:
                return new ImpactApplyEffectCommand(SDBInterface.GetImpactApplyEffectCommandDef(commandId));
            case CommandType.ImpactToggleEffect:
                break;
            case CommandType.ImpactRemoveEffect:
                break;
            case CommandType.Call:
                return new CallCommand(SDBInterface.GetCallCommandDef(commandId));
            case CommandType.ConditionalBranch:
                return new ConditionalBranchCommand(SDBInterface.GetConditionalBranchCommandDef(commandId));
            case CommandType.LogicAndChain:
                return new LogicAndChainCommand(SDBInterface.GetLogicAndChainCommandDef(commandId));
            case CommandType.LogicOrChain:
                return new LogicOrChainCommand(SDBInterface.GetLogicOrChainCommandDef(commandId));
            case CommandType.LogicNegate:
                return new LogicNegateCommand(SDBInterface.GetLogicNegateCommandDef(commandId));
            case CommandType.LogicOr:
                return new LogicOrCommand(SDBInterface.GetLogicOrCommandDef(commandId));
            case CommandType.WhileLoop:
                return new WhileLoopCommand(SDBInterface.GetWhileLoopCommandDef(commandId));
            case CommandType.InstantActivation:
                return new InstantActivationCommand(SDBInterface.GetInstantActivationCommandDef(commandId));
            case CommandType.TimeDuration:
                return new TimeDurationCommand(SDBInterface.GetTimeDurationCommandDef(commandId));
            case CommandType.AirborneDuration:
                return new AirborneDurationCommand(SDBInterface.GetAirborneDurationCommandDef(commandId));
            case CommandType.RequireHasEffect:
                return new RequireHasEffectCommand(SDBInterface.GetRequireHasEffectCommandDef(commandId));
            case CommandType.RequireMovestate:
                return new RequireMovestateCommand(SDBInterface.GetRequireMovestateCommandDef(commandId));
            case CommandType.TargetByObjectType:
                return new TargetByObjectTypeCommand(SDBInterface.GetTargetByObjectTypeCommandDef(commandId));
            case CommandType.TargetSelf:
                return new TargetSelfCommand(SDBInterface.GetTargetSelfCommandDef(commandId));
            case CommandType.TargetByCharacterState:
                return new TargetByCharacterStateCommand(SDBInterface.GetTargetByCharacterStateCommandDef(commandId));
            case CommandType.ForcePush:
                return new ForcePushCommand(SDBInterface.GetForcePushCommandDef(commandId));
            case CommandType.ModifyPermission:
                return new ModifyPermissionCommand(CustomDBInterface.GetModifyPermissionCommandDef(commandId));
            case CommandType.SetGliderParametersDef:
                return new SetGliderParametersCommand(CustomDBInterface.GetSetGliderParametersCommandDef(commandId));
            case CommandType.BeginInteraction:
                return new BeginInteractionCommand();
            case CommandType.EndInteraction:
                return new EndInteractionCommand();
            case CommandType.InteractionType:
                return new InteractionTypeCommand(SDBInterface.GetInteractionTypeCommandDef(commandId));
            case CommandType.AuthorizeTerminal:
                return new AuthorizeTerminalCommand(CustomDBInterface.GetAuthorizeTerminalCommandDef(commandId));
            default:
                break;
        }

        return new CustomPlaceholderCommand(commandType.ToString(), commandId);
    }
}