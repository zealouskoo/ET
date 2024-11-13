using MemoryPack;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace ET
{
    public struct EntryEvent1
    {
    }   
    
    public struct EntryEvent2
    {
    } 
    
    public struct EntryEvent3
    {
    }
    
    public static class Entry
    {
        //空函数，为了初始化静态类
        public static void Init()
        {
            
        }
        
        // 游戏客户端和服务器端共同的一个程序入口
        public static void Start()
        {
            StartAsync().Coroutine();
        }
        
        private static async ETTask StartAsync()
        {
            WinPeriod.Init();

            // 注册Mongo type
            MongoRegister.Init();
            // 注册Entity序列化器
            EntitySerializeRegister.Init();
            World.Instance.AddSingleton<IdGenerater>();
            World.Instance.AddSingleton<OpcodeType>();
            World.Instance.AddSingleton<ObjectPool>();
            World.Instance.AddSingleton<MessageQueue>();
            World.Instance.AddSingleton<NetServices>();
            World.Instance.AddSingleton<NavmeshComponent>();
            World.Instance.AddSingleton<LogMsg>();
            
            // 创建需要reload的code singleton
            CodeTypes.Instance.CreateCode();
            
            await World.Instance.AddSingleton<ConfigLoader>().LoadAsync();

            // 用于创建管理 Fiber
            await FiberManager.Instance.Create(SchedulerType.Main, ConstFiberId.Main, 0, SceneType.Main, "");
        }
    }
}