using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace Luckshot.Platform
{
	public class DRMFreePlatform : Platform
	{
		public override PlatformID GetPlatformID()
		{
			PlatformID platformID = PlatformID.DRMFree;

			if (BigHopsPrefs.Instance.BuildMode != BuildMode.Release &&
				BigHopsPrefs.Instance.FakePlatformID != PlatformID.None)
			{
				platformID = BigHopsPrefs.Instance.FakePlatformID;
			}

			return platformID;
		}

		protected override IEnumerator InitializeRoutine()
		{
			// TODO: Hook up in-game achievement shower
			yield break;
		}

		public override void SaveToDisk(string saveText)
		{
			string saveLocation = PlatformServices.GetSaveLocation();
			if (File.Exists(saveLocation))
				File.Delete(saveLocation);

			File.WriteAllText(saveLocation, saveText);
		}

		public override string LoadFromDisk()
		{
			string saveLocation = PlatformServices.GetSaveLocation();
			if (!File.Exists(saveLocation))
				return null;

			string loadText = File.ReadAllText(saveLocation);
			return loadText;
		}
	}
}