namespace ET.Server
{

    [ComponentOf(typeof(Player))]
    public class PlayerOfflineTimeoutComponent : Entity, IAwake, IDestroy
    {
        public long Timer;
    }
}