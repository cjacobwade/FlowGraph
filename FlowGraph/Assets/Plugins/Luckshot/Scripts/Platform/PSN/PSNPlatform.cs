#if UNITY_PS4 && !UNITY_EDITOR
using UnityEngine;
using System.Collections;
using InControl;
using LinqTools;

namespace Luckshot.Platform
{
	public class PSNPlatform : Platform 
	{
		bool npReady = false;		// Is the NP plugin initialised and ready for use.

	#region Achievements
		bool _isAttemptingToAwardTrophy = false;
		bool _successfullyAwardedTrophy = false;
	#endregion

	#region Cloud Saves
		string virtualUserOnlineID = "_ERGVirtualUser1";

		// TUS request states.
		enum TUSDataRequestType
		{
			None,
			SaveRawData,
			LoadRawData,
			SavePlayerPrefs,
			LoadPlayerPrefs,
		}
		TUSDataRequestType m_TUSDataRequestType = TUSDataRequestType.None;
	#endregion

		SonyNpUser user = null;
		SonyNpFriends friends = null;
		SonyNpTrophy trophies = null;
		SonyNpRanking ranking = null;

		//SonyNpMessaging messaging; // Used for sending messages with data from the game, maybe useful for Daily Challenges?
		//SonyNpCloud cloudStorage = null;
		SonyNpUtilities utilities = null;
		SonyNpRequests requests = null;

		public override PlatformID GetPlatformID()
		{ return PlatformID.PS4; }

		public override bool SupportsAchievements()
		{ return true; }

		protected override IEnumerator InitializeRoutine()
		{
			Sony.NP.Main.OnNPInitialized += (msg) => npReady = true;

	#if UNITY_PS4
			UnityEngine.PS4.PS4Input.OnUserServiceEvent = ((uint eventtype, uint userid) => 
			{
				int SCE_USER_SERVICE_EVENT_TYPE_LOGOUT = 1;
				if (eventtype == SCE_USER_SERVICE_EVENT_TYPE_LOGOUT)
					Sony.NP.User.LogOutUser((int)userid);
			} );
	#endif
		
			// Enable/Disable internal logging, log messages are handled by the OnLog, OnLogWarning and OnLogError event handlers.
			Sony.NP.Main.enableInternalLogging = true;

			// Add NP event handlers.
			Sony.NP.Main.OnLog += OnLog;
			Sony.NP.Main.OnLogWarning += OnLogWarning;
			Sony.NP.Main.OnLogError += OnLogError;

	#if NPTOOLKIT_OVERRIDE_AGE_RATING
			// You can override the age rating like this...
			Sony.NP.Main.InitializeWithNpAgeRating(npCreationFlagss, 12);
	#else
			// Initialise with trophy registration and the age rating that was set in the editor player settings...
			Sony.NP.Main.Initialize(Sony.NP.Main.kNpToolkitCreate_CacheTrophyIcons);
	#endif

			// System events.
	// 		Sony.NP.System.OnConnectionUp += OnSomeEvent;
	// 		Sony.NP.System.OnConnectionDown += OnConnectionDown;
	// 		Sony.NP.System.OnSysResume += OnSomeEvent;
	// 		Sony.NP.System.OnSysNpMessageArrived += OnSomeEvent;
	// 		Sony.NP.System.OnSysStorePurchase += OnSomeEvent;
	// 		Sony.NP.System.OnSysStoreRedemption += OnSomeEvent;
	// 		Sony.NP.System.OnSysEvent += OnSomeEvent;	// Some other event.

	#region Achievement Events
			// Probably don't need to use these callbacks, but good to know about
	// 		Sony.NP.Trophies.OnGotGameInfo += OnTrophyGotGameInfo;
	// 		Sony.NP.Trophies.OnGotGroupInfo += OnTrophyGotGroupInfo;
	//		Sony.NP.Trophies.OnGotTrophyInfo += OnTrophyGotTrophyInfo;
	// 		Sony.NP.Trophies.OnGotProgress += OnTrophyGotProgress;
			Sony.NP.Trophies.OnGotTrophyInfo += OnTrophyGotTrophyInfo;
 			Sony.NP.Trophies.OnAwardedTrophy += OnAwardTrophySuccess;
			Sony.NP.Trophies.OnAwardTrophyFailed += OnAwardTrophyFail;
 			Sony.NP.Trophies.OnAlreadyAwardedTrophy += OnAwardTrophyFail;
	// 		Sony.NP.Trophies.OnUnlockedPlatinum += OnPlatinumUnlocked;
	#endregion

	#region Cloud Save Events
			// TODO: Should only need to use set and request here!
			// Title Small Storage (TSS)
			Sony.NP.TusTss.OnTssDataRecieved += OnGotTssData;
			Sony.NP.TusTss.OnTssNoData += OnSomeEvent;
			Sony.NP.TusTss.OnTusTssError += OnLogError;

			// Title User Storage (TUS)
	// 		Sony.NP.TusTss.OnTusDataSet += OnSetTusData;
	// 		Sony.NP.TusTss.OnTusDataRecieved += OnGotTusData;
			Sony.NP.TusTss.OnTusVariablesSet += OnSetTusVariables;
	// 		Sony.NP.TusTss.OnTusVariablesModified += OnModifiedTusVariables;
			Sony.NP.TusTss.OnTusVariablesRecieved += OnGotTusVariables;
			Sony.NP.TusTss.OnTusTssError += OnLogError;
	#endregion

			// Messaging events.
			//Sony.NP.Messaging.OnSessionInviteMessageRetrieved += OnMessagingSessionInviteRetrieved;
			//Sony.NP.Messaging.OnMessageSessionInviteReceived += OnMessagingSessionInviteReceived;
			//Sony.NP.Messaging.OnMessageSessionInviteAccepted += OnMessagingSessionInviteAccepted;

			// User events.
	// 		Sony.NP.User.OnSignedIn += OnSignedIn;
	// 		Sony.NP.User.OnSignedOut += OnSomeEvent;
	// 		Sony.NP.User.OnSignInError += OnSignInError;

			user = new SonyNpUser();
			friends = new SonyNpFriends();
			ranking = new SonyNpRanking();
	// 		sessions = new SonyNpSession();
	// 		messaging = new SonyNpMessaging();
	// 		commerce = new SonyNpCommerce();
	//		cloudStorage = new SonyNpCloud();
	//		utilities = new SonyNpUtilities();

			requests = new SonyNpRequests();

			while (!npReady)
				yield return null;
		}

