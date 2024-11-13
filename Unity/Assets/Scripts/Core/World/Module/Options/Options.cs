using CommandLine;
using System;
using System.Collections.Generic;

namespace ET
{
    public enum AppType
    {
        Server,
        Watcher, // 每台物理机一个守护进程，用来启动该物理机上的所有进程
        GameTool,
        ExcelExporter,
        Proto2CS,
        BenchmarkClient,
        BenchmarkServer,
        
        Demo,
        LockStep,
    }
    
    /// <summary>
    /// 要使用的命令行参数必须在此定义，否则会报错。
    /// </summary>
    public class Options: Singleton<Options>
    {
        /// <summary>
        /// 服务器端应用类型
        /// </summary>
        [Option("AppType", Required = false, Default = AppType.Server, HelpText = "AppType enum")]
        public AppType AppType { get; set; }

        /// <summary>
        /// 启服配置
        /// </summary>
        [Option("StartConfig", Required = false, Default = "StartConfig/Localhost")]
        public string StartConfig { get; set; }

        /// <summary>
        /// 进程 ID
        /// </summary>
        [Option("Process", Required = false, Default = 1)]
        public int Process { get; set; }
        
        /// <summary>
        /// 是否在开发模式下
        /// </summary>
        [Option("Develop", Required = false, Default = 0, HelpText = "develop mode, 0正式 1开发 2压测")]
        public int Develop { get; set; }

        /// <summary>
        /// 输出日志等级
        /// </summary>
        [Option("LogLevel", Required = false, Default = 0)]
        public int LogLevel { get; set; }
        
        /// <summary>
        /// 是否启用输入输出流进行交流
        /// </summary>
        [Option("Console", Required = false, Default = 0)]
        public int Console { get; set; }
    }
}