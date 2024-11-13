using System.Collections.Generic;
using System.Reflection;
using System;

namespace ET
{
    public class CodeTypes: Singleton<CodeTypes>, ISingletonAwake<Assembly[]>
    {
        private readonly Dictionary<string, Type> allTypes = new();
        private readonly UnOrderMultiMapSet<Type, Type> types = new();
        
        public void Awake(Assembly[] assemblies)
        {
            Dictionary<string, Type> addTypes = AssemblyHelper.GetAssemblyTypes(assemblies);
            foreach ((string fullName, Type type) in addTypes)
            {
                this.allTypes[fullName] = type;
                
                if (type.IsAbstract)
                {
                    continue;
                }
                
                // 记录所有的有BaseAttribute标记的的类型
                object[] objects = type.GetCustomAttributes(typeof(BaseAttribute), true);

                foreach (object o in objects)
                {
                    this.types.Add(o.GetType(), type);
                }
            }
        }

        public HashSet<Type> GetTypes(Type systemAttributeType)
        {
            if (!this.types.ContainsKey(systemAttributeType))
            {
                return new HashSet<Type>();
            }

            return this.types[systemAttributeType];
        }

        public Dictionary<string, Type> GetTypes()
        {
            return allTypes;
        }

        public Type GetType(string typeName)
        {
            return this.allTypes[typeName];
        }
        
        public void CreateCode()
        {
            // 收集有标识为 CodeAttribute 属性标签的类的泛型类型
            // 无法直接显式的进行挂载的原因是这些类都会进行热重载
            var hashSet = this.GetTypes(typeof (CodeAttribute));
            // 通过类型进行遍历
            foreach (Type type in hashSet)
            {
                // 通过其类型来创建出各自的实例
                object obj = Activator.CreateInstance(type);
                // 调用各自的 Awake 函数
                ((ISingletonAwake)obj).Awake();
                // 最终还是通过 World 的 AddSingleton 来添加这些静态类
                World.Instance.AddSingleton((ASingleton)obj);
            }
        }
    }
}