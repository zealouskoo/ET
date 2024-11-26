namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    [FriendOfAttribute(typeof(ET.RoleInfos))]
    public class C2R_DeleteRoleHandler : MessageSessionHandler<C2R_DeleteRole, R2C_DeleteRole>
    {
        protected override async ETTask Run(Session session, C2R_DeleteRole request, R2C_DeleteRole response)
        {
            if (session.GetComponent<SessionLockingComponent>() != null)
            {
                response.Error = ErrorCode.ERR_DeleteRepeatedly;
                session.Disconnect().Coroutine();
                return;
            }

            string token = session.Root().GetComponent<TokenComponent>().Get(request.Account);
            if (string.IsNullOrEmpty(token) || !token.Equals(request.Token))
            {
                response.Error = ErrorCode.ERR_TokenError;
                session.Disconnect().Coroutine();
                return;
            }

            CoroutineLockComponent coroutineLockComponent = session.Root().GetComponent<CoroutineLockComponent>();
            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await coroutineLockComponent.Wait(CoroutineLockType.DeleteRole, request.Account.GetLongHashCode()))
                {
                    DBComponent db = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());

                    var roleInfosList = await db.Query<RoleInfos>(role => role.Account.Equals(request.Account)
                                                                        && role.ServerId.Equals(request.ServerId)
                                                                        && role.Id.Equals(request.RoleInfosId));

                    if (roleInfosList == null || roleInfosList.Count <= 0)
                    {
                        response.Error = ErrorCode.ERR_RoleNotFound;
                        return;
                    }

                    RoleInfos roleInfos = roleInfosList[0];
                    session.AddChild(roleInfos);

                    roleInfos.State = (int)RoleState.Freeze;
                    await db.Save(roleInfos);
                    response.DeletedRoleInfosId = roleInfos.Id;
                    roleInfos?.Dispose();
                }
            }
        }
    }
}