namespace ET.Server
{
    [ComponentOf(typeof(Session))]
    public class AccountCheckOutTimeComponent : Entity, IAwake<string>, IDestroy
    {
        // 定时器 ID
        public long Timer = 0;
        
        // 用户名
        public string AccountName = "";
    }
}