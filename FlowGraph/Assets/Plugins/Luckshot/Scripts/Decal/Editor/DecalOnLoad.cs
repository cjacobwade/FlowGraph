using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class DecalOnLoad
{
	static DecalOnLoad()
	{
		InitializeDecals();

		EditorSceneManager.sceneOpened -= EditorSceneManager_SceneOpened;
		EditorSceneManager.sceneOpened += EditorSceneManager_SceneOpened;

		EditorApplication.playModeStateChanged -= EditorApplication_PlayModeStateChanged;
		EditorApplication.playModeStateChanged += EditorApplication_PlayModeStateChanged;
	}

	private static void EditorApplication_PlayModeStateChanged(PlayModeStateChange obj)
	{
		if (obj == PlayModeStateChange.EnteredEditMode)
			InitializeDecals();
	}

	private static void EditorSceneManager_SceneOpened(Scene scene, OpenSceneMode mode)
	{
		InitializeDecals();
	}

	private static void InitializeDecals()
	{
		foreach (var decal in MonoBehaviour.FindObjectsOfType<Decal>())
			decal.RegenerateIfNeeded();
	}
}
