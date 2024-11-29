namespace ET.Server
{
    [Invoke(TimerInvokeType.PlayerOfflineTimeout)]
    public class PlayerOfflineTimeout : ATimer<PlayerOfflineTimeoutComponent>
    {
        protected override void Run(PlayerOfflineTimeoutComponent t)
        {
            t?.KickPlayer();
        }
    }

    [EntitySystemOf(typeof(PlayerOfflineTimeoutComponent))]
    [FriendOfAttribute(typeof(ET.Server.PlayerOfflineTimeoutComponent))]
    public static partial class PlayerOfflineTimeoutComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.PlayerOfflineTimeoutComponent self)
        {
            self.Timer = self.Root().GetComponent<TimerComponent>()
                    .NewOnceTimer(TimeInfo.Instance.ServerNow() + 10000, TimerInvokeType.PlayerOfflineTimeout, self);
        }
        [EntitySystem]
        private static void Destroy(this ET.Server.PlayerOfflineTimeoutComponent self)
        {
            self.Root().GetComponent<TimerComponent>().Remove(ref self.Timer);
        }

        public static void KickPlayer(this PlayerOfflineTimeoutComponent self)
        {
            DisconnectHelper.KickPlayer(self.GetParent<Player>()).Coroutine();
        }
    }
}