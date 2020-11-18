using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Text;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[InitializeOnLoad]
public static class FlowTypeCache
{
	static FlowTypeCache()
	{
		initialized = false;
	}

	// Iterate over child classes of FlowModule
	// Cache all functions with FlowEffectInstance as first arg

	// Iterate over child classes of PropertyItem
	// Cache all static functions (can't do instance functions because we don't have ref)
	// Maybe cache all functions in case some effects want to execute functions on instance?

	public class ModuleInfo
	{
		public TypeInfo typeInfo = null;
		public List<MethodInfo> methodInfos = new List<MethodInfo>();
	}

	public class PropertyItemInfo
	{
		public TypeInfo typeInfo = null;
		public List<MethodInfo> staticMethodInfos = new List<MethodInfo>();
		public List<MethodInfo> instanceMethodInfos = new List<MethodInfo>();
	}

	private static readonly List<Type> acceptedParameterTypes = new List<Type>()
	{
		typeof(bool),
		typeof(int),
		typeof(float),
		typeof(string),
		typeof(Color),
		typeof(Vector2),
		typeof(Vector3),
		typeof(Vector4),
		typeof(UnityEngine.Object),
		typeof(UnityEngine.Object[])
	};

	private static readonly List<string> acceptedAssemblies = new List<string>()
	{	
		"Assembly-CSharp-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
		"Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
		"Assembly-CSharp-Editor-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
		"Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
	};

	private static List<Assembly> assemblies = new List<Assembly>();

	private static Dictionary<Type, ModuleInfo> moduleTypeToInfoMap = new Dictionary<Type, ModuleInfo>();
	private static Dictionary<Type, PropertyItemInfo> propertyItemTypeToInfoMap = new Dictionary<Type, PropertyItemInfo>();

	private static Dictionary<string, Type> moduleNameToTypeMap = new Dictionary<string, Type>();
	private static Dictionary<string, Type> propertyNameToTypeMap = new Dictionary<string, Type>();

	private static Dictionary<string, Dictionary<string, MethodInfo>> moduleFunctionToMethodInfoLookup = new Dictionary<string, Dictionary<string, MethodInfo>>();
	private static Dictionary<string, Dictionary<string, MethodInfo>> propertyNameToInstanceMethodNameMap = new Dictionary<string, Dictionary<string, MethodInfo>>();
	private static Dictionary<string, Dictionary<string, MethodInfo>> propertyNameToStaticMethodNameMap = new Dictionary<string, Dictionary<string, MethodInfo>>();

	private static bool initialized = false;

#if UNITY_EDITOR
	[MenuItem("Luckshot/Cache Type Info")]
#endif
	public static void CacheTypeInfo()
	{
		Clear();

		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (acceptedAssemblies.Contains(assembly.FullName))
				assemblies.Add(assembly);
		}

		CollectFlowModuleInfo();
		//CollectPropertyItemInfo();

