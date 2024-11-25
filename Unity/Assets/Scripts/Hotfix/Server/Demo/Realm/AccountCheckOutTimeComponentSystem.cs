namespace ET.Server
{
    [Invoke(TimerInvokeType.AccountSessionCheckoutTime)]
    public class AccountSessionCheckoutTimer : ATimer<AccountCheckOutTimeComponent>
    {
        protected override void Run(AccountCheckOutTimeComponent t)
        {
            t?.DeleteSession();
        }
    }

    [EntitySystemOf(typeof(AccountCheckOutTimeComponent))]
    [FriendOfAttribute(typeof(ET.Server.AccountCheckOutTimeComponent))]
    public static partial class AccountCheckOutTimeComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.AccountCheckOutTimeComponent self, string accountName)
        {
            self.AccountName = accountName;
            // 移除计时器
            self.Root().GetComponent<TimerComponent>().Remove(ref self.Timer);
            // 单次运行的计时器
            self.Timer = self.Root().GetComponent<TimerComponent>().NewOnceTimer(TimeInfo.Instance.ServerNow() + 600000, 
                                                                                (int)TimerInvokeType.AccountSessionCheckoutTime, 
                                                                                self);
        }
        [EntitySystem]
        private static void Destroy(this ET.Server.AccountCheckOutTimeComponent self)
        {
            self.Root().GetComponent<TimerComponent>().Remove(ref self.Timer);
        }

        public static void DeleteSession(this ET.Server.AccountCheckOutTimeComponent self)
        {
            // 1. 获取当前 session，因为 AccountCheckOutTimeComponent 是 session 的一个组件
            Session session = self.GetParent<Session>();
            // 2. 获取之前 session
            Session originSession = session.Root().GetComponent<AccountSessionComponent>().Get(self.AccountName);
            // 如果两个 session 一致，则将 account 对应的 session 自字典表中移除
            if (originSession != null || session.InstanceId.Equals(originSession.InstanceId))
            {
                session.Root().GetComponent<AccountSessionComponent>().Remove(self.AccountName);
            }
            // 3. 创建 A2C_Disconnect 消息
            A2C_Disconnect a2CDisconnect = A2C_Disconnect.Create();
            // 4. a2CDisconnect 事件的错误码设为 1
            a2CDisconnect.Error = 1;
            // 5. 使用 session 发送 a2CDisconnect 消息
            session?.Send(a2CDisconnect);
            // 6. 关闭 session
            session?.Disconnect().Coroutine();
            // A2C_Disconnect
        }
    }
}