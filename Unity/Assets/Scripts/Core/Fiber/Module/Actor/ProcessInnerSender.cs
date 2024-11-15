using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 用于两个纤程间进行通信
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class ProcessInnerSender: Entity, IAwake, IDestroy, IUpdate
    {
        public const long TIMEOUT_TIME = 40 * 1000;
        
        public int RpcId;

        public readonly Dictionary<int, MessageSenderStruct> requestCallback = new();
        
        public readonly List<MessageInfo> list = new();
    }
}