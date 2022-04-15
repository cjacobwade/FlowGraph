using UnityEngine;
using System.Collections;

namespace Luckshot.Platform
{
	public enum PlatformID
	{
		None = 0,

		Steam = 1,
		DRMFree = 2,
		PS4 = 3,
		XboxOne = 4,
		Switch = 5
	}

	public class PlatformServices : Singleton<PlatformServices>
	{
		private Platform currentPlatform = null;
		public static Platform CurrentPlatform
		{ get { return Instance.currentPlatform; } }

		public static PlatformID GetPlatformID()
		{
			if (CurrentPlatform != null)
				return CurrentPlatform.GetPlatformID();

			return BigHopsPrefs.Instance.FakePlatformID;
		}

		static readonly int saveVersion = 0;

		public static string GetSaveName()
		{ return "/hopsave_v" + saveVersion; }

		public static string GetSaveLocation()
		{ return Application.persistentDataPath + GetSaveName() + ".hop"; }

		public Coroutine Initialize()
		{ return StartCoroutine(InitializeRoutine()); }

		private IEnumerator InitializeRoutine()
		{
#if UNITY_EDITOR || UNITY_STANDALONE
			currentPlatform = gameObject.AddComponent<DRMFreePlatform>();
#elif UNITY_PS4
			currentPlatform = gameObject.AddComponent<PSNPlatform>();
#elif UNITY_SWITCH
			currentPlatform = gameObject.AddComponent<SwitchPlatform>();
#endif

			yield return currentPlatform.Initialize();

			if (BigHopsPrefs.Instance.AutoLoadOnStart)
			{
				if (!SaveManager.Instance.LoadFromDisk())
				{
					SaveManager.Instance.SaveToDisk();
					SaveManager.Instance.LoadFromDisk();
				}
			}
		}

		public static void SetPlatform(Platform platform)
		{ Instance.currentPlatform = platform; }
	}

}