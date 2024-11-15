// #define IS_EVENT_ASYNC
using System;
using System.Collections.Generic;
using System.IO;

namespace ET.Client
{
    [Event(SceneType.Main)]
    public class EntryEvent3_InitClient: AEvent<Scene, EntryEvent3>
    {
        protected override async ETTask Run(Scene root, EntryEvent3 args)
        {
            GlobalComponent globalComponent = root.AddComponent<GlobalComponent>();
            root.AddComponent<UIGlobalComponent>();
            root.AddComponent<UIComponent>();
            root.AddComponent<ResourcesLoaderComponent>();
            root.AddComponent<PlayerComponent>();
            root.AddComponent<CurrentScenesComponent>();
            root.AddComponent<ComputersComponent>();
            
            // 根据配置修改掉Main Fiber的SceneType
            SceneType sceneType = EnumHelper.FromString<SceneType>(globalComponent.GlobalConfig.AppType.ToString());
            root.SceneType = sceneType;
            
            await EventSystem.Instance.PublishAsync(root, new AppStartInitFinish());
            /*
#if IS_EVENT_ASYNC
            // 事件抛出的两种形式
            // 1 异步 await EventSystem.Instance.PublishAsync
            //      参数  Scene   Scene的实体，这里应该是 root
            //      参数  Struct  事件的结构体，这里是 TestEventStruct
            await EventSystem.Instance.PublishAsync(root, new TestEventStruct() { TestValue = 10 });
            Log.Debug("After published TestEventStruct");
#else
            // 2 同步  EventSystem.Instance.Publish
            // 注意两个的区别
            // 会直接执行 Log.Debug("After published TestEventStruct");
            // 而不是在等待异步事件执行完再执行。

            EventSystem.Instance.Publish(root, new TestEventStruct() { TestValue = 10 });
            Log.Debug("After published TestEventStruct");
#endif
            */
        }
    }
}