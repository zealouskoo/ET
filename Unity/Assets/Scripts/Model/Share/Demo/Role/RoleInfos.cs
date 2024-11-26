namespace ET
{
    public enum RoleState
    {
        Normal = 0,
        Freeze = 1,
    }
    
    [ChildOf] // 在服务器和客户端有不同的父实体，不指定typeof(Entity)
    public class RoleInfos : Entity, IAwake
    {
        public string Name;
        public int ServerId;
        public int State;
        public string Account;
        public long LastLoginTime;
        public long CreateTime;
    }
}