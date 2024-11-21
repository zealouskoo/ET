using System.Text.RegularExpressions;

namespace ET.Server.Handler
{
    // 从游戏客户端发送消息，通过 router 节点连接到 realm 服务器上
    // 消息需要交由 Realm 场景进行处理
    [MessageSessionHandler(SceneType.Realm)]
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
                    // wait to implement
                }
            }
            
            await ETTask.CompletedTask;
        }
    }
}