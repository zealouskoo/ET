namespace ET.Server
{
    [MessageHandler(SceneType.LoginCenter)]
    public class R2L_LoginAccountRequestHandler : MessageHandler<Scene, R2L_LoginAccountRequest, L2R_LoginAccountRequest>
    {
        protected override async ETTask Run(Scene scene, R2L_LoginAccountRequest request, L2R_LoginAccountRequest response)
        {
            long accountId = request.AccountName.GetLongHashCode();
            
            CoroutineLockComponent coroutineLockComponent = scene.Root().GetComponent<CoroutineLockComponent>();

            using (await coroutineLockComponent.Wait(CoroutineLockType.LoginCenterLock, accountId))
            {
                if (!scene.GetComponent<LoginInfoRecordComponent>().IsExist(accountId))
                {
                    // 没有找到已经上线的账号
                    return;
                }
                
                int zone = scene.GetComponent<LoginInfoRecordComponent>().Get(accountId);
                // 获得网关服务器的地址
                StartSceneConfig gateConfig = RealmGateAddressHelper.GetGate(zone, request.AccountName);
                // 找到账号所在的服务器地址，通知下线
                
                L2G_DisconnectGateUnit l2GDisconnectGateUnit = L2G_DisconnectGateUnit.Create();
                l2GDisconnectGateUnit.AccountName = request.AccountName;
                G2L_DisconnectGateUnit g2lDisconnectGateUnit = 
                        await scene.GetComponent<MessageSender>().Call(gateConfig.ActorId, l2GDisconnectGateUnit) as G2L_DisconnectGateUnit;
                
                // 这个错误直接返回到发送消息的 Realm 端
                response.Error = g2lDisconnectGateUnit.Error;
            }
        }
    }
}