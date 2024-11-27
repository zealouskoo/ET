using System.Collections.Generic;

namespace ET.Server
{
    [EntitySystemOf(typeof(LoginInfoRecordComponent))]
    [FriendOfAttribute(typeof(ET.Server.LoginInfoRecordComponent))]
    public static partial class LoginInfoRecordComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.LoginInfoRecordComponent self)
        {

        }
        
        [EntitySystem]
        private static void Destroy(this ET.Server.LoginInfoRecordComponent self)
        {
            self.AccountLoginInfoDictionary.Clear();
        }

        public static void Add(this ET.Server.LoginInfoRecordComponent self, long key, int value)
        {
            if (!self.AccountLoginInfoDictionary.TryAdd(key, value))
            {
                self.AccountLoginInfoDictionary[key] = value;
                return;
            }
        }

        public static void Remove(this ET.Server.LoginInfoRecordComponent self, long key)
        {
            if (self.AccountLoginInfoDictionary.ContainsKey(key))
            {
                self.AccountLoginInfoDictionary.Remove(key);
            }
        }

        public static int Get(this ET.Server.LoginInfoRecordComponent self, long key)
        {
            int value = self.AccountLoginInfoDictionary.GetValueOrDefault(key, -1);

            return value;
        }

        public static bool IsExist(this ET.Server.LoginInfoRecordComponent self, long key)
        {
            return self.AccountLoginInfoDictionary.ContainsKey(key);
        }
    }
}