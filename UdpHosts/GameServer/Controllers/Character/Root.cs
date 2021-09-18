namespace GameServer.Controllers.Character
{
    [ControllerID(Enums.GSS.Controllers.Character)]
    public class Root : Base
    {
        public override void Init(INetworkClient client, IPlayer player, IShard shard)
        {
        }
    }
}