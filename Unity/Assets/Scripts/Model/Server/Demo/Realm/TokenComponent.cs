using System.Collections.Generic;

namespace ET.Server
{

    [ComponentOf(typeof(Scene))]
    public class TokenComponent : Entity, IAwake
    {
        public readonly Dictionary<string, string> AccountTokenDictionary = new Dictionary<string, string>();
    }
}