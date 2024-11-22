using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class AccountSessionComponent : Entity, IAwake, IDestroy
    {
        // 用户名和 Session 一一对应
        public Dictionary<string, EntityRef<Session>> AccountSessionDictionary = new ();
    }
}