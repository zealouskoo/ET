namespace ET
{
    [EntitySystemOf(typeof(ServerInfos))]
    [FriendOfAttribute(typeof(ET.ServerInfos))]
    public static partial class ServerInfoSystem
    {
        [EntitySystem]
        private static void Awake(this ET.ServerInfos self)
        {

        }

        public static void FromMessage(this ET.ServerInfos self, ServerInfoProto serverInfoProto)
        {
            self.ServerName = serverInfoProto.ServerName;
            self.Status = serverInfoProto.Status;
        }

        public static ServerInfoProto ToMessage(this ET.ServerInfos self)
        {
            ServerInfoProto serverInfoProto = ServerInfoProto.Create();
            serverInfoProto.Id = (int)self.Id;
            serverInfoProto.ServerName = self.ServerName;
            serverInfoProto.Status = self.Status;
            return serverInfoProto;
        }
    }
}