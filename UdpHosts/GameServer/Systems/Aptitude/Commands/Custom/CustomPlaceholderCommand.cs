namespace GameServer.Aptitude;

public class CustomPlaceholderCommand : ICommand
{
    public string Label;

    public CustomPlaceholderCommand(string label, uint id)
    {
        Label = label;
        Id = id;
    }
    
    public uint Id { get; set; }

    public bool Execute(Context context)
    {
        return true;
    }

    public override string ToString()
    {
        return $"PLACEHOLDER ({Label}, ID {Id})";
    }
}