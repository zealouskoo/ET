namespace ET.Server
{
    [EntitySystemOf(typeof(TokenComponent))]
    [FriendOfAttribute(typeof(ET.Server.TokenComponent))]
    public static partial class TokenComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.TokenComponent self)
        {

        }

        public static void Add(this ET.Server.TokenComponent self, string key, string token)
        {
            self.AccountTokenDictionary.Add(key, token);
            self.TimeOutRemoveKey(key, token).Coroutine();
        }

        public static string Get(this ET.Server.TokenComponent self, string key)
        {
            string token = string.Empty;
            self.AccountTokenDictionary.TryGetValue(key, out token);
            return token;
        }

        public static void Remove(this ET.Server.TokenComponent self, string key)
        {
            if (self.AccountTokenDictionary.ContainsKey(key))
            {
                self.AccountTokenDictionary.Remove(key);
            }
        }

        private static async ETTask TimeOutRemoveKey(this ET.Server.TokenComponent self, string key, string tokenKey)
        {
            await self.Root().GetComponent<TimerComponent>().WaitAsync(600000);
            string onlineToken = self.Get(key);
            if (string.IsNullOrEmpty(onlineToken) && onlineToken.Equals(tokenKey))
            {
                self.Remove(key);
            }
        }
    }
}