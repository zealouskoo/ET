namespace ET.Server
{
    [MessageHandler(SceneType.Gate)]
    public class L2G_DisconnectGateUnitHandler : MessageHandler<Scene, L2G_DisconnectGateUnit, G2L_DisconnectGateUnit>
    {
        protected override async ETTask Run(Scene scene, L2G_DisconnectGateUnit request, G2L_DisconnectGateUnit response)
        {
            // 1. 检查玩家是否在线
            PlayerComponent playerComponent = scene.GetComponent<PlayerComponent>();
            Player player = playerComponent.GetByAccount(request.AccountName);
            if (player == null)
            {
                return;
            }
            // 2. 在 GateSessionKeyComponent 中移除用户
            scene.GetComponent<GateSessionKeyComponent>().Remove(request.AccountName.GetLongHashCode());
            
            // 3. 获取 session,发送 A2C_Disconnect 消息
            Session gateSession = player.GetComponent<PlayerSessionComponent>().Session;
            if (gateSession != null && !gateSession.IsDisposed)
            {
                A2C_Disconnect a2CDisconnect = A2C_Disconnect.Create();
                a2CDisconnect.Error = ErrorCode.ERR_OtherAccountLogin;
                gateSession.Send(a2CDisconnect);
                gateSession?.Disconnect().Coroutine();
            }

            PlayerSessionComponent playerSessionComponent = player.GetComponent<PlayerSessionComponent>();
            
            if (playerSessionComponent?.Session != null)
            {
                playerSessionComponent.Session = null;
            }

            player.AddComponent<PlayerOfflineTimeoutComponent>();
        }
    }
}