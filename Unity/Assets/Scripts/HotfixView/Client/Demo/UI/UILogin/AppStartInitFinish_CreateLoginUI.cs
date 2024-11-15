namespace ET.Client
{
	[Event(SceneType.Demo)]
	public class AppStartInitFinish_CreateLoginUI: AEvent<Scene, AppStartInitFinish>
	{
		protected override async ETTask Run(Scene root, AppStartInitFinish args)
		{
			await UIHelper.Create(root, UIType.UILogin, UILayer.Mid);

			// ==================== Test custom componet and system
			//Computer computer = root.GetComponent<ComputersComponent>().AddChild<Computer>();

			//computer.AddComponent<PCCaseComponent>();
			//MonitorComponent monitor = computer.AddComponent<MonitorComponent, int>(10);

			//computer.Open();
			//monitor.ChangeBrightness(5);

			//await root.GetComponent<TimerComponent>().WaitAsync(3000);

			//Scene scene = computer.Root();
			//Log.Debug($"Component root instanceId:{scene.InstanceId}	Scene root instanceId:{root.InstanceId}");

			//computer?.Dispose();
			
			// ======================= Test Async
			//Log.Debug("Start test");

			//await TestAsync(root);
			//int testValue = await TestAsync2(root);
			//Log.Debug($"testValue: {testValue}");

			//Log.Debug("End test");
		}

		//private async ETTask TestAsync(Scene s) {
		//	Log.Debug("Enter TestAsync");
		//	await s.GetComponent<TimerComponent>().WaitAsync(3000);
  //          Log.Debug("Leave TestAsync");
  //      }

		//private async ETTask<int> TestAsync2(Scene s) {
		//	Log.Debug("Enter TestAsync2");
		//	await s.GetComponent<TimerComponent>().WaitAsync(3000);
  //          Log.Debug("Leave TestAsync2");

		//	return 10;
  //      }
	}
}
