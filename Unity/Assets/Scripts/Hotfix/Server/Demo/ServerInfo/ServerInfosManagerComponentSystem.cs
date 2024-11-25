using System.Collections.Generic;

namespace ET.Server
{

    [EntitySystemOf(typeof(ServerInfosManagerComponent))]
    [FriendOfAttribute(typeof(ET.Server.ServerInfosManagerComponent))]
    [FriendOfAttribute(typeof(ET.ServerInfos))]
    public static partial class ServerInfosManagerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.ServerInfosManagerComponent self)
        {
            self.Load();
        }

        [EntitySystem]
        private static void Destroy(this ET.Server.ServerInfosManagerComponent self)
        {
            self.ClearServerInfosList();
        }

        public static void Load(this ServerInfosManagerComponent self)
        {
            self.ClearServerInfosList();

            Dictionary<int, StartZoneConfig> serverInfosConfigs = StartZoneConfigCategory.Instance.GetAll();

            foreach (StartZoneConfig serverInfosConfig in serverInfosConfigs.Values)
            {
                if (serverInfosConfig.ZoneType != 1)
                {
                    continue;
                }

                ServerInfos serverInfos = self.AddChildWithId<ServerInfos>(serverInfosConfig.Id);
                serverInfos.ServerName  = serverInfosConfig.DBName;
                serverInfos.Status      = (int)ServerStatus.Normal;
                self.ServerInfosList.Add(serverInfos);
            }
        }

        public static void ClearServerInfosList(this ServerInfosManagerComponent self)
        {
            foreach (EntityRef<ServerInfos> serverInfoRef in self.ServerInfosList)
            {
                ServerInfos serverInfos = serverInfoRef;
                serverInfos?.Dispose();
            }
            self.ServerInfosList?.Clear();
        }
    }
}