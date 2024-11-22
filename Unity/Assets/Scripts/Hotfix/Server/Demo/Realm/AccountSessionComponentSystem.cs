namespace ET.Server
{
    [EntitySystemOf(typeof(AccountSessionComponent))]
    [FriendOfAttribute(typeof(ET.Server.AccountSessionComponent))]
    public static partial class AccountSessionComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.AccountSessionComponent self)
        {

        }
        [EntitySystem]
        private static void Destroy(this ET.Server.AccountSessionComponent self)
        {
            self.AccountSessionDictionary.Clear();
        }

        public static Session Get(this ET.Server.AccountSessionComponent self, string accountName)
        {
            if (!self.AccountSessionDictionary.TryGetValue(accountName, out EntityRef<Session> session))
            {
                return null;
            }
            return session;
        }

        public static void Add(this ET.Server.AccountSessionComponent self, string accountName, EntityRef<Session> session)
        {
            if (!self.AccountSessionDictionary.TryAdd(accountName, session))
            {
                self.AccountSessionDictionary[accountName] = session;
                return;
            }
        }

        public static void Remove(this ET.Server.AccountSessionComponent self, string accountName)
        {
            if (self.AccountSessionDictionary.ContainsKey(accountName))
            {
                self.AccountSessionDictionary.Remove(accountName);
            }
        }
    }
}