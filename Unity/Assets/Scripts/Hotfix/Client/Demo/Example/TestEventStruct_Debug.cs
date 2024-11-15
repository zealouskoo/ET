
namespace ET.Client {
    [Event(SceneType.Demo)]
    public class TestEventStruct_Debug : AEvent<Scene, TestEventStruct> {
        protected override async ETTask Run(Scene scene, TestEventStruct a) {

            // 调取 scene 实体的 TimerComponent 组件进行等待
            await scene.GetComponent<TimerComponent>().WaitAsync(2000);
            // 2秒后输出 TestValue 值
            Log.Debug(a.TestValue.ToString());
            await ETTask.CompletedTask;
        }
    }
}