		initialized = true;
	}

	private static void InitializeIfNeeded()
	{
		if (!initialized)
			CacheTypeInfo();
	}

	public static ModuleFunction GetDefaultModuleFunction()
	{
		var moduleFunction = new ModuleFunction();

		foreach (var kvp in moduleTypeToInfoMap)
		{
			moduleFunction.module = kvp.Key.Name;
			moduleFunction.function = kvp.Value.methodInfos[0].Name;
			break;
		}

		return moduleFunction;
	}

	public static List<ModuleInfo> GetModuleInfos()
	{
		InitializeIfNeeded();
		return moduleTypeToInfoMap.Select(kvp => kvp.Value).ToList(); 
	}

	public static ModuleInfo GetModuleInfo(Type type)
	{
		InitializeIfNeeded();
		moduleTypeToInfoMap.TryGetValue(type, out ModuleInfo info);
		return info;
	}

	public static List<PropertyItemInfo> GetPropertyItemInfos()
	{
		InitializeIfNeeded();
		return propertyItemTypeToInfoMap.Select(kvp => kvp.Value).ToList(); }

	public static PropertyItemInfo GetPropertyItemInfo(Type type)
	{
		InitializeIfNeeded();
		propertyItemTypeToInfoMap.TryGetValue(type, out PropertyItemInfo info);
		return info;
	}

	public static Type GetModuleType(string module)
	{
		InitializeIfNeeded();
		moduleNameToTypeMap.TryGetValue(module, out Type type);
		return type;
	}

	public static MethodInfo GetModuleFunction(string module, string function)
	{
		InitializeIfNeeded();
		moduleFunctionToMethodInfoLookup.TryGetValue(module, out Dictionary<string, MethodInfo> moduleFunctionsMap);
		if(moduleFunctionsMap != null)
		{
			moduleFunctionsMap.TryGetValue(function, out MethodInfo methodInfo);
			if (methodInfo != null)
				return methodInfo;
		}

		return null;
	}

	private static void Clear()
	{
		assemblies.Clear();

		moduleTypeToInfoMap.Clear();
		moduleFunctionToMethodInfoLookup.Clear();
		moduleNameToTypeMap.Clear();

		propertyItemTypeToInfoMap.Clear();
		propertyNameToInstanceMethodNameMap.Clear();
		propertyNameToStaticMethodNameMap.Clear();
		propertyNameToTypeMap.Clear();
	}

	private static void CollectFlowModuleInfo()
	{
		int numTypes = 0;

		// Note: pulling from just FlowModule's assembly isn't going to get all the right types
		foreach (var assembly in assemblies)
		{
			var assemblyTypes = assembly.GetTypes();
			foreach (var type in assemblyTypes)
			{
#if UNITY_EDITOR
				EditorUtility.DisplayProgressBar("Cacheing Type Info", "Collecting Flow Modules", numTypes / (float)assemblyTypes.Length);
#endif

				if (type.IsSubclassOf(typeof(FlowModule)))
				{
					ModuleInfo moduleInfo = null;

					var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
					foreach (var method in methods)
					{
						if (method.IsSpecialName) // dont want properties
							continue;

						var parameters = method.GetParameters();
						if (parameters != null && parameters.Length >= 1 &&
							parameters[0].ParameterType == typeof(FlowEffectInstance))
						{
							if (moduleInfo == null)
								moduleInfo = new ModuleInfo();

							moduleInfo.methodInfos.Add(method);
						}
					}

					if (moduleInfo != null)
					{
						moduleInfo.typeInfo = type.GetTypeInfo();
						moduleTypeToInfoMap.Add(type, moduleInfo);
					}
				}

				numTypes++;
			}
		}

		foreach(var kvp in moduleTypeToInfoMap)
		{
			moduleNameToTypeMap.Add(kvp.Key.Name, kvp.Key);

			Dictionary<string, MethodInfo> methodNameToInfoMap = new Dictionary<string, MethodInfo>();

			foreach(var method in kvp.Value.methodInfos)
				methodNameToInfoMap.Add(method.Name, method);

			moduleFunctionToMethodInfoLookup.Add(kvp.Key.Name, methodNameToInfoMap);
		}

#if UNITY_EDITOR
		EditorUtility.ClearProgressBar();
#endif
	}

	/*
	private static void CollectPropertyItemInfo()
	{
		int numTypes = 0;

		// Note: pulling from just PropertyItem's assembly isn't going to get all the right types
		foreach (var assembly in assemblies)
		{
			var assemblyTypes = assembly.GetTypes();
			foreach (var type in assemblyTypes)
			{
#if UNITY_EDITOR
				EditorUtility.DisplayProgressBar("Cacheing Type Info", "Collecting PropertyItems", numTypes / (float)assemblyTypes.Length);
#endif

				if (type.IsSubclassOf(typeof(PropertyItem)))
				{
					PropertyItemInfo propertyItemInfo = null;

					// Iterate up the type hierarchy to collect all methods through inheritance
					// but only up to propertyItem

					var typeIter = type;
					while (typeIter != null && (typeIter == typeof(PropertyItem) || typeIter.IsSubclassOf(typeof(PropertyItem))))
					{
						var staticMethods = typeIter.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
						foreach (var staticMethod in staticMethods)
						{
							if (staticMethod.IsSpecialName) // don't want properties
								continue;

							var parameters = staticMethod.GetParameters();
							if (parameters != null && parameters.Length >= 1 &&
								parameters[0].ParameterType == typeof(Item))
							{
								if (propertyItemInfo == null)
									propertyItemInfo = new PropertyItemInfo();

								propertyItemInfo.staticMethodInfos.Add(staticMethod);
							}
						}

						var instanceMethods = typeIter.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.GetField);
						foreach (var instanceMethod in instanceMethods)
						{
							if (instanceMethod.IsSpecialName) // don't want properties
								continue;

							bool validParams = true;

							foreach (var paramater in instanceMethod.GetParameters())
							{
								if (!IsValidArgumentType(paramater.ParameterType))
								{
									validParams = false;
									break;
								}
							}

							if (validParams)
							{
								if (propertyItemInfo == null)
									propertyItemInfo = new PropertyItemInfo();

								propertyItemInfo.instanceMethodInfos.Add(instanceMethod);
							}
						}

						typeIter = typeIter.BaseType;
					}

					if (propertyItemInfo != null)
					{
						propertyItemInfo.typeInfo = type.GetTypeInfo();
						propertyItemTypeToInfoMap.Add(type, propertyItemInfo);
					}
				}

				numTypes++;
			}
		}

#if UNITY_EDITOR
		EditorUtility.ClearProgressBar();
#endif
	}
	*/

	private static bool IsValidArgumentType(Type type)
	{
		foreach(var validType in acceptedParameterTypes)
		{
			if (type == validType || type.IsSubclassOf(validType))
				return true;
		}

		return false;
	}
}