using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Luckshot.PathShape;

[InitializeOnLoad]
public static class PathMeshOnLoad
{
	static PathMeshOnLoad()
	{
		EditorSceneManager.sceneOpened -= EditorSceneManager_SceneOpened;
		EditorSceneManager.sceneOpened += EditorSceneManager_SceneOpened;

		if (!EditorApplication.isPlayingOrWillChangePlaymode)
			RegenerateAll();
	}

	private static void EditorSceneManager_SceneOpened(Scene scene, OpenSceneMode mode)
	{
		RegenerateAll();
	}

	private static void RegenerateAll()
	{
		foreach (var pathMesh in MonoBehaviour.FindObjectsOfType<PathMesh>())
			pathMesh.RegenerateIfNeeded();
	}
}
