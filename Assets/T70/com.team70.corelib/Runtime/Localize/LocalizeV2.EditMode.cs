#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public partial class LocalizeV2: ScriptableObject
{
	public static HashSet<string> ignoreStrings = new HashSet<string>()
	{
		
	};

	public static HashSet<string> ignoreIDs = new HashSet<string>()
	{
		
	};

	public void ClearTranslation(int index)
	{
		for (var i = 0; i < data.Count; i++)
		{
			data[i].SetValue(string.Empty, index);
		}
	}

	public string ToTSV()
	{
		data.Sort((e1, e2) =>
		{
			var g1 = e1.GetGroup2();
			var g2 = e2.GetGroup2();

			if (g1 != g2) return string.Compare(g1, g2, StringComparison.Ordinal);
			var result = e1.locID.Length.CompareTo(e2.locID.Length);
			return result == 0 ? string.Compare(e1.locID, e2.locID, StringComparison.Ordinal) : result;
		});

		var sb = new StringBuilder();
		sb.Append("-LOCID");
		sb.Append("\t");

		for (var i = 0; i < langs.Count; i++)
		{
			sb.Append(langs[i]);
			sb.Append("\t");
		}
		sb.Append("\n");

		for (var i = 0; i < data.Count; i++)
		{
			sb.AppendLine(data[i].ToTSV(langs));
		}
		return sb.ToString();
	}

	public void Set(string locID, string value, int langIndex, bool? enable = null)
	{
		if (_cache == null) iBuildCache();

		locID = locID.Trim();

		Element result;

		if (_cache.TryGetValue(locID, out result))
		{
			// Update existed item
			result.SetValue(value, langIndex);
			if (enable != null) result.enable = enable.Value;
			UnityEditor.EditorUtility.SetDirty(this);
			return;
		}

		result = new Element()
		{
			locID = locID,
			enable = enable == null || enable.Value,
			locValues = new List<string>()
		};

		result.SetValue(value, langIndex);
		_cache.Add(locID, result);
		data.Add(result);
	}
}
#endif