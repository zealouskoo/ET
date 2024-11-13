using System.Diagnostics;

namespace ET.Client
{
    [EntitySystemOf(typeof(Computer))]
    public static partial class ComputerSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.Computer self)
        {
            Log.Debug("Computer awake!");
        }
        [EntitySystem]
        private static void Update(this ET.Client.Computer self)
        {
            Log.Debug("Computer update!");
        }
        [EntitySystem]
        private static void Destroy(this ET.Client.Computer self)
        {
            Log.Debug("Computer Destroy!");
        }

        public static void Open(this ET.Client.Computer self)
        {
            Log.Debug("Computer Open!");
        }
    }
}