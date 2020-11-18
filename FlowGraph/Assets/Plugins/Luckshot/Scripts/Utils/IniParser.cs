/*    
    .Ini file Parser
    Author: Tristan 'Kennyist' Cunningham - www.tristanjc.com
    Date: 13/04/2014
    License: Creative commons ShareAlike 3.0 - https://creativecommons.org/licenses/by-sa/3.0/
*/

using UnityEngine;
using System.Collections;
using System.IO;

/// <summary>
/// An .ini file parser that Creates and edits .ini files, With functions to fetch and delete values.
/// </summary>
public class IniParser
{
	private ArrayList keys = new ArrayList();
	private ArrayList vals = new ArrayList();
	private ArrayList comments = new ArrayList();

	/// <summary>
	/// Initializes a new instance of the <see cref="iniParser"/> class without loading a file.
	/// </summary>
	public IniParser() { }

	/// <summary>
	/// Initializes a new instance of the <see cref="iniParser"/> class with loading a file.
	/// </summary>
	/// <param name="file">Name of the file you want to load.</param>
	public IniParser(string file)
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		Load(file);
#endif
	}

	/// <summary>
	/// Returns true if the file exists, or false if it doesnt.
	/// </summary>
	/// <param name="file">The selected file.</param>
	public bool DoesExist(string file)
	{
		return File.Exists(Application.dataPath + "/" + file + ".ini") ? true : false;
	}

	/// <summary>ss
	/// Set the variable and value if they dont exist. Updates the variables value if does exist.
	/// </summary>
	/// <param name="key">The variable name</param>
	/// <param name="val">The value of the variable</param>
	public void Set(string key, string val)
	{
		for (int i = 0; i < keys.Count; i++)
		{
			if (keys[i].ToString() == key)
			{
				vals[i] = val;
				return;
			}
		}

		keys.Add(key);
		vals.Add(val);
		comments.Add("");
	}

	/// <summary>
	/// Set the variable and value if they dont exist including a comment. Updates the variables value and comment if does exist.
	/// </summary>
	/// <param name="key">The variable name</param>
	/// <param name="val">The value of the variable</param>
	/// <param name="comment">The comment of the variable</param>
	public void Set(string key, string val, string comment)
	{
		for (int i = 0; i < keys.Count; i++)
		{
			if (keys[i].ToString() == key)
			{
				vals[i] = val;
				comments[i] = comment;
				return;
			}
		}

		keys.Add(key);
		vals.Add(val);
		comments.Add(comment);
	}

	/// <summary>
	/// Returns the value for the input variable.
	/// </summary>
	/// <param name="key">The variable name.</param>
	public string Get(string key)
	{
		for (int i = 0; i < keys.Count; i++)
		{
			if (keys[i].Equals(key))
			{
				return vals[i].ToString();
			}
		}
		return "";
	}

	public bool HasKey(string key)
	{
		for (int i = 0; i < keys.Count; i++)
		{
			if (keys[i].Equals(key))
				return true;
		}
		return false;
	}

	public void TryGetBool(string key, ref bool? val)
	{
		val = null;
		for (int i = 0; i < keys.Count; i++)
		{
			if (keys[i].Equals(key))
			{
				bool setVal = false;
				if (bool.TryParse(vals[i].ToString(), out setVal))
					val = setVal;
			}
		}
	}

	public void TryGetInt(string key, ref int? val)
	{
		val = null;
		for (int i = 0; i < keys.Count; i++)
		{
			if (keys[i].Equals(key))
			{
				int setVal = -1;
				if (int.TryParse(vals[i].ToString(), out setVal))
					val = setVal;
			}
		}
	}

	public void TryGetFloat(string key, ref float? val)
	{
		val = null;
		for (int i = 0; i < keys.Count; i++)
		{
			if (keys[i].Equals(key))
			{
				float setVal = -1f;
				if (float.TryParse(vals[i].ToString(), out setVal))
					val = setVal;
			}
		}
	}

	public int GetEnum<T>(string key) where T : struct
	{
		for (int i = 0; i < keys.Count; i++)
		{
			if (keys[i].Equals(key))
				return (int)System.Enum.Parse(typeof(T), vals[i].ToString());
		}

		return 0;
	}

	/// <summary>
	/// Returns the Key, Value and comment of the choosen variable.
	/// </summary>
	/// <returns>String array containing the 3 values</returns>
	/// <param name="key">The variable name.</param>
	public string[] GetLine(string key)
	{
		string[] list = new string[2];

		for (int i = 0; i < keys.Count; i++)
		{
			if (keys[i].Equals(key))
			{
				list[0] = keys[i].ToString();
				list[1] = vals[i].ToString();
				list[2] = comments[i].ToString();
				return list;
			}
		}

		return list;
	}

	/// <summary>
	/// Removes the selected Variable including its value and comment.
	/// </summary>
	/// <param name="key">The variable name.</param>
	public void Remove(string key)
	{
		for (int i = 0; i < keys.Count; i++)
		{
			if (keys[i].ToString() == key)
			{
				keys.RemoveAt(i);
				vals.RemoveAt(i);
				comments.RemoveAt(i);
				return;
			}
		}
		Debug.LogWarning("Key not found");
	}

	/// <summary>
	/// Save the specified file.
	/// </summary>
	/// <param name="file">The file name.</param>
	public void Save(string file)
	{
		StreamWriter wr = new StreamWriter(Application.dataPath + "/" + file + ".ini");

		for (int i = 0; i < keys.Count; i++)
		{
			if (comments[i].Equals(""))
			{
				wr.WriteLine(keys[i] + "=" + vals[i]);
			}
			else
			{
				wr.WriteLine(keys[i] + "=" + vals[i] + " //" + comments[i]);
			}
		}

		wr.Close();

		Debug.Log(file + ".ini Saved");
	}

	/// <summary>
	/// Load the specified file.
	/// </summary>
	/// <param name="file">The file name.</param>
	public void Load(string file)
	{
		keys = new ArrayList();
		vals = new ArrayList();
		comments = new ArrayList();

		string line = "", dir = Application.dataPath + "/" + file + ".ini";
		int offset = 0, comment = 0;

		try
		{
			using (StreamReader sr = new StreamReader(dir))
			{
				while ((line = sr.ReadLine()) != null)
				{
					offset = line.IndexOf("=");
					comment = line.IndexOf("//");
					if (offset > 0 && comment == -1)
						Set(line.Substring(0, offset), line.Substring(offset + 1));
				}
				sr.Close();
				Debug.Log(file + " Loaded");
			}
		}
		catch (IOException e)
		{
			Debug.Log("Error opening " + file + ".ini");
			Debug.LogWarning(e);
		}
	}

	/// <summary>
	/// How many keys are stored.
	/// </summary>
	public int Count()
	{
		return keys.Count;
	}
}
