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

	public partial class PlatformServices : Singleton<PlatformServices>
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

		public static string GetSaveLocation()
		{ return Application.dataPath + "/save.hop"; }

		public Coroutine Initialize()
		{ return StartCoroutine(InitializeRoutine()); }

		private IEnumerator InitializeRoutine()
		{
#if UNITY_EDITOR || UNITY_STANDALONE
			if (BigHopsPrefs.Instance.FakePlatformID != PlatformID.Steam)
				currentPlatform = gameObject.AddComponent<DRMFreePlatform>();
//			else
//				currentPlatform = gameObject.AddComponent<SteamPlatform>();
#elif UNITY_PS4
		currentPlatform = gameObject.AddComponent<PSNPlatform>();
#elif UNITY_SWITCH
		currentPlatform = gameObject.AddComponent<SwitchPlatform>();
#endif

			yield return currentPlatform.Initialize();

			if (BigHopsPrefs.Instance.AutoLoadOnStart)
			{
				if (!SaveManager.Instance.Load())
				{
					SaveManager.Instance.Save();
					SaveManager.Instance.Load();
				}
			}
		}

		public static void SetPlatform(Platform platform)
		{ Instance.currentPlatform = platform; }
	}

}