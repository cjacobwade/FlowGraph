using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class AnimationUtils
{
	public static void AssignMeshToSkinnedRenderer(SkinnedMeshRenderer skinnedRenderer, Mesh mesh)
	{
#if UNITY_EDITOR
		string path = AssetDatabase.GetAssetPath(mesh);

		GameObject rigPrefab = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
		var rigSkinnedRenderers = rigPrefab.GetComponentsInChildren<SkinnedMeshRenderer>();

		SkinnedMeshRenderer matchingRigSkin = null;
		foreach (var rigSkinnedRenderer in rigSkinnedRenderers)
		{
			if (rigSkinnedRenderer.sharedMesh == mesh)
			{
				matchingRigSkin = rigSkinnedRenderer;
				break;
			}
		}

		Transform remappedRoot = skinnedRenderer.transform.parent.FindRecursive(matchingRigSkin.rootBone.name);

		Transform[] remappedBones = new Transform[matchingRigSkin.bones.Length];
		for (int i = 0; i < remappedBones.Length; i++)
		{
			string boneName = matchingRigSkin.bones[i].name;
			remappedBones[i] = remappedRoot.FindRecursive(boneName);
		}

		skinnedRenderer.rootBone = remappedRoot;
		skinnedRenderer.bones = remappedBones;

		Bounds bounds = PhysicsUtils.CalculateRenderersBounds(new Renderer[]{ skinnedRenderer}, remappedRoot);
		skinnedRenderer.localBounds = bounds;
#endif
		skinnedRenderer.sharedMesh = mesh;
	}

	public static void GetBindPoseBonePositionRotation(Matrix4x4 skinMatrix, Matrix4x4 boneMatrix, 
		Transform bone, out Vector3 position, out Quaternion rotation)
	{
		// Get global matrix for bone
		Matrix4x4 bindMatrixGlobal = skinMatrix * boneMatrix.inverse;

		// Get local X, Y, Z, and position of matrix
		Vector3 mX = new Vector3(bindMatrixGlobal.m00, bindMatrixGlobal.m10, bindMatrixGlobal.m20);
		Vector3 mY = new Vector3(bindMatrixGlobal.m01, bindMatrixGlobal.m11, bindMatrixGlobal.m21);
		Vector3 mZ = new Vector3(bindMatrixGlobal.m02, bindMatrixGlobal.m12, bindMatrixGlobal.m22);
		Vector3 mP = new Vector3(bindMatrixGlobal.m03, bindMatrixGlobal.m13, bindMatrixGlobal.m23);

		// Set position
		// Adjust scale of matrix to compensate for difference in binding scale and model scale
		float bindScale = mZ.magnitude;
		float modelScale = Mathf.Abs(bone.lossyScale.z);
		position = mP * (modelScale / bindScale);

		// Set rotation
		// Check if scaling is negative and handle accordingly
		if (Vector3.Dot(Vector3.Cross(mX, mY), mZ) >= 0)
			rotation = Quaternion.LookRotation(mZ, mY);
		else
			rotation = Quaternion.LookRotation(-mZ, -mY);
	}

	public static void PlayForwards(this Animation animation, float? normalizedTime = null)
	{
		AnimationState state = animation[animation.clip.name];

		if (normalizedTime.HasValue)
			state.normalizedTime = normalizedTime.Value;
		else if (!state.enabled)
			state.normalizedTime = 0f;

		state.speed = 1f;
		state.weight = 1f;
		state.enabled = true;
	}

	public static void PlayBackwards(this Animation animation, float? normalizedTime = null)
	{
		AnimationState state = animation[animation.clip.name];

		if (normalizedTime.HasValue)
			state.normalizedTime = normalizedTime.Value;
		else if (!state.enabled)
			state.normalizedTime = 1f;

		state.speed = -1f;
		state.weight = 1f;
		state.enabled = true;
	}

	public static void SampleAnimation(this Animation animation, float normalizedTime)
	{
		AnimationState state = animation[animation.clip.name];

		bool wasEnabled = state.enabled;

		state.enabled = true;
		state.normalizedTime = normalizedTime;
		state.speed = 1f;
		state.weight = 1f;

		animation.Sample();

		state.enabled = wasEnabled;
	}
}
