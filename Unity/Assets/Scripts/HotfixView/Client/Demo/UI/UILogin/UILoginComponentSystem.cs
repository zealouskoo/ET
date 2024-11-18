using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	/// <summary>
	/// 登录面板的 UI 逻辑
	/// </summary>
	[EntitySystemOf(typeof(UILoginComponent))]
	[FriendOf(typeof(UILoginComponent))]
	public static partial class UILoginComponentSystem
	{
		[EntitySystem]
		private static void Awake(this UILoginComponent self)
		{
			// 获取引用的组件
			ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
			// 获取登录按钮对象
			self.loginBtn = rc.Get<GameObject>("LoginBtn");
			
			// 根据对象找到 Button 组件为其添加按钮事件
			self.loginBtn.GetComponent<Button>().onClick.AddListener(()=> { self.OnLogin(); });
			// 获取用户名称输入对象
			self.account = rc.Get<GameObject>("Account");
            // 获取用户密码输入对象
            self.password = rc.Get<GameObject>("Password");
		}

		
		public static void OnLogin(this UILoginComponent self)
		{
			LoginHelper.Login(
				self.Root(), 
				self.account.GetComponent<InputField>().text, 
				self.password.GetComponent<InputField>().text).Coroutine();
		}
	}
}
