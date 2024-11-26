namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    [FriendOfAttribute(typeof(ET.RoleInfos))]
    public class C2R_CreateRoleHandler : MessageSessionHandler<C2R_CreateRole, R2C_CreateRole>
    {
        protected override async ETTask Run(Session session, C2R_CreateRole request, R2C_CreateRole response)
        {
            // 是否有 session 锁
            if (session.GetComponent<SessionLockingComponent>() != null)
            {
                response.Error = ErrorCode.ERR_CreateRoleRepeatedly;
                session.Disconnect().Coroutine();
                return;
            }
            // 比对 token 是否有效
            string token = session.Root().GetComponent<TokenComponent>().Get(request.Account);
            if (string.IsNullOrEmpty(token) || !token.Equals(request.Token))
            {
                response.Error = ErrorCode.ERR_TokenError;
                session.Disconnect().Coroutine();
                return;
            }
            // 检查传入的名称不为空
            if (string.IsNullOrEmpty(request.Name))
            {
                response.Error = ErrorCode.ERR_RoleNameEmpty;
                session.Disconnect().Coroutine();
                return;
            }

            // 锁 session 和 coroutine
            CoroutineLockComponent coroutineLockComponent = session.Root().GetComponent<CoroutineLockComponent>();
            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await coroutineLockComponent.Wait(CoroutineLockType.CreateRole, request.Account.GetLongHashCode()))
                {
                    DBComponent db = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());

                    var roleInfos = await db.Query<RoleInfos>(role => role.Name.Equals(request.Name)
                                                            && role.ServerId.Equals(request.ServerId));

                    if (roleInfos != null && roleInfos.Count > 0)
                    {
                        response.Error = ErrorCode.ERR_RoleDuplicate;
                        return;
                    }

                    RoleInfos newRoleInfos = session.AddChild<RoleInfos>();
                    newRoleInfos.Name = request.Name;
                    newRoleInfos.ServerId = request.ServerId;
                    newRoleInfos.State = (int)RoleState.Normal;
                    newRoleInfos.Account = request.Account;
                    newRoleInfos.CreateTime = TimeInfo.Instance.ServerNow();
                    newRoleInfos.LastLoginTime = 0;

                    await db.Save(newRoleInfos);
                }
            }
            //
        }
    }
}