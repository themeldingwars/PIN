namespace GameServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var server = new GameServer();
            server.Run();
        }
    }
}
