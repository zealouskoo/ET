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

            NetClient2Main_Login response = await clientSenderComponent.LoginAsync(account, password);

            if (response.Error != ErrorCode.ERR_Success) {
                Log.Error($"Error:{response.Error}.");
                return;
            }


            // 将得到的 playerId 记录到 PlayerComponent 组件上
            root.GetComponent<PlayerComponent>().MyId = response.PlayerId;

            // 公布登录结束 LoginFinish 事件
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
        }
    }
}