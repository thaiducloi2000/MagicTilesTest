using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SearchUI2
{
	Action<string, bool> onChange; //searchTerm, caseSensitive
	
	string searchTerm;
	bool caseSensitive;
	bool dirty;
	
	static GUIStyle toolbarSearchField;
	static GUIStyle toolbarSearchFieldCancelButton;
	static GUIStyle toolbarSearchFieldCancelButtonEmpty;
	
	public SearchUI2(Action<string, bool> onChange)
	{
		this.onChange = onChange;
	}
	
	public void Draw()
	{
		if (toolbarSearchField == null)
		{
			toolbarSearchField = "ToolbarSeachTextFieldPopup";
			toolbarSearchFieldCancelButton = "ToolbarSeachCancelButton";
			toolbarSearchFieldCancelButtonEmpty = "ToolbarSeachCancelButtonEmpty";
		}
		
		GUILayout.BeginHorizontal(EditorStyles.toolbar);
		{
			var v = GUILayout.Toggle(caseSensitive, "Aa", EditorStyles.toolbarButton, GUILayout.Width(24f));
			if (v != caseSensitive)
			{
				caseSensitive = v;
				dirty = true;
			}
			
			GUILayout.Space(2f);
			var value = GUILayout.TextField(searchTerm ?? "", toolbarSearchField);	
			if (searchTerm != value)
			{
				searchTerm = value;	
				dirty = true;
			}
			
			var style = string.IsNullOrEmpty(searchTerm) ? toolbarSearchFieldCancelButtonEmpty : toolbarSearchFieldCancelButton;
			if (GUILayout.Button("Cancel", style))
			{
				searchTerm = string.Empty;
				dirty = true;
			}
			GUILayout.Space(2f);
		}
		GUILayout.EndHorizontal();
		
		if (dirty && onChange != null)
		{
			dirty = false;
			onChange(caseSensitive ? searchTerm : searchTerm.ToLower(), caseSensitive);
		}
	}
	
	static public int StringMatch(string pattern, bool caseSensitive, params string[] inputs)
	{
		var max = 0;
		for (var i = 0;i < inputs.Length;i ++)
		{
            if (string.IsNullOrEmpty(inputs[i])) continue;
			max = Mathf.Max(max, StringMatch(pattern, caseSensitive ? inputs[i]: inputs[i].ToLower()));
		}
		
		return max > pattern.Length ? max : 0;
	}
	
	static public int StringMatch(string pattern, string input)
	{
		if (input == pattern) return int.MaxValue;
		if (input.Contains(pattern)) return int.MaxValue-1;
		
		int pidx = 0;
		int score = 0;
		int tokenScore = 0;
		
		for (var i = 0;i < input.Length; i++)
		{
			var ch = input[i];
			if (ch == pattern[pidx])
			{
				tokenScore += tokenScore + 1; //increasing score for continuos token
				pidx++;
				if (pidx >= pattern.Length) break;
			} else {
				tokenScore = 0;
			}
			
			score += tokenScore;
		}
		
		return score;
	}
}
