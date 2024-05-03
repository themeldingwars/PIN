namespace GameServer.Aptitude;

public class CustomNOOPCommand : ICommand
{
    public string Label;
    public uint Id;

    public CustomNOOPCommand(string label, uint id)
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
        return $"NO-OP (Client {Label}, ID {Id})";
    }
}