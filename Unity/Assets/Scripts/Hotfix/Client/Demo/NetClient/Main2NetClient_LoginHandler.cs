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

            R2C_Login r2CLogin;
            using (Session session = await netComponent.CreateRouterSession(realmAddress, account, password))
            {
                C2R_Login c2RLogin = C2R_Login.Create();
                c2RLogin.Account = account;
                c2RLogin.Password = password;
                r2CLogin = (R2C_Login)await session.Call(c2RLogin);
            }

            if (r2CLogin.Error != ErrorCode.ERR_Success) {
                response.Error = r2CLogin.Error;
                return;
            }

            // 创建一个gate Session,并且保存到SessionComponent中
            Session gateSession = await netComponent.CreateRouterSession(NetworkHelper.ToIPEndPoint(r2CLogin.Address), account, password);
            gateSession.AddComponent<ClientSessionErrorComponent>();
            root.AddComponent<SessionComponent>().Session = gateSession;
            C2G_LoginGate c2GLoginGate = C2G_LoginGate.Create();
            c2GLoginGate.Key = r2CLogin.Key;
            c2GLoginGate.GateId = r2CLogin.GateId;
            G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(c2GLoginGate);

            Log.Debug("登陆gate成功!");

            response.PlayerId = g2CLoginGate.PlayerId;
        }
    }
}