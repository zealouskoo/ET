namespace ET.Client
{
    /// <summary>
    /// 此组件是用来帮助游戏客户端的第一个 Fiber、MainFiber 和游戏服务器端建立连接
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class ClientSenderComponent: Entity, IAwake, IDestroy
    {
        public int fiberId;

        public ActorId netClientActorId;
    }
}