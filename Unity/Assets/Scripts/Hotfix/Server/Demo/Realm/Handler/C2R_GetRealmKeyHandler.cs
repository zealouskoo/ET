namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    public class C2R_GetRealmKeyHandler : MessageSessionHandler<C2R_GetRealmKey, R2C_GetRealmKey>
    {
        protected override async ETTask Run(Session session, C2R_GetRealmKey request, R2C_GetRealmKey response)
        {
            if (session.GetComponent<SessionLockingComponent>() != null)
            {
                response.Error = ErrorCode.ERR_GetRealmKeyRepeatedly;
                session.Disconnect().Coroutine();
                return;
            }

            string token = session.Root().GetComponent<TokenComponent>().Get(request.Account);
            if (string.IsNullOrEmpty(token) || !token.Equals(request.Account))
            {
                response.Error = ErrorCode.ERR_TokenError;
                session.Disconnect().Coroutine();
                return;
            }
            
            CoroutineLockComponent coroutineLockComponent = session.Root().GetComponent<CoroutineLockComponent>();
            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await coroutineLockComponent.Wait(CoroutineLockType.GetRealmKeyLock, request.Account.GetLongHashCode()))
                {
                    // 随机分配一个 Gate
                    StartSceneConfig gateConfig = RealmGateAddressHelper.GetGate(session.Zone(), request.Account);
                    Log.Debug($"Gate address: {gateConfig}");
                    
                    // 向 gate 请求一个 key，客户端可以用这个 key 连接 gate
                    R2G_GetLoginKey r2GGetLoginKey = R2G_GetLoginKey.Create();
                    r2GGetLoginKey.Account = request.Account;
                    G2R_GetLoginKey g2RGetLoginKey = 
                            await session.Root().GetComponent<MessageSender>().Call(gateConfig.ActorId, r2GGetLoginKey) as G2R_GetLoginKey;

                    if (g2RGetLoginKey.Error != ErrorCode.ERR_Success)
                    {
                        Log.Debug("获取 LoginKey 错误！");
                        response.Error = g2RGetLoginKey.Error;
                        session.Disconnect().Coroutine();
                        return;
                    }

                    response.Address = gateConfig.InnerIPPort.ToString();
                    response.Key = g2RGetLoginKey.Key;
                    response.GateId = g2RGetLoginKey.GateId;
                    
                    session.Disconnect().Coroutine();
                }
            }
        }
    }
}