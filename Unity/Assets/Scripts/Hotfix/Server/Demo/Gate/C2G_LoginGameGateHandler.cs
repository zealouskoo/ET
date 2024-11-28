namespace ET.Server
{
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_LoginGameGateHandler : MessageSessionHandler<C2G_LoginGameGate, G2C_LoginGameGate>
    {
        protected override async ETTask Run(Session session, C2G_LoginGameGate request, G2C_LoginGameGate response)
        {
            if (session.GetComponent<SessionLockingComponent>() != null)
            {
                response.Error = ErrorCode.ERR_LoginGateRepeatedly;
                session.Disconnect().Coroutine();
                return;
            }

            string token = session.Root().GetComponent<TokenComponent>().Get(request.AccountName);
            // if(string.IsNullOrEmpty(token) || !token.Equals(request.))
        }
    }
}