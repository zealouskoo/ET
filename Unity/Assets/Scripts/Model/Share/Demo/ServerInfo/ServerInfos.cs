namespace ET
{

    public enum ServerStatus
    {
        Normal = 0,
        Stop = 1,
    }
    
    [ChildOf] // 会有不同的父实体，所以不指定 typeof 具体的类型
    public class ServerInfos : Entity, IAwake
    {
        public int Status;
        public string ServerName;
    }
}