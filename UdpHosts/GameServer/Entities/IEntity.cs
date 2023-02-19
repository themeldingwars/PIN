﻿using System.Collections.Generic;

namespace GameServer.Entities;

public interface IEntity
{
    ulong EntityID { get; }
    IShard Owner { get; }
    IDictionary<Enums.GSS.Controllers, ushort> ControllerRefMap { get; }

    void RegisterController(Enums.GSS.Controllers controller);
}