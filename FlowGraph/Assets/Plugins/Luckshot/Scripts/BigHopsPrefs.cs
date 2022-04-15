using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using NaughtyAttributes;
using UnityEngine.SceneManagement;

using PlatformID = Luckshot.Platform.PlatformID;

public enum BuildMode
{
	Development = 0,
	Release = 1,
	Demo = 2
}

public enum ProgressMode
{
	Freeplay = 0,
	Story = 1
}

[CreateAssetMenu(menuName = "Big Hops/Big Hops Prefs")]
public class BigHopsPrefs : ScriptableObjectSingleton<BigHopsPrefs>
{
	[Header("Platform")]

	public BuildMode BuildMode = BuildMode.Development;
	public PlatformID FakePlatformID = PlatformID.None;

	[Header("Progress")]

	public ProgressMode ProgressMode = ProgressMode.Freeplay;

	[Header("Saves")]

	public bool AutoLoadOnStart = false;
	public bool AutoSaveOnQuit = false;

	[Header("Camera")]

	public bool InvertCameraY = false;
	public bool CameraYAssist = true;

	[Header("Gameplay")]

	public bool ProSwimming = true;
	public bool ShowNewItemText = true;

	[Header("Audio")]

	public bool PlayMusic = true;
	public bool PlaySFX = true;

	[Header("Debug")]

	public bool DebugBreakLB = true;
	public bool InfiniteStamina = false;
	public bool VisualizeCollision = false;

	[Scene]
	public string LoadScene = string.Empty;

	public static event Action PrefsChanged = delegate {};

	public void NotifyChanged()
	{
		PrefsChanged();
	}

	#region INI Config
	private IniParser iniParser = new IniParser();
	private const string configName = "BigHopsv1"; // When releasing versions with new save data, tick this

	private static BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;

	bool IsTypeStringArray(Type type)
	{ return type == typeof(string[]) || type == typeof(List<string>); }

	bool IsFieldValid(FieldInfo inPropertyInfo)
	{
		var type = inPropertyInfo.FieldType;

		// No collection types
		if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string) && !IsTypeStringArray(type))
			return false;

		// No struct types
		if (type.IsValueType && !type.IsPrimitive && !type.IsEnum)
			return false;

		// No classe types
		if (type.IsClass && type != typeof(string) && !IsTypeStringArray(type))
			return false;

		return true;
	}

	public void LoadConfig()
	{
		var fieldInfos = typeof(BigHopsPrefs).GetFields(bindingFlags);

		bool fileExists = iniParser.DoesExist(configName);
		if (fileExists)
		{
			iniParser.Load(configName);

			for (int i = 0; i < fieldInfos.Length; i++)
			{
				if (!IsFieldValid(fieldInfos[i]))
					continue;

				Type type = fieldInfos[i].FieldType;
				string propName = fieldInfos[i].Name;

				bool? boolVal = null;
				iniParser.TryGetBool(propName, ref boolVal);
				if (boolVal.HasValue)
				{
					fieldInfos[i].SetValue(Instance, boolVal.Value);
				}
				else
				{
					int? intVal = null;
					iniParser.TryGetInt(propName, ref intVal);
					if (intVal.HasValue)
					{
						fieldInfos[i].SetValue(Instance, intVal.Value);
					}
					else
					{
						float? floatVal = null;
						iniParser.TryGetFloat(propName, ref floatVal);
						if (floatVal.HasValue)
						{
							fieldInfos[i].SetValue(Instance, floatVal.Value);
						}
						else if (iniParser.HasKey(propName))
						{
							string stringVal = iniParser.Get(propName);
							if (type.IsEnum)
							{
								fieldInfos[i].SetValue(Instance, Enum.Parse(type, stringVal));
							}
							else if (IsTypeStringArray(type))
							{
								string[] stringArrValues = iniParser.Get(propName).Split(',');
								fieldInfos[i].SetValue(Instance, stringArrValues);
							}
							else
							{
								fieldInfos[i].SetValue(Instance, iniParser.Get(propName));
							}
						}
					}
				}
			}
		}

		// Fill in any missing values
		bool anyAdditions = false;
		for (int i = 0; i < fieldInfos.Length; i++)
		{
			if (!IsFieldValid(fieldInfos[i]))
				continue;

			if (!iniParser.HasKey(fieldInfos[i].Name))
			{
				var type = fieldInfos[i].FieldType;
				if (!IsTypeStringArray(type))
				{
					var propertyVal = fieldInfos[i].GetValue(Instance);
					iniParser.Set(fieldInfos[i].Name, propertyVal.ToString());
				}
				else
				{
					var stringArr = (string[])fieldInfos[i].GetValue(Instance);
					string combinedString = "";
					for (int j = 0; j < stringArr.Length; j++)
					{
						combinedString += stringArr[j];
						if (j < stringArr.Length - 1)
							combinedString += ",";
					}

					iniParser.Set(fieldInfos[i].Name, combinedString);
				}

				anyAdditions = true;
			}
		}

		if(!fileExists || anyAdditions)
		{
			iniParser.Save(configName);
			LoadConfig();
		}
	}
	#endregion // INI Config
}