		public override void Tick()
		{
			Sony.NP.Main.Update();

	// 		if (InputManager.ActiveDevice != null && InputManager.ActiveDevice.LeftStickDown.WasPressed)
	// 		{
	// 			TryToUnlockAchievement(new UserSystem.User(), 0);
	// 		}
		}

		static public Sony.NP.ErrorCode ErrorHandler(Sony.NP.ErrorCode errorCode)
		{
			if (errorCode != Sony.NP.ErrorCode.NP_OK)
			{
				Debug.Log("Error: " + errorCode);
			}

			return errorCode;
		}

		void OnLog(Sony.NP.Messages.PluginMessage msg)
		{
			Debug.Log(msg.Text);
		}

		void OnLogWarning(Sony.NP.Messages.PluginMessage msg)
		{
			Debug.Log("Warning: " + msg.Text);
		}

		void OnLogError(Sony.NP.Messages.PluginMessage msg)
		{
			Debug.Log("Error: " + msg.Text);
		}

		void OnSomeEvent(Sony.NP.Messages.PluginMessage msg)
		{
			Debug.Log(msg.ToString());
		}

	#region Network Connection
		void OnSignedIn(Sony.NP.Messages.PluginMessage msg)
		{
			Debug.Log(msg.ToString());

			Sony.NP.ResultCode result = new Sony.NP.ResultCode();
			Sony.NP.User.GetLastSignInError(out result);
			if (result.lastError != Sony.NP.ErrorCode.NP_OK)
			{
				Debug.Log("Error: " + result.className + ": " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
			}
		}

		void OnConnectionDown(Sony.NP.Messages.PluginMessage msg)
		{
			Debug.Log("Connection Down");

			// Determining the reason for loss of connection...
			//
			// When connection is lost we can call Sony.NP.System.GetLastConnectionError() to obtain
			// the NetCtl error status and reason for loss of connection.
			//
			// ResultCode.lastError will be either NP_ERR_NOT_CONNECTED
			// or NP_ERR_NOT_CONNECTED_FLIGHT_MODE.
			//
			// For the case where ResultCode.lastError == NP_ERR_NOT_CONNECTED further information about
			// the disconnection reason can be inferred from ResultCode.lastErrorSCE which contains
			// the SCE NetCtl error code relating to the disconnection (please refer to SCE SDK docs when
			// interpreting this code).

			// Get the reason for loss of connection...
			Sony.NP.ResultCode result = new Sony.NP.ResultCode();
			Sony.NP.System.GetLastConnectionError(out result);
			Debug.Log("Reason: " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
		}

		void OnSignInError(Sony.NP.Messages.PluginMessage msg)
		{
			Sony.NP.ResultCode result = new Sony.NP.ResultCode();
			Sony.NP.User.GetLastSignInError(out result);
			Debug.Log(result.className + ": " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
		}
	#endregion

	#region Achievements
		protected override IEnumerator LoadAchievementDataRoutine(User userInfo)
		{
			Sony.NP.Trophies.RequestTrophyInfo();

			// Wait until we've finished requesting trophies
			// This request is started after the NP plugin is initialized so by the time we're in the game it will definitely be finished
			while (Sony.NP.Trophies.RequestTrophyInfoIsBusy())
				yield return null;

			// CONVERT TROPHY INFO INTO ACHIEVEMENT DATA
			var cachedTrophyData = Sony.NP.Trophies.GetCachedTrophyData ();
			var cachedTrophyInfo = Sony.NP.Trophies.GetCachedTrophyDetails ();

			_achievementData = new AchievementData[cachedTrophyData.Length];
			for (int i = 0; i < cachedTrophyData.Length; ++i)
			{
				var newAchievementData = new AchievementData(cachedTrophyData[i], cachedTrophyInfo[i]);
				var localAchievementData = PlatformServices.LocalAchievementData[i];

				// Copy achievement evalutation settings from local cache of achievements
				newAchievementData.associatedStat = localAchievementData.associatedStat;
				newAchievementData.compareType = localAchievementData.compareType;
				newAchievementData.statGroup = localAchievementData.statGroup;

				_achievementData[i] = newAchievementData;
			}

			yield return _achievementData;

			PlatformServices.GetInstance().RegisterAchievementCallbacks();
		}

		protected override IEnumerator TryToUnlockAchievementRoutine(AchievementData achievementDatum, System.Action unlockEvent)
		{
			Sony.NP.Trophies.AwardTrophy(GetAchievementData().IndexOf(achievementDatum));
			_isAttemptingToAwardTrophy = true;

			while (_isAttemptingToAwardTrophy)
				yield return null;

			if(_successfullyAwardedTrophy)
				unlockEvent();
		}

		void OnTrophyGotTrophyInfo(Sony.NP.Messages.PluginMessage msg)
		{
			OnScreenLog.Add("Got Trophy Data!");

			Sony.NP.Trophies.TrophyData[] data = Sony.NP.Trophies.GetCachedTrophyData();
			Sony.NP.Trophies.TrophyDetails[] details = Sony.NP.Trophies.GetCachedTrophyDetails();

			for (int i = 0; i < details.Length; i++)
			{
				Debug.Log(" " + i + ": " +
					details[i].groupId + ", " +
					details[i].hidden + ", " +
					details[i].trophyGrade + ", " +
					details[i].trophyId + ", " +
					details[i].description + ", " +
					details[i].name);

				Debug.Log(" " + i + ": " +
					data[i].timestamp.ToString() + ", " +
					data[i].trophyId + ", " +
					data[i].unlocked + ", " +
					data[i].userId + ", " +
					data[i].hasIcon);
			}
		}

		void OnAwardTrophySuccess(Sony.NP.Messages.PluginMessage msg)
		{
			_isAttemptingToAwardTrophy = false;
			_successfullyAwardedTrophy = true;
		}

		void OnAwardTrophyFail(Sony.NP.Messages.PluginMessage msg)
		{
			_isAttemptingToAwardTrophy = false;
			_successfullyAwardedTrophy = false;
		}
	#endregion // Achievements

	#region Cloud Saves
		void OnGotTssData(Sony.NP.Messages.PluginMessage msg)
		{
			//byte[] data = Sony.NP.TusTss.GetTssData();
			// Compare it against our current balance/news info, if it's different then update it
		}

		// Event handler; called when the async request to get TUS data has completed.
		void OnGotTusData(Sony.NP.Messages.PluginMessage msg)
		{
	#if UNITY_PS4
			byte[] data;
			data = Sony.NP.TusTss.GetTusData();

			if(m_TUSDataRequestType == TUSDataRequestType.LoadPlayerPrefs)
				UnityEngine.PS4.PS4PlayerPrefs.LoadFromByteArray(data);
	#endif
		}

		// Event handler; called when the async request to set TUS variables has completed.
		void OnSetTusVariables(Sony.NP.Messages.PluginMessage msg)
		{
			OnScreenLog.Add("Sent TUS variables");
		}

		// Event handler; called when the async request to get TUS variables has completed.
		void OnGotTusVariables(Sony.NP.Messages.PluginMessage msg)
		{
			// If we only want the values we could do long[] values = Sony.NP.TusTss.GetTusVariablesValue()
			// But lets get more info...

			Sony.NP.TusTss.TusRetrievedVariable[] variables = Sony.NP.TusTss.GetTusVariables();
			for (int i = 0; i < variables.Length; i++)
			{
				//string ownerNpID = System.Text.Encoding.Default.GetString(variables[i].ownerNpID);
				//string lastChangeAuthorNpID = System.Text.Encoding.Default.GetString(variables[i].lastChangeAuthorNpID);
			
				// Compare cloud save time vs local save time, if cloud save is newer then clobber current files
			}
		}
	#endregion
	}
}
#endif