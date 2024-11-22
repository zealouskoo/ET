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
            // 4. 验证请求的用户名密码是否为空，空的话则返回错误码
            if (string.IsNullOrEmpty(request.AccountName) || string.IsNullOrEmpty(request.Password))
            {
                response.Error = ErrorCode.ERR_LoginInfoEmpty;
            }

            /*
             *  5. 正则表达式验证用户名和密码是否合规，不合规则返回错误码
             *       5.1 用户名
             *          5.1.1 长度在6-15之间
             *          5.1.2 不能以数字开头
             *          5.1.3 必须包含一个数字
             *          5.1.4 必须包含一个大写字母
             *          5.1.5 必须包含一个小写字母
             */
            if (!Regex.IsMatch(request.AccountName.Trim(), @"^(?![0-9])(?=.*[0-9].*)(?=.*[A-Z].*)(?=.*[a-z].*).{6,15}$"))
            {
                response.Error = ErrorCode.ERR_AccountNameFormError;
                session.Disconnect().Coroutine();
                return;
            }

            /*
             *  5.2 密码
             *      5.2.1 必须包含一个数字
             *      5.2.2 必须包含一个大写字母
             *      5.2.3 必须包含一个小写字母
             */
            if (Regex.IsMatch(request.Password.Trim(), @"^(A-Za-z0-9)+$"))
            {
                response.Error = ErrorCode.ERR_LoginPasswordError;
                session.Disconnect().Coroutine();
                return;
            }

            // 6. 实现 SessionLockingComponent 组件，防止非客户端的访问

            // 7. 使用 CoroutineLockComponent 组件，并添加协程锁类型 LoginAccount,防止同用户多端同时登录

            CoroutineLockComponent coroutineLockComponent = session.Root().GetComponent<CoroutineLockComponent>();
            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await coroutineLockComponent.Wait(CoroutineLockType.LoginAccount, request.AccountName.GetHashCode()))
                {
                    // 可控原则，所有的实体都得有父实体进行管理，如果实体不在实体层级树中会导致服务器不稳定
                    // 1. 让Scene实体能操作数据库，添加数据库管理组件。
                    // 2. 在 Unity.Model 项目的../Model/Server/Demo/Realm 下新建 Account 实体。 是 Session 的子实体
                    //    2.1   AccountType 类型的枚举
                    //      2.1.1 General 0
                    //      2.1.2 Banned  1
                    //    2.2   string AccountName
                    //    2.3   string Password
                    //    2.4   long   CreateTime
                    //    2.5   int    AccountType
                    // 3. 查询Account信息，结合 excel 修改内容，考虑目前实际操作的库，中间的关系
                    DBComponent db = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
                    List<Account> accountList = await db.Query<Account>(account => account.AccountName.Equals(request.AccountName));

                    Account account = null;
                    // 4. 账号是否存在
                    //      4.1 存在
                    //          4.1.1   账号是否被ban，是则返回错误代码（添加相应错误码），关闭session
                    //          4.1.2   密码是否正确，不正确返回错误代码，关闭session
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
                        //      4.2 不存在     需要新建账号并存储到数据库中，使用AddChild泛型来生成Account对象
                        //          4.2.1   获取服务器时间 TimeInfo.Instance.ServerNow
                        account = session.AddChild<Account>();
                        account.AccountName = request.AccountName.Trim();
                        account.Password    = request.Password;
                        account.CreateTime  = TimeInfo.Instance.ServerNow();
                        account.AccountType = (int)AccountType.General;
                        await db.Save(account);
                    }
                    

                }
            }

            await ETTask.CompletedTask;
        }
    }
}