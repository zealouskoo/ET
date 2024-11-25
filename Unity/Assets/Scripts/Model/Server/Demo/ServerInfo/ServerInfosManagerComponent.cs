using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class ServerInfosManagerComponent : Entity, IAwake, IDestroy
    {
        public List<EntityRef<ServerInfos>> ServerInfosList = new List<EntityRef<ServerInfos>>();
    }
}