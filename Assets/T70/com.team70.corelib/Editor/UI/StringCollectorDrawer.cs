using System;
using UnityEditor;
using UnityEngine;

public class StringCollectorDrawer
{
    public string[] data;
    int _selectedIndex = -1;
	
	public bool Draw(Rect rect, ref string selected, bool forceRefresh)
	{
		var result = false;
        
        if (data == null || data.Length == 0)
        {
            EditorGUI.HelpBox(rect, "Data set is empty!", MessageType.Warning);
            return false;
        }
        
        if (_selectedIndex == -1 || forceRefresh)
        {
            _selectedIndex = Array.IndexOf(data, selected);
            if (_selectedIndex == -1)
            {
                _selectedIndex = 0;
                selected = data[0];
                result = true;
            }
        }
		
		var newIndex = EditorGUI.Popup(rect, _selectedIndex, data, EditorStyles.toolbarDropDown);
		if (newIndex != _selectedIndex)
		{
			_selectedIndex = newIndex;
			selected = data[_selectedIndex];
			return true;
		}
		
		return result;
	}
    
    public bool Draw(ref string selected, bool forceRefresh = false)
    {
        var rect = GUILayoutUtility.GetRect(0, Screen.width, 18f, 18f);
		return Draw(rect, ref selected, forceRefresh);
    }
}