namespace ET
{
    [EntitySystemOf(typeof(RoleInfos))]
    [FriendOfAttribute(typeof(ET.RoleInfos))]
    public static partial class RoleInfoSystem
    {
        [EntitySystem]
        private static void Awake(this ET.RoleInfos self)
        {

        }

        public static void FromMessage(this ET.RoleInfos self, RoleInfosProto proto)
        {
            self.Name = proto.RoleName;
            self.State = proto.State;
            self.Account = proto.Account;
            self.ServerId = proto.ServerId;
            self.CreateTime = proto.CreateTime;
            self.LastLoginTime = proto.LastLoginTime;
        }

        public static RoleInfosProto ToMessage(this ET.RoleInfos self)
        {
            RoleInfosProto proto = RoleInfosProto.Create();
            
            proto.RoleName = self.Name;
            proto.State = self.State;
            proto.Account = self.Account;
            proto.ServerId = self.ServerId;
            proto.CreateTime = self.CreateTime;
            proto.LastLoginTime = self.LastLoginTime;
            
            return proto;
        }
    }
}