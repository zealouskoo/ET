namespace ET.Server
{
    [Invoke(TimerInvokeType.AccountSessionCheckoutTime)]
    public class AccountSessionCheckoutTimer : ATimer<AccountSessionComponent>
    {
        protected override void Run(AccountSessionComponent t)
        {
            // t?.DeleteSession();
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

        }

        public static void DeleteSession(this ET.Server.AccountCheckOutTimeComponent self)
        {

        }
    }
}