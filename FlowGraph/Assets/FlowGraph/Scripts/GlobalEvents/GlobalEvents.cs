using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif


#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public static class GlobalEvents
{ 
	static GlobalEvents()
	{
		InitializeIfNeeded();
	}

	public class GlobalEventInfo
	{
		public Type type = null;
		public List<EventInfo> eventInfos = new List<EventInfo>();
	}

	private static Dictionary<string, Type> typeNameToTypeMap = new Dictionary<string, Type>();
	private static Dictionary<Type, GlobalEventInfo> typeToGlobalEventInfoMap = new Dictionary<Type, GlobalEventInfo>();
	private static Dictionary<string, Dictionary<string, EventInfo>> typeEventToEventInfoLookup = new Dictionary<string, Dictionary<string, EventInfo>>();

	private static bool initialized = false;

	public static void InitializeIfNeeded()
	{
		if (!initialized)
		{
			CacheGlobalEvents();
			initialized = true;
		}
	}

	public static List<GlobalEventInfo> GetGlobalEventInfos()
	{
		InitializeIfNeeded();
		return typeToGlobalEventInfoMap.Select(kvp => kvp.Value).ToList();
	}

	public static GlobalEventInfo GetGlobalEventInfo(string typeName)
	{
		InitializeIfNeeded();
		typeNameToTypeMap.TryGetValue(typeName, out Type type);
		return GetGlobalEventInfo(type);
	}

	public static GlobalEventInfo GetGlobalEventInfo(Type type)
	{
		InitializeIfNeeded();
		typeToGlobalEventInfoMap.TryGetValue(type, out GlobalEventInfo globalEventInfo);
		return globalEventInfo;
	}

	public static EventInfo GetEventInfo(string typeName, string eventName)
	{
		InitializeIfNeeded();
		if(	typeEventToEventInfoLookup.TryGetValue(typeName, out Dictionary<string, EventInfo> functionToEventMap) &&
			functionToEventMap.TryGetValue(eventName, out EventInfo eventInfo))
		{
			return eventInfo;
		}

		return null;
	}

	public static void CacheGlobalEvents() 
	{
		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (!ReflectionUtils.ProjectAssemblies.Contains(assembly.FullName))
				continue;

			foreach (var type in assembly.GetTypes())
			{
				GlobalEventInfo globalEventInfo = null;

				EventInfo[] events = type.GetEvents(BindingFlags.Static | BindingFlags.Public);
				foreach(var e in events)
				{
					var attrib = e.GetCustomAttribute<GlobalEventAttribute>();
					if (attrib == null)
						continue;

					if (globalEventInfo == null)
					{
						globalEventInfo = new GlobalEventInfo();
						globalEventInfo.type = type;
						typeToGlobalEventInfoMap.Add(type, globalEventInfo);
					}

					globalEventInfo.eventInfos.Add(e);
				}
			}
		}

		foreach (var kvp in typeToGlobalEventInfoMap)
		{
			typeNameToTypeMap.Add(kvp.Key.Name, kvp.Key);

			Dictionary<string, EventInfo> methodNameToInfoMap = new Dictionary<string, EventInfo>();
			foreach (var eventInfo in kvp.Value.eventInfos)
				methodNameToInfoMap.Add(eventInfo.Name, eventInfo);

			typeEventToEventInfoLookup.Add(kvp.Key.Name, methodNameToInfoMap);
		}
	}
}
