namespace GameServer.Aptitude;

public class CustomPlaceholderCommand : ICommand
{
    public string Label;
    public uint Id;

    public CustomPlaceholderCommand(string label, uint id)
    {
        Label = label;
        Id = id;
    }

    public bool Execute(Context context)
    {
        return true;
    }

    public override string ToString()
    {
        return $"PLACEHOLDER ({Label})";
    }
}