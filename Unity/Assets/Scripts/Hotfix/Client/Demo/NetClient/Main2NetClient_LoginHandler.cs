using System;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    [MessageHandler(SceneType.NetClient)] // 属性标签，要处理的网络消息的 Scene 类型
    public class Main2NetClient_LoginHandler: MessageHandler<Scene, Main2NetClient_Login, NetClient2Main_Login>
    {
        /// <summary>
        /// 把网络消息发送给 NetClient上的实体处理
        /// </summary>
        /// <param name="root">具体处理的实体，是一个 Scene 类型</param>
        /// <param name="request">请求的网络消息类型</param>
        /// <param name="response">请求返回的网络消息类型</param>
        /// <returns></returns>
        protected override async ETTask Run(Scene root, Main2NetClient_Login request, NetClient2Main_Login response)
        {
            string account = request.Account;
            string password = request.Password;
            // 创建一个ETModel层的Session
            root.RemoveComponent<RouterAddressComponent>();
            // 获取路由跟realmDispatcher地址
            RouterAddressComponent routerAddressComponent =
                    root.AddComponent<RouterAddressComponent, string, int>(ConstValue.RouterHttpHost, ConstValue.RouterHttpPort);
            await routerAddressComponent.Init();
            // 用于客户端和服务器之间进行网络连接的组件
            root.AddComponent<NetComponent, AddressFamily, NetworkProtocol>(routerAddressComponent.RouterManagerIPAddress.AddressFamily, NetworkProtocol.UDP);
            root.GetComponent<FiberParentComponent>().ParentFiberId = request.OwnerFiberId;

            NetComponent netComponent = root.GetComponent<NetComponent>();
            
            // 取模获得网关地址
            IPEndPoint realmAddress = routerAddressComponent.GetRealmAddress(account);

            R2C_LoginAccount r2CLogin;
            Session session = await netComponent.CreateRouterSession(realmAddress, account, password);
            C2R_LoginAccount c2RLogin = C2R_LoginAccount.Create();
            c2RLogin.AccountName = account;
            c2RLogin.Password = password;
            r2CLogin = (R2C_LoginAccount)await session.Call(c2RLogin);

            if (r2CLogin.Error == ErrorCode.ERR_Success)
            {
                root.AddComponent<SessionComponent>().Session = session;
            }
            else
            {
                session?.Dispose();
            }
            
            response.Error = r2CLogin.Error;
            response.Token = r2CLogin.Token;

        }
    }
}