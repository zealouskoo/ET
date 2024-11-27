using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class LoginInfoRecordComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<long, int> AccountLoginInfoDictionary = new Dictionary<long, int>();
    }
}