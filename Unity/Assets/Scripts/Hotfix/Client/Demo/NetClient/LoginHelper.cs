namespace ET.Client
{
    /// <summary>
    /// 虽说这个类是一个 ET.Client 的类，但是机器人会使用这个类来模拟登录操作，且并没有使用 Unity3D 的类库
    /// 所以需要保证逻辑层代码能在服务器运行
    /// </summary>
    public static class LoginHelper
    {
        /// <summary>
        /// 异步的登录函数
        /// </summary>
        /// <param name="root">Scene的实体，来自主纤程，游戏客户端的第一个纤程，要能分清 Scene 实体来自于哪个 Fiber</param>
        /// <param name="account">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public static async ETTask Login(Scene root, string account, string password)
        {
            // 移除并添加 ClientSenderComponent 组件是为了保证组件是全新的
            // 得保证和之前的登录没有任何的关系
            root.RemoveComponent<ClientSenderComponent>();
            
            ClientSenderComponent clientSenderComponent = root.AddComponent<ClientSenderComponent>();

            // 此时的 session 还没有建立，无法直接使用 ClientSenderComponent 的 call 命令来发送
            NetClient2Main_Login response = await clientSenderComponent.LoginAsync(account, password);

            if (response.Error != ErrorCode.ERR_Success) {
                Log.Error($"请求登录失败,返回错误代码:{response.Error}.");
                return;
            }
            Log.Debug("请求登录成功！！！");
            string Token = response.Token;
            
            // 获取服务器列表
            C2R_GetServerInfos c2RGetServerInfos = C2R_GetServerInfos.Create();
            c2RGetServerInfos.Account = account;
            c2RGetServerInfos.Token = Token;
            R2C_GetServerInfos r2CGetServerInfos = await clientSenderComponent.Call(c2RGetServerInfos) as R2C_GetServerInfos;

            if (r2CGetServerInfos.Error != ErrorCode.ERR_Success)
            {
                Log.Error("请求网关服务器列表错误！");
                return;
            }
            
            // 获取区服角色列表
            
            // 将得到的 playerId 记录到 PlayerComponent 组件上
            root.GetComponent<PlayerComponent>().MyId = response.PlayerId;

            // 公布登录结束 LoginFinish 事件
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
        }
    }
}