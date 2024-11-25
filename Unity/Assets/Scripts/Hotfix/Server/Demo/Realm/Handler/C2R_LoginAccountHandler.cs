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
                    
                    /*
                     * 1. 声明一条网络信息（R2L_LoginAccountRequest），用于自 Realm 向 LoginCenter 请求登录信息
                     *  考虑下 R2L_LoginAccountRequest 这条信息会申明在哪个 proto 文件内。
                     * Realm to LoginCenter 是服务器内部的消息，所以在 InnerMessage.proto 中
                     */
                    R2L_LoginAccountRequest r2LLoginAccount = R2L_LoginAccountRequest.Create();
                     /* 2. 使用 StartSceneConfigCategory 获取 LoginCenterConfig，在 StartSceneConfig 内 */
                     StartSceneConfig loginCenterConfig = StartSceneConfigCategory.Instance.LoginCenterConfig;
                     
                     /* 3. 使用 MessageSender 组件向 LoginCenter 服务器发送 r2LLoginAccount，
                      *  发送的 ActorId 在loginCenterConfig中，并回传 L2R_LoginAccountRequest 消息 */
                      L2R_LoginAccountRequest l2RLoginAccount = 
                              await session.Fiber().Root.GetComponent<MessageSender>().Call(loginCenterConfig.ActorId, r2LLoginAccount) 
                                      as L2R_LoginAccountRequest;
                     
                      /* 4. 检查下回传的消息是否为 ERR_Success，不是的话就返回错误代码，结束 session 并终止后续的逻辑
                      *  任何可能造成内存泄露的情况都要处理，本例中 account 本不需要手动释放，但是养成好习惯还是写上释放语句。*/
                      if (!l2RLoginAccount.Error.Equals(ErrorCode.ERR_Success))
                      {
                          response.Error = l2RLoginAccount.Error;
                          session.Disconnect().Coroutine();
                          account?.Dispose();
                          return;
                      }
                      /* 5. 编写 AccountSessionComponent 组件，并在 Realm 的实体上添加此组件 
                      
                      * 6. 检查之前是否有其他的同名登录过了，AccountSessionComponent 里有没有存储过同名的 Session
                      *    假如玩家 A 先登录之后，玩家 B 也用同样用户名和密码登录，业务流程应该是在 B 登录时，
                      *    通知 A 玩家有其他玩家在其他的地方登录了他的账号，并将 A 的连接关闭。 */
                      Session otherSession = session.Root().GetComponent<AccountSessionComponent>().Get(request.AccountName);
                      otherSession?.Send(A2C_Disconnect.Create());
                      otherSession?.Disconnect().Coroutine();
                      /* 7. 之后用 AccountSessionComponent 记录下玩家的用户名和 Session */
                      session.Root().GetComponent<AccountSessionComponent>().Add(request.AccountName, session);
                      /* 8. 编写 AccountCheckOutTimeComponent 组件，并添加到 session 上进行管理 */
                      session.AddComponent<AccountCheckOutTimeComponent, string>(request.AccountName);
                      /* 9. 生成 Token 字符串，服务器当前时间 + 随机数字 */
                      string Token = TimeInfo.Instance.ServerNow().ToString() + RandomGenerator.RandomNumber(int.MinValue, int.MaxValue);
                      /* 10. 删除之前 TokenComponent 组件内的 AccountName 值（如果有的话） */
                      session.Root().GetComponent<TokenComponent>().Remove(request.AccountName);
                      /* 11. 把刚刚生成的 Token 值写入到 TokenComponent 组件中 */
                      session.Root().GetComponent<TokenComponent>().Add(request.AccountName, Token);
                      /* 12. 把 Token 写入到返回信息中 */
                      response.Token = Token;
                      /* 13. 记得释放 account */
                      account?.Dispose();
                }
            }

            await ETTask.CompletedTask;
        }
    }
}