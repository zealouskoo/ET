using System;

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

        public static async ETTask KickPlayerNoLock(Player player)
        {
            if (player == null || player.IsDisposed)
            {
                return;
            }

            switch (player.State)
            {
                case PlayerState.Disconnect:
                    break;
                case PlayerState.Gate:
                    break;
                case PlayerState.Game:
                    // 通知游戏逻辑服下线Unit角色逻辑，并将数据存入数据库
                    M2G_RequestExitGame m2GRequestExitGame = await player.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.Player)
                            .Call(player.UnitId, G2M_RequestExitGame.Create()) as M2G_RequestExitGame;
                    
                    G2L_RemoveLoginReocrd g2LRemoveLoginReocrd = G2L_RemoveLoginReocrd.Create();
                    g2LRemoveLoginReocrd.AccountName = player.Account;
                    g2LRemoveLoginReocrd.ServerId = player.Zone();
                    L2G_RemoveLoginRecord l2GRemoveLoginReocrd = await player.Root().GetComponent<MessageSender>()
                            .Call(StartSceneConfigCategory.Instance.LoginCenterConfig.ActorId, g2LRemoveLoginReocrd) as L2G_RemoveLoginRecord;
                    break;
            }
            
            player.State = PlayerState.Disconnect;
            TimerComponent timerComponent = player.Root().GetComponent<TimerComponent>();
            // 1. 移除Player和PlayerSessionComponent的Location
            await player.GetComponent<PlayerSessionComponent>().RemoveLocation(LocationType.GateSession);
            await player.RemoveLocation(LocationType.Player);   // 登录的时候添加的，退出时进行移除
            // 2. PlayerComponent移除Player
            player.Root().GetComponent<PlayerComponent>().Remove(player);
            // 3. 释放player
            player?.Dispose();
            // 4. 等待0.3秒
            await timerComponent.WaitAsync(300);
        }

        public static async ETTask KickPlayer(Player player)
        {
            if (player == null || player.IsDisposed)
            {
                return;
            }
            
            long instanceId = player.InstanceId;
            CoroutineLockComponent coroutineLockComponent = player.Root().GetComponent<CoroutineLockComponent>();

            using (await coroutineLockComponent.Wait(CoroutineLockType.LoginGate, player.Account.GetLongHashCode()))
            {
                if (player.IsDisposed || instanceId != player.InstanceId)
                {
                    return;
                }
                
                // await KickPlayerNoLock(player);
            }
        }
    }
}