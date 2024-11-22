using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ET.Server.Handler
{
    // 从游戏客户端发送消息，通过 router 节点连接到 realm 服务器上
    // 消息需要交由 Realm 场景进行处理
    [MessageSessionHandler(SceneType.Realm)]
    [FriendOfAttribute(typeof(ET.Server.Account))]
    public class C2R_LoginAccountHandler : MessageSessionHandler<C2R_LoginAccount, R2C_LoginAccount>
    {
        protected override async ETTask Run(Session session, C2R_LoginAccount request, R2C_LoginAccount response)
        {
            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            if (session.GetComponent<SessionLockingComponent>() != null)
            {
                // 产生了重复的请求
                response.Error = ErrorCode.ERR_LoginRepeatedly;
                session.Disconnect().Coroutine();
                return;
            }
            if (string.IsNullOrEmpty(request.AccountName) || string.IsNullOrEmpty(request.Password))
            {
                response.Error = ErrorCode.ERR_LoginInfoEmpty;
            }

            if (!Regex.IsMatch(request.AccountName.Trim(), @"^(?![0-9])(?=.*[0-9].*)(?=.*[A-Z].*)(?=.*[a-z].*).{6,15}$"))
            {
                response.Error = ErrorCode.ERR_AccountNameFormError;
                session.Disconnect().Coroutine();
                return;
            }
            if (Regex.IsMatch(request.Password.Trim(), @"^(A-Za-z0-9)+$"))
            {
                response.Error = ErrorCode.ERR_LoginPasswordError;
                session.Disconnect().Coroutine();
                return;
            }

            CoroutineLockComponent coroutineLockComponent = session.Root().GetComponent<CoroutineLockComponent>();
            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await coroutineLockComponent.Wait(CoroutineLockType.LoginAccount, request.AccountName.GetHashCode()))
                {
                    // 可控原则，所有的实体都得有父实体进行管理，如果实体不在实体层级树中会导致服务器不稳定
                 
                    DBComponent db = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
                    List<Account> accountList = await db.Query<Account>(account => account.AccountName.Equals(request.AccountName));

                    Account account = null;
                    
                    if (accountList != null && accountList.Count > 0)
                    {
                        account = accountList[0];
                        if (account.AccountType == (int)AccountType.Banned)
                        {
                            response.Error = ErrorCode.ERR_AccountBanned;
                            session.Disconnect().Coroutine();
                            return;
                        }

                        if (!account.Password.Equals(request.Password))
                        {
                            response.Error = ErrorCode.ERR_LoginPasswordError;
                            session.Disconnect().Coroutine();
                            return;
                        }
                    }
                    else
                    {
                        account = session.AddChild<Account>();
                        account.AccountName = request.AccountName.Trim();
                        account.Password    = request.Password;
                        account.CreateTime  = TimeInfo.Instance.ServerNow();
                        account.AccountType = (int)AccountType.General;
                        await db.Save(account);
                    }

                    R2L_LoginAccountRequest r2LLoginAccount = R2L_LoginAccountRequest.Create();
                    
                    /*
                     * 1. 声明一条网络信息（R2L_LoginAccountRequest），用于自 Realm 向 LoginCenter 请求登录信息
                     *  考虑下 R2L_LoginAccountRequest 这条信息会申明在哪个 proto 文件内。
                     * 2. 使用 StartSceneConfigCategory 获取 LoginCenterConfig，在 StartSceneConfig 内
                     * 3. 使用 MessageSender 组件向 LoginCenter 服务器发送 r2LLoginAccount，
                     *  发送的 ActorId 在loginCenterConfig中，并回传 L2R_LoginAccountRequest 消息
                     * 4. 检查下回传的消息是否为 ERR_Success，不是的话就返回错误代码，结束 session 并终止后续的逻辑
                     *  任何可能造成内存泄露的情况都要处理，本例中 account 本不需要手动释放，但是养成好习惯还是写上释放语句。
                     * 5. 编写 AccountSessionComponent 组件，并在 Realm 的实体上添加此组件
                     * 6. 检查之前是否有其他的同名登录过了，
                     *    假如玩家 A 先登录之后，玩家 B 也用同样用户名和密码登录，业务流程应该是在 B 登录时，
                     *    通知 A 玩家有其他玩家在其他的地方登录了他的账号，并将 A 的连接关闭。
                     * 7. 之后用 AccountSessionComponent 记录下玩家的用户名和 Session
                     * 8. 编写 AccountCheckOutTimeComponent 组件，并添加到 session 上进行管理
                     */
                }
            }

            await ETTask.CompletedTask;
        }
    }
}