namespace GameServer.Systems.Aptitude.Commands.Custom;

public class CustomNOOPCommand : ICommand
{
    public string Label;

    public CustomNOOPCommand(string label, uint id)
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
        return $"NO-OP (Client {Label}, ID {Id})";
    }
}