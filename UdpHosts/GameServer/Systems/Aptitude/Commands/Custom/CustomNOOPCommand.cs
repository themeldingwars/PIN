namespace GameServer.Aptitude;

public class CustomNOOPCommand : ICommand
{
    public string Label;
    
    public CustomNOOPCommand(string label, uint id)
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
        return $"NO-OP (Client {Label}, ID {Id})";
    }
}