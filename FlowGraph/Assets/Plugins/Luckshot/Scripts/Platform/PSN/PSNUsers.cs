#pragma warning disable 0414

#if UNITY_PS4

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PSNUsers
{
	string remoteOnlineID = "Q-ZLqkCtBK-GB-EN";
	byte[] remoteNpID = null;
#if UNITY_PS4
	string [] sUserColors = { "BLUE", "RED", "GREEN", "PINK" };
#endif

	public PSNUsers()
	{
		Initialize();
	}

	public void Initialize()
	{
		Sony.NP.User.OnGotUserProfile += OnUserGotProfile;
		Sony.NP.User.OnGotRemoteUserNpID += OnGotRemoteUserNpID;
		Sony.NP.User.OnGotRemoteUserProfile += OnGotRemoteUserProfile;
		Sony.NP.User.OnUserProfileError += OnUserProfileError;

		//Sony.NP.User.
	}

// 	public void MenuUser(MenuStack menuStack)
// 	{
// 		bool signedIn = Sony.NP.User.IsSignedInPSN;
// 
// 		if (menuUser.AddItem("Get My Profile", !Sony.NP.User.IsUserProfileBusy))
// 		{
// 			// NOTE: Sony.NP.User.RequestUserProfile() should succeed even if Sony.NP.User.SignIn() has not
// 			// been executed as long as the user is already signed in on the Vita.
// 			Sony.NP.ErrorCode res = Sony.NP.User.RequestUserProfile();
// 			if (res != Sony.NP.ErrorCode.NP_OK)
// 			{
// 				// Failed, most likely the user is not signed in.
// 				Sony.NP.ResultCode result = new Sony.NP.ResultCode();
// 				Sony.NP.User.GetLastUserProfileError(out result);
// 				OnScreenLog.Add(result.className + ": " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
// 			}
// 		}
// 
// 		if (menuUser.AddItem("Get Remote Profile (onlineID)", signedIn && !Sony.NP.User.IsUserProfileBusy))
// 		{
// 			// Lookup another users profile from their onlineID.
// 			//
// 			// NOTE for PS3 & PSVita: If you already know the users npID then calling RequestRemoteUserProfileForNpID() is
// 			// much faster as only one network request is required, calling RequestRemoteUserProfileForOnlineID()
// 			// performs two requests, the 1st to lookup the npID from the onlineID and the 2nd to retrieve
// 			// the users profile. There is no penalty when using this on PS4.
// 			//
// 			Sony.NP.ErrorCode res = Sony.NP.User.RequestRemoteUserProfileForOnlineID(remoteOnlineID);
// 			if (res != Sony.NP.ErrorCode.NP_OK)
// 			{
// 				Sony.NP.ResultCode result = new Sony.NP.ResultCode();
// 				Sony.NP.User.GetLastUserProfileError(out result);
// 				OnScreenLog.Add(result.className + ": " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
// 			}
// 		}
// 
// 		if (menuUser.AddItem("Get Remote NpID", signedIn && !Sony.NP.User.IsUserProfileBusy))
// 		{
// 			// Lookup another users NpID from their online ID.
// 			Sony.NP.ErrorCode res = Sony.NP.User.RequestRemoteUserNpID(remoteOnlineID);
// 			if (res != Sony.NP.ErrorCode.NP_OK)
// 			{
// 				Sony.NP.ResultCode result = new Sony.NP.ResultCode();
// 				Sony.NP.User.GetLastUserProfileError(out result);
// 				OnScreenLog.Add(result.className + ": " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
// 			}
// 		}
// 
// 		if (menuUser.AddItem("Get Remote Profile (npID)", (remoteNpID != null) && signedIn && !Sony.NP.User.IsUserProfileBusy))
// 		{
// 			// Lookup another users profile from their npID.
// 			Sony.NP.ErrorCode res = Sony.NP.User.RequestRemoteUserProfileForNpID(remoteNpID);
// 			if (res != Sony.NP.ErrorCode.NP_OK)
// 			{
// 				Sony.NP.ResultCode result = new Sony.NP.ResultCode();
// 				Sony.NP.User.GetLastUserProfileError(out result);
// 				OnScreenLog.Add(result.className + ": " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
// 			}
// 		}
// 
// 		if (menuUser.AddItem("Get local users status & NPIDs"))
// 		{
// 			// Lookup up local users
// 			for (int slot = 0; slot < 4; slot++)
// 			{
// 				UnityEngine.PS4.PS4Input.LoggedInUser userdata = UnityEngine.PS4.PS4Input.PadGetUsersDetails( slot );
// 				byte[] npid;
// 				int status = Sony.NP.User.GetUserSigninStatus( userdata.userId , out npid);
// 				if (userdata.userId != -1)
// 				{
// 					OnScreenLog.Add(String.Format(" slot:{0} userid:0x{1:x} status:{2} color:{3} username:{4}{5} npid:{6}" , 
// 							slot, 
// 							userdata.userId, 
// 							status==2?"SIGNED into PSN":"NOT SIGNED INTO PSN" , 
// 							sUserColors[userdata.color],
// 							userdata.userName,
// 							userdata.primaryUser ? "(Primary User)" : "",
// 							System.Text.Encoding.Default.GetString(npid) )	);
// 				}
// 			}
// 		}		
// 		if (menuUser.AddItem("Use slot 0 for all NPToolkit functions"))
// 		{
// 			int playerSlot = 0;		// user slot 0
// 			UnityEngine.PS4.PS4Input.LoggedInUser userDetails = UnityEngine.PS4.PS4Input.PadGetUsersDetails(playerSlot);
// 			if (userDetails.status != 0 )
// 			{
// 				OnScreenLog.Add(String.Format("Using slot {0} (userId:0x{1:x}) for all default functions ", playerSlot, userDetails.userId ));
// 				Sony.NP.User.SetCurrentUserId(userDetails.userId);	// set the userId that we want to register the trophy pack with
// 			}
// 			else
// 			{
// 				OnScreenLog.Add("no player signed in in slot " + playerSlot );
// 			}	
// 		}		
// 		if (menuUser.AddItem("Use slot 1 for all NPToolkit functions"))
// 		{
// 			int playerSlot = 1;		// user slot 1
// 			UnityEngine.PS4.PS4Input.LoggedInUser userDetails = UnityEngine.PS4.PS4Input.PadGetUsersDetails(playerSlot);
// 			if (userDetails.status != 0 )
// 			{
// 				OnScreenLog.Add(String.Format("Using slot {0} (userId:0x{1:x}) for all default functions ", playerSlot, userDetails.userId ));
// 				Sony.NP.User.SetCurrentUserId(userDetails.userId);	// set the userId that we want to register the trophy pack with
// 			}
// 			else
// 			{
// 				OnScreenLog.Add("no player signed in in slot " + playerSlot );
// 			}	
// 		}		
// 		if (menuUser.AddItem("Use slot 2 for all NPToolkit functions"))
// 		{
// 			int playerSlot = 2;		// user slot 2
// 			UnityEngine.PS4.PS4Input.LoggedInUser userDetails = UnityEngine.PS4.PS4Input.PadGetUsersDetails(playerSlot);
// 			if (userDetails.status != 0 )
// 			{
// 				OnScreenLog.Add(String.Format("Using slot {0} (userId:0x{1:x}) for all default functions ", playerSlot, userDetails.userId ));
// 				Sony.NP.User.SetCurrentUserId(userDetails.userId);	// set the userId that we want to register the trophy pack with
// 			}
// 			else
// 			{
// 				OnScreenLog.Add("no player signed in in slot " + playerSlot );
// 			}	
// 		}
// 		if (menuUser.AddItem("Use slot 3 for all NPToolkit functions"))
// 		{
// 			int playerSlot = 3;		// user slot 3
// 			UnityEngine.PS4.PS4Input.LoggedInUser userDetails = UnityEngine.PS4.PS4Input.PadGetUsersDetails(playerSlot);
// 			if (userDetails.status != 0 )
// 			{
// 				OnScreenLog.Add(String.Format("Using slot {0} (userId:0x{1:x}) for all default functions ", playerSlot, userDetails.userId ));
// 				Sony.NP.User.SetCurrentUserId(userDetails.userId);	// set the userId that we want to register the trophy pack with
// 			}
// 			else
// 			{
// 				OnScreenLog.Add("no player signed in in slot " + playerSlot );
// 			}	
// 		}		
// 	}

	void OnUserGotProfile(Sony.NP.Messages.PluginMessage msg)
	{
		Sony.NP.User.UserProfile profile = Sony.NP.User.GetCachedUserProfile();
		OnScreenLog.Add(msg.ToString());
		OnScreenLog.Add(" OnlineID: " + profile.onlineID);

		string npID = System.Text.Encoding.Default.GetString(profile.npID);
		OnScreenLog.Add(" NpID: " + npID);

		OnScreenLog.Add(" Avatar URL: " + profile.avatarURL);
		OnScreenLog.Add(" Country Code: " + profile.countryCode);
		OnScreenLog.Add(" Language: " + profile.language);
		OnScreenLog.Add(" Age: " + profile.age);
		OnScreenLog.Add(" Chat Restrict: " + profile.chatRestricted);
		OnScreenLog.Add(" Content Restrict: " + profile.contentRestricted);

		// only valid on PS4
		OnScreenLog.Add(" FirstName: " + profile.firstName);
		OnScreenLog.Add(" MiddleName: " + profile.middleName);
		OnScreenLog.Add(" LastName: " + profile.lastName);
		OnScreenLog.Add(" ProfilePictureUrl: " + profile.profilePictureUrl);
		OnScreenLog.Add(" AccountId: 0x" + profile.npAccountId.ToString("X"));		

		// Download and display the avatar image.
		SonyNpMain.SetAvatarURL(profile.avatarURL, 0);
	}

	void OnGotRemoteUserNpID(Sony.NP.Messages.PluginMessage msg)
	{
		remoteNpID = Sony.NP.User.GetCachedRemoteUserNpID();

		string npID = System.Text.Encoding.Default.GetString(remoteNpID);
		OnScreenLog.Add("Got Remote User NpID: " + npID);
	}

	void OnGotRemoteUserProfile(Sony.NP.Messages.PluginMessage msg)
	{
		Sony.NP.User.RemoteUserProfile profile = Sony.NP.User.GetCachedRemoteUserProfile();
		OnScreenLog.Add("Got Remote User Profile");
		OnScreenLog.Add(" OnlineID: " + profile.onlineID);
		string npID = System.Text.Encoding.Default.GetString(profile.npID);
		OnScreenLog.Add(" NpID: " + npID);
		OnScreenLog.Add(" Avatar URL: " + profile.avatarURL);
		OnScreenLog.Add(" Country Code: " + profile.countryCode);
		OnScreenLog.Add(" Language: " + profile.language);
	
		// only valid on PS4
		OnScreenLog.Add(" FirstName: " + profile.firstName);
		OnScreenLog.Add(" MiddleName: " + profile.middleName);
		OnScreenLog.Add(" LastName: " + profile.lastName);
		OnScreenLog.Add(" ProfilePictureUrl: " + profile.profilePictureUrl);

		// Download and display the avatar image.
		SonyNpMain.SetAvatarURL(profile.avatarURL, 1);
	}

	void OnUserProfileError(Sony.NP.Messages.PluginMessage msg)
	{
		Sony.NP.ResultCode result = new Sony.NP.ResultCode();
		Sony.NP.User.GetLastUserProfileError(out result);
		OnScreenLog.Add(result.className + ": " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
	}
}

#endif