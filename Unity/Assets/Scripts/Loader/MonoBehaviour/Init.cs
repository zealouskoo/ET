using System;
using CommandLine;
using UnityEngine;

namespace ET
{
	/// <summary>
	/// The main entry of client.
	/// All start from here.
	/// Entire Unity.Loader solution cannot hotfix, any changes must regenerate HybridCLR's data.
	/// path://Assets/Scripts/Loader/MonoBehaviour/Init.cs
	/// </summary>
	public class Init: MonoBehaviour
	{
		private void Start()
		{
			// this is an async function.
			this.StartAsync().Coroutine();
		}
		
		private async ETTask StartAsync()
		{
			//Don't destroy the gameObject which script attached. 
			DontDestroyOnLoad(gameObject);
			
			//程序集所在作用域的异常回调函数
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Log.Error(e.ExceptionObject.ToString());
			};

			// 命令行参数
			// 游戏客户端是空的，设置在服务器端
			string[] args = "".Split(" ");
			Parser.Default.ParseArguments<Options>(args)
				.WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
				.WithParsed((o)=>World.Instance.AddSingleton(o));
			Options.Instance.StartConfig = $"StartConfig/Localhost";
			
			//日志输出的单例
			World.Instance.AddSingleton<Logger>().Log = new UnityLogger();
			ETTask.ExceptionHandler += Log.Error;
			
			//TimeInfo的单例
			World.Instance.AddSingleton<TimeInfo>();
			//Fiber管理器的单例
			World.Instance.AddSingleton<FiberManager>();

			//创建Resources管理器的单例，并调用 CreatePackageAsync 这个异步方法加载名为 "DefaultPackage" 的资源
			await World.Instance.AddSingleton<ResourcesComponent>().CreatePackageAsync("DefaultPackage", true);
			
			//CodeLoader的单例，加载热更新的代码
			CodeLoader codeLoader = World.Instance.AddSingleton<CodeLoader>();
			//异步下载方法
			await codeLoader.DownloadAsync();
			
			codeLoader.Start();
		}

		private void Update()
		{
			TimeInfo.Instance.Update();
			FiberManager.Instance.Update();
		}

		private void LateUpdate()
		{
			FiberManager.Instance.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			World.Instance.Dispose();
		}
	}
	
	
}