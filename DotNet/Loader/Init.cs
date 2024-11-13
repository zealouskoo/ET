using System;
using System.Threading;
using CommandLine;

namespace ET
{
	public class Init
	{
		public void Start()
		{
			try
			{
				// 设置当前程序集作用域的异常回调处理函数
				AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
				{
					Log.Error(e.ExceptionObject.ToString());
				};
				
				// 服务器端命令行参数处理
				Parser.Default.ParseArguments<Options>(System.Environment.GetCommandLineArgs())
						.WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
						.WithParsed((o)=>World.Instance.AddSingleton(o));
				// 与客户端一样的处理方式
				World.Instance.AddSingleton<Logger>().Log = new NLogger(Options.Instance.AppType.ToString(), Options.Instance.Process, 0);
				
				// 与客户端一样的处理方式
				ETTask.ExceptionHandler += Log.Error;
				// 与客户端一样的处理方式
				World.Instance.AddSingleton<TimeInfo>();
				// 与客户端一样的处理方式
				World.Instance.AddSingleton<FiberManager>();
				
				//此处与客户端不一样了
				World.Instance.AddSingleton<CodeLoader>();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public void Update()
		{
			TimeInfo.Instance.Update();
			FiberManager.Instance.Update();
		}

		public void LateUpdate()
		{
			FiberManager.Instance.LateUpdate();
		}
	}
}
