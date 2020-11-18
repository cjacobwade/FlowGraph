using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TagManager : Singleton<TagManager>
{
	Dictionary<System.Type, List<TagBase>> _typeToTagListMap = new Dictionary<System.Type, List<TagBase>>();

	List<TagBase> _tagList = new List<TagBase>();
	
	public void RegisterTag(TagBase inTag)
	{
		if (!_typeToTagListMap.TryGetValue(inTag.GetType(), out _tagList))
		{
			_tagList = new List<TagBase>();
			_typeToTagListMap.Add(inTag.GetType(), _tagList);
		}

		_tagList.Add(inTag);
	}

	public bool DeregisterTag(TagBase inTag)
	{
		if (_typeToTagListMap.TryGetValue(inTag.GetType(), out _tagList))
		{
			return _tagList.Remove(inTag);
		}

		return false;
	}

	public void GetTagList<T>(ref List<T> list) where T : TagBase
	{
		_typeToTagListMap.TryGetValue(typeof(T), out _tagList);

		list.Clear();
		for (int i = 0; i < _tagList.Count; i++)
		{
			list.Add(_tagList[i] as T);
		}
	}

	public T GetFirstTag<T>() where T : TagBase
	{
		if (_typeToTagListMap.TryGetValue(typeof(T), out _tagList))
		{
			return _tagList.First() as T;
		}

		return null;
	}
}
