using System.Collections.Generic;

namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    [FriendOfAttribute(typeof(ET.RoleInfos))]
    public class C2R_GetRoleInfosHandler : MessageSessionHandler<C2R_GetRoleInfos, R2C_GetRoleInfos>
    {
        protected override async ETTask Run(Session session, C2R_GetRoleInfos request, R2C_GetRoleInfos response)
        {
            // 检查是否存在 session 锁
            if (session.GetComponent<SessionLockingComponent>() != null)
            {
                response.Error = ErrorCode.ERR_LoginRepeatedly;
                session.Disconnect().Coroutine();
                return;
            }
            // 检查 Token 是否正确
            string token = session.Root().GetComponent<TokenComponent>().Get(request.Account);

            if (string.IsNullOrEmpty(token) || !token.Equals(request.Token))
            {
                response.Error = ErrorCode.ERR_TokenError;
                session.Disconnect().Coroutine();
                return;
            }

            // 锁 session
            // 锁协程
            // 获取数据库组件,查询用户名和区服 ID 相对应的角色信息
            CoroutineLockComponent coroutineLockComponent = session.Root().GetComponent<CoroutineLockComponent>();
            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await coroutineLockComponent.Wait(CoroutineLockType.CreateRole, request.Account.GetLongHashCode()))
                {
                    DBComponent db = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());

                    List<RoleInfos> roleInfosList = await db.Query<RoleInfos>(role => role.Account.Equals(request.Account) 
                                                    && role.ServerId.Equals(request.ServerId)
                                                    && role.State.Equals((int)RoleState.Normal));

                    if (roleInfosList == null || roleInfosList.Count == 0)
                    {
                        return;
                    }

                    foreach (RoleInfos roleInfos in roleInfosList)
                    {
                        response.RoleInfosList.Add(roleInfos.ToMessage());
                        roleInfos?.Dispose();   //好程序员应该明白为什么要写这个
                    }
                    
                    roleInfosList?.Clear();
                }
            }
        }
    }
}