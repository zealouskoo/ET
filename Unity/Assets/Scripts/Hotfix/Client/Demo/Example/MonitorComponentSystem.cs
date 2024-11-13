namespace ET.Client
{
    [EntitySystemOf(typeof(MonitorComponent))]
    [FriendOfAttribute(typeof(MonitorComponent))]
    public static partial class MonitorComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.MonitorComponent self, int args2)
        {
            Log.Debug("MonitorComponentSystem Awake");
            self.Brighteness = args2;
        }
        [EntitySystem]
        private static void Destroy(this ET.Client.MonitorComponent self)
        {
            Log.Debug("MonitorComponentSystem Destroy");
        }

        public static void ChangeBrightness(this ET.Client.MonitorComponent self, int value)
        {
            self.Brighteness = value;
            Log.Debug("Monitor's brightness had been set to " + value);
        }
    }
}