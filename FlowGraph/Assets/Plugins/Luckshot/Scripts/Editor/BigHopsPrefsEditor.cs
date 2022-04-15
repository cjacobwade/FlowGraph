using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using PlatformID = Luckshot.Platform.PlatformID;

[CustomEditor(typeof(BigHopsPrefs))]
public class BigHopsPrefsEditor : Editor
{
	private BigHopsPrefs Prefs => target as BigHopsPrefs;

	[MenuItem("Big Hops/Open Big Hops Prefs &y")]
	private static void OpenBigHopsPrefs()
	{
		Selection.activeObject = BigHopsPrefs.Instance;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if(GUI.changed)
		{
			Prefs.NotifyChanged();
		}

		if(GUILayout.Button("Load Scene"))
		{
			QuickLoadScene();
		}

		if(GUILayout.Button("Steam Release"))
		{
			SetSteam();
		}

		if(GUILayout.Button("DRM Free Release"))
		{
			SetDRMFree();
		}
	}

	private void QuickLoadScene()
	{
		if (!string.IsNullOrEmpty(Prefs.LoadScene))
		{
			string sceneName = Prefs.LoadScene + ".unity";

			EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
			for (int i = 0; i < scenes.Length; i++)
			{
				if (scenes[i].path.Contains(sceneName))
				{
					EditorSceneManager.OpenScene(scenes[i].path, OpenSceneMode.Single);
					return;
				}
			}
		}
	}

	private void SetSteam()
	{
		Prefs.FakePlatformID = PlatformID.Steam;
		SetRelease();
	}

	private void SetDRMFree()
	{
		Prefs.FakePlatformID = PlatformID.DRMFree;
		SetRelease();
	}

	private void SetRelease()
	{
		Prefs.BuildMode = BuildMode.Release;

		Prefs.AutoSaveOnQuit = true;
		Prefs.AutoLoadOnStart = true;

		Prefs.InfiniteStamina = false;
		Prefs.VisualizeCollision = false;

		Prefs.PlayMusic = true;
		Prefs.PlaySFX = true;
	}
}
