using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffloadUpdateManager : Singleton<OffloadUpdateManager>
{
	public class OffloadSet
	{
		public OffloadSet(int numFrames)
		{
			for (int i = 0; i < numFrames; i++)
			{
				_updateLists.Add(new List<System.Action>());
			}
		}

		List<List<System.Action>> _updateLists = new List<List<System.Action>>();

		public int NumFrames => _updateLists.Count;

		public void Tick(int frame)
		{
			int relativeFrame = frame % NumFrames;
			for(int i = 0; i < _updateLists[relativeFrame].Count; i++)
			{
				_updateLists[relativeFrame][i]();
			}
		}

		public void RegisterUpdate(System.Action update)
		{
			int frameWithMinUpdates = 0;
			int minUpdates = int.MaxValue;
			for(int i = 0; i < _updateLists.Count; i++)
			{
				if(_updateLists[i].Count < minUpdates)
				{
					minUpdates = _updateLists[i].Count;
					frameWithMinUpdates = i;
				}
			}

			_updateLists[frameWithMinUpdates].Add(update);
		}

		public void UnregisterUpdate(System.Action update)
		{
			for (int i = 0; i < _updateLists.Count; i++)
			{
				_updateLists[i].Remove(update);
			}
		}
	}

	List<OffloadSet> _offloadSets = new List<OffloadSet>();

	private void Update()
	{
		int frame = Time.frameCount;
		for(int i = 0; i < _offloadSets.Count; i++)
		{
			_offloadSets[i].Tick(frame);
		}
	}

	public void RegisterUpdate(System.Action update, int gapFrames)
	{
		OffloadSet offloadSet = null;
		for(int i = 0; i < _offloadSets.Count; i++)
		{
			if (_offloadSets[i].NumFrames == gapFrames)
			{
				offloadSet = _offloadSets[i];
				break;
			}
		}

		if(offloadSet == null)
		{
			offloadSet = new OffloadSet(gapFrames);
			_offloadSets.Add(offloadSet);
		}

		offloadSet.RegisterUpdate(update);
	}

	public void UnregisterUpdate(System.Action update, int gapFrames)
	{
		for(int i = 0; i < _offloadSets.Count; i++)
		{
			if(_offloadSets[i].NumFrames == gapFrames)
			{
				_offloadSets[i].UnregisterUpdate(update);
				return;
			}
		}
	}
}
