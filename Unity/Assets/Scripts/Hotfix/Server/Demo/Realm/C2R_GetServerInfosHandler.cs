namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    [FriendOfAttribute(typeof(ET.Server.ServerInfosManagerComponent))]
    public class C2R_GetServerInfosHandler : MessageSessionHandler<C2R_GetServerInfos, R2C_GetServerInfos>
    {
        protected override async ETTask Run(Session session, C2R_GetServerInfos request, R2C_GetServerInfos response)
        {
            // 根据请求的账号名获得在 Session 中登录的令牌
            string token = session.Root().GetComponent<TokenComponent>().Get(request.Account);
            // 比较客户端传入的令牌和 session 中保存的是否相同 
            if (token == null || token != request.Token)
            {
                response.Error = ErrorCode.ERR_TokenError;
                session.Disconnect().Coroutine();
                return;
            }

            foreach (var serverInfosRef in session.Root().GetComponent<ServerInfosManagerComponent>().ServerInfosList)
            {
                ServerInfos serverInfos = serverInfosRef;
                response.ServerInfosList.Add(serverInfos.ToMessage());
            }
            
            await ETTask.CompletedTask;
        }
    }
}