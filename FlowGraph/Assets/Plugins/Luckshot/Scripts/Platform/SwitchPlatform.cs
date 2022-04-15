#if UNITY_SWITCH
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nn.fs;
using nn.account;
using UnityEngine.Switch;
using System.Text;

namespace Luckshot.Platform
{
	public enum SwitchResolutionOption
	{
		Default1280x720,
		Low1152x648
	}

	public class SwitchPlatform : Platform
	{
		public override PlatformID GetPlatformID()
		{ return PlatformID.Switch; }

		private Uid userID = new Uid();
		private string mountName = "BigHopsSave";
		private string filePath = "";

		private FileHandle fileHandle = new FileHandle();

		public override Coroutine Initialize()
		{
			Account.Initialize();
			UserHandle userHandle = new UserHandle();

			if (!Account.TryOpenPreselectedUser(ref userHandle))
			{
				Debug.Log("Preselect user: " + userHandle.ToString());
				nn.Nn.Abort("Failed to open preselected user");
			}



			Debug.Log("Opened Preselected User");

			nn.Result result = Account.GetUserId(ref userID, userHandle);
			result.abortUnlessSuccess();

			Debug.Log("Got User ID");

			result = SaveData.Ensure(userID);
			result.abortUnlessSuccess();

			Debug.Log("Ensured Save Data???");

			Debug.Log(mountName);

			result = SaveData.Mount(mountName, userID);
			result.abortUnlessSuccess();

			Debug.Log("Mounted");

			filePath = string.Format("{0}:/{1}", mountName, PlatformServices.GetSaveName());

			SetResolution(SwitchResolutionOption.Default1280x720, 60);

			return base.Initialize();
		}

		public static void SetResolution(SwitchResolutionOption option, int refreshRate = 60)
		{
			int width = 0;
			int height = 0;

			switch(option)
			{
				case SwitchResolutionOption.Default1280x720:
					width = 1280;
					height = 720;
					break;
				case SwitchResolutionOption.Low1152x648:
					width = 1152;
					height = 648;
					break;
			}

			Screen.SetResolution(width, height, true, refreshRate);
		}

		public override void SaveToDisk(string saveText)
		{
			byte[] data = Encoding.ASCII.GetBytes(saveText);

			Notification.EnterExitRequestHandlingSection();

			nn.Result result = File.Delete(filePath);
			if (!FileSystem.ResultPathNotFound.Includes(result))
				result.abortUnlessSuccess();

			result = File.Create(filePath, data.LongLength);
			result.abortUnlessSuccess();

			result = File.Open(ref fileHandle, filePath, OpenFileMode.Write);
			result.abortUnlessSuccess();

			result = File.Write(fileHandle, 0, data, data.LongLength, WriteOption.Flush);
			result.abortUnlessSuccess();

			File.Close(fileHandle);
			result = FileSystem.Commit(mountName);
			result.abortUnlessSuccess();

			Notification.LeaveExitRequestHandlingSection();
		}

		public override string LoadFromDisk()
		{
			EntryType entryType = 0;
			nn.Result result = FileSystem.GetEntryType(ref entryType, filePath);
			if (FileSystem.ResultPathNotFound.Includes(result))
				return string.Empty;

			result.abortUnlessSuccess();

			result = File.Open(ref fileHandle, filePath, OpenFileMode.Read);
			result.abortUnlessSuccess();

			long fileSize = 0;
			result = File.GetSize(ref fileSize, fileHandle);
			result.abortUnlessSuccess();

			byte[] data = new byte[fileSize];
			result = File.Read(fileHandle, 0, data, fileSize);
			result.abortUnlessSuccess();

			File.Close(fileHandle);

			string saveText = Encoding.ASCII.GetString(data);
			return saveText;
		}

		void OnDestroy()
		{
			FileSystem.Unmount(mountName);
		}
	}
}
#endif