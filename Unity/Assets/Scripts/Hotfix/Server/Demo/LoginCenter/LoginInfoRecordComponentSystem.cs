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

        /// <summary>
        /// 添加账号和Zone字典项
        /// </summary>
        /// <param name="self">扩展方法的对象</param>
        /// <param name="key">Account</param>
        /// <param name="value">Zone/ServerId</param>
        public static void Add(this ET.Server.LoginInfoRecordComponent self, long key, int value)
        {
            if (!self.AccountLoginInfoDictionary.TryAdd(key, value))
            {
                self.AccountLoginInfoDictionary[key] = value;
                return;
            }
        }

        /// <summary>
        /// 移除对应账号的字典项
        /// </summary>
        /// <param name="self">扩展方法的对象</param>
        /// <param name="key"></param>
        public static void Remove(this ET.Server.LoginInfoRecordComponent self, long key)
        {
            if (self.AccountLoginInfoDictionary.ContainsKey(key))
            {
                self.AccountLoginInfoDictionary.Remove(key);
            }
        }

        /// <summary>
        /// 返回Zone/ServerId
        /// </summary>
        /// <param name="self">扩展方法的对象</param>
        /// <param name="key">Account name long</param>
        /// <returns></returns>
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