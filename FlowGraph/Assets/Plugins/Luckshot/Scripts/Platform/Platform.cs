#pragma warning disable 0414

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Luckshot.Platform
{
	public abstract class Platform : MonoBehaviour
	{
		public abstract PlatformID GetPlatformID();

		public virtual bool IsInitialized()
		{ return true; }

		public virtual Coroutine Initialize()
		{ return StartCoroutine(InitializeRoutine()); }

		protected virtual IEnumerator InitializeRoutine()
		{ yield break; }

		public virtual void SaveToDisk(string text)
		{
			Debug.Log("Saving to " + PlatformServices.GetSaveLocation());
		}

		public virtual string LoadFromDisk()
		{
			Debug.Log("Loading " + PlatformServices.GetSaveLocation());
			return string.Empty;
		}

		#region Users
		public class User
		{
			public int controllerID;
			public string username;
			public Texture2D userIcon;
		}

		private List<User> users = new List<User>();

		public virtual string GetCurrentUserName()
		{ return "Player"; }

		public virtual string GetBuildID()
		{ return "UnknownPlatformBuildID"; }
		#endregion

		// LEADERBOARDS
	}
}