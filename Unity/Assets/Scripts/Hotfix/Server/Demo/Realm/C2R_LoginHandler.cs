using System.Collections.Generic;

namespace ET.Server
{
    // 要保证 Realm 这个实体能有操作数据库的能力
    // 思考 Realm 是在哪里被创建的？
    [MessageSessionHandler(SceneType.Realm)]
    [FriendOfAttribute(typeof(ET.Server.AccountInfo))]
    public class C2R_LoginHandler : MessageSessionHandler<C2R_Login, R2C_Login> {
        protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response) {
            // 检查传入的用户名和密码是否为空
            // 是的话则返回错误代码
            // 并关闭 session
            if (string.IsNullOrEmpty(request.Account) || string.IsNullOrEmpty((request.Password)))
            {
                response.Error = ErrorCode.ERR_LoginInfoEmpty;
                CloseSession(session).Coroutine();
                return;
            }
            
            // 使用协程锁在使用协程和异步操作的情况下保证账号的唯一性
            using (await session.Root().GetComponent<CoroutineLockComponent>()
                           .Wait(CoroutineLockType.LoginAccount, request.Account.GetLongHashCode()))
            {
                DBComponent db = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
                List<AccountInfo> accountInfos = await db.Query<AccountInfo>(accountInfo => accountInfo.Account.Equals(request.Account));

                if (accountInfos.Count <= 0)
                {
                    AccountInfosComponent accountInfosComponent =
                            session.GetComponent<AccountInfosComponent>()
                            ?? session.AddComponent<AccountInfosComponent>();

                    AccountInfo accountInfo = accountInfosComponent.AddChild<AccountInfo>();
                    accountInfo.Account = request.Account;
                    accountInfo.Password = request.Password;
                    await db.Save(accountInfo);
                }
                else
                {
                    // 只需要检查第一项即可，用户名不允许重复。
                    if (accountInfos[0].Password != request.Password)
                    {
                        response.Error = ErrorCode.ERR_LoginPasswordError;
                        CloseSession(session).Coroutine();
                        return;
                    }
                }
            }

            // While query AccountInfo, we'll use the field: Account in AccountInfo,
            // you should add FriendOfAttribute or encounter an error
            
            // if there is no match Account(queried infos <= 0), we should create and save it.
            
            // if some data are found, we should compare the password if matched.
            
            // 随机分配一个Gate
            StartSceneConfig config = RealmGateAddressHelper.GetGate(session.Zone(), request.Account);
            Log.Debug($"gate address: {config}");

            // 向gate请求一个key,客户端可以拿着这个key连接gate
            R2G_GetLoginKey r2GGetLoginKey = R2G_GetLoginKey.Create();
            r2GGetLoginKey.Account = request.Account;
            G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey) await session.Fiber().Root.GetComponent<MessageSender>().Call(
                config.ActorId, r2GGetLoginKey);

            response.Address = config.InnerIPPort.ToString();
            response.Key = g2RGetLoginKey.Key;
            response.GateId = g2RGetLoginKey.GateId;

            CloseSession(session).Coroutine();
        }

        private async ETTask CloseSession(Session session) {
            await session.Root().GetComponent<TimerComponent>().WaitAsync(1000);
            session.Dispose();
        }
    }
}
