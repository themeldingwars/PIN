namespace GameServer.Physics;

public struct PhysicsEngineSettings
{
    public uint ZoneId;
    public string MapsPath = string.Empty;
    public string AssetDBPath = string.Empty;
    public string CachePath = string.Empty;
    public bool LoadMapsCollision;
    public bool ForceReload = false;
    public bool EnableDebugPipe = false;
    public bool IsDebugPipeClient = false;

    public bool EnablePhysicsDebugRenderer = false;
    public int PhysicsDebugRendererFrameRate = 60;
    public bool PhysicsDebugRendererShowConstraints = true;
    public bool PhysicsDebugRendererShowContacts = false;
    public bool PhysicsDebugRendererShowBoundingBoxes = true;

    public PhysicsEngineSettings()
    {
    }
}