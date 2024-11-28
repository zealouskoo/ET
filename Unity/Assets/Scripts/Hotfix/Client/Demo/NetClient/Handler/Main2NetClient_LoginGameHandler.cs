using ET.Server;

namespace ET.Client
{
    [MessageHandler(SceneType.NetClient)]
    public class Main2NetClient_LoginGameHandler : MessageHandler<Scene, Main2NetClient_LoginGame, NetClient2Main_LoginGame>
    {
        protected override async ETTask Run(Scene root, Main2NetClient_LoginGame request, NetClient2Main_LoginGame response)
        {
            // 创建一个 gate session，并且保存到 SessionComponent 中
            NetComponent netComponent = root.GetComponent<NetComponent>();
            Session gateSession =
                    await netComponent.CreateRouterSession(NetworkHelper.ToIPEndPoint(request.GateAddress), request.Account, request.Account);
            gateSession.AddComponent<ClientSessionErrorComponent>();
            root.GetComponent<SessionComponent>().Session = gateSession;
            
            // 创建 C2G_LoginGameGate 消息，并发送
            C2G_LoginGameGate c2GLoginGameGate = C2G_LoginGameGate.Create();
            c2GLoginGameGate.Key = request.RealmKey;
            c2GLoginGameGate.AccountName = request.Account;
            c2GLoginGameGate.RoleId = request.RoleId;
            G2C_LoginGameGate g2CLoginGameGate = await gateSession.Call(c2GLoginGameGate) as G2C_LoginGameGate;

            if (g2CLoginGameGate.Error != ErrorCode.ERR_Success)
            {
                response.Error = g2CLoginGameGate.Error;
                Log.Error("登录 Gate 网关服务器失败！");
                gateSession.Disconnect().Coroutine();
                return;
            }
            Log.Debug("登录网关服务器成功！");
            
            G2C_EnterGame g2CEnterGame = await gateSession.Call(C2G_EnterGame.Create()) as G2C_EnterGame;
            if (g2CEnterGame.Error != ErrorCode.ERR_Success)
            {
                response.Error = g2CEnterGame.Error;
                Log.Error("登录 Map 服务器失败！");
                gateSession.Disconnect().Coroutine();
                return;
            }
            Log.Debug("登录 Map 服务器成功！");
            g2CLoginGameGate.PlayerId = g2CEnterGame.MyUnitId;
        }
    }
}