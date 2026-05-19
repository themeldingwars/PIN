namespace GameServer.Systems.Aptitude.Commands.Custom;

public class CustomPlaceholderCommand : ICommand
{
    public string Label;

    public CustomPlaceholderCommand(string label, uint id)
    {
        Label = label;
        Id = id;
    }

    public uint Id { get; set; }

    public void Execute(Context context, ref CommandResult result)
    {
    }

    public override string ToString()
    {
        return $"PLACEHOLDER ({Label}, ID {Id})";
    }
}