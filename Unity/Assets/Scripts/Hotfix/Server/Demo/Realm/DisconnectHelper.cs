namespace ET.Server
{
    public static class DisconnectHelper
    {
        public static async ETTask Disconnect(this Session self)
        {
            // 1. 判断 self 状态

            if (self == null || self.IsDisposed)
            {
                return;
            }
            
            long instanceId = self.InstanceId;
            // 2. 调用 TimerComponent 组件的 WaitAsync 方法等待 1 秒钟后
            await self.Root().GetComponent<TimerComponent>().WaitAsync(1000);
            //    对比 session 是否发生改变，没有变化则释放 session 连接
            if (instanceId != self.InstanceId)
            {
                return;
            }
            
            self.Dispose();
        }

        public static async ETTask KickPlayer(Player player)
        {
        }
    }
}