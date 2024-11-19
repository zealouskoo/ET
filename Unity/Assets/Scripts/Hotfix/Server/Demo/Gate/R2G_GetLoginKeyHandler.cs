using System;


namespace ET.Server
{
	/// <summary>
	/// 网络消息的编写
	/// 1. 首先在相应的 proto 文件（位于Assets/Config/Proto/）添加消息结构，
	/// 2. 在 Hotfix 项目的相应文件夹内添加处理模块
	/// 3. 需要继承自 MessageHandler 这个基类
	/// </summary>
	
	[MessageHandler(SceneType.Gate)]
	public class R2G_GetLoginKeyHandler : MessageHandler<Scene, R2G_GetLoginKey, G2R_GetLoginKey>
	{
		protected override async ETTask Run(Scene scene, R2G_GetLoginKey request, G2R_GetLoginKey response)
		{
			long key = RandomGenerator.RandInt64();
			scene.GetComponent<GateSessionKeyComponent>().Add(key, request.Account);
			response.Key = key;
			response.GateId = scene.Id;
			await ETTask.CompletedTask;
		}
	}
}