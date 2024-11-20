namespace ET.Client
{
    public static partial class SceneChangeHelper
    {
        /// <summary>
        /// 场景切换协程
        /// </summary>
        /// <param name="root">Scene 实体</param>
        /// <param name="sceneName">Scene 名字</param>
        /// <param name="sceneInstanceId">Scene ID</param>
        public static async ETTask SceneChangeTo(Scene root, string sceneName, long sceneInstanceId)
        {
            root.RemoveComponent<AIComponent>();
            
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            currentScenesComponent.Scene?.Dispose(); // 删除之前的CurrentScene，创建新的
            Scene currentScene = CurrentSceneFactory.Create(sceneInstanceId, sceneName, currentScenesComponent);
            UnitComponent unitComponent = currentScene.AddComponent<UnitComponent>();
         
            // 可以订阅这个事件中创建Loading界面
            EventSystem.Instance.Publish(root, new SceneChangeStart());
            // 等待CreateMyUnit的消息
            // 在下面这个消息中通知 CreateMyUnit 的消息
            // M2C_CreateMyUnitHandler(Assets/Scripts/Hotfix/Client/Demo/Main/Unit/M2C_CreateMyUnitHandler.cs)
            Wait_CreateMyUnit waitCreateMyUnit = await root.GetComponent<ObjectWait>().Wait<Wait_CreateMyUnit>();
            M2C_CreateMyUnit m2CCreateMyUnit = waitCreateMyUnit.Message;
            // 这个 UnitFactory 是客户端的，和服务器端的完全不一样
            Unit unit = UnitFactory.Create(currentScene, m2CCreateMyUnit.Unit);
            unitComponent.Add(unit); // 此行目前是多余的
            root.RemoveComponent<AIComponent>();
            
            // 发布事件
            EventSystem.Instance.Publish(currentScene, new SceneChangeFinish());
            // 通知等待场景切换的协程
            // EnterMapAsync(Assets/Scripts/Hotfix/Client/Demo/Main/Login/EnterMapHelper.cs)
            root.GetComponent<ObjectWait>().Notify(new Wait_SceneChangeFinish());
        }
    }
}