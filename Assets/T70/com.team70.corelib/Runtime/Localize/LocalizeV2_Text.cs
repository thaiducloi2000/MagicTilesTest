using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizeV2_Text : LocalizeV2.LocalizedText 
{
	public Text target;
    public bool toUpper = false;

	
    [ContextMenu(itemName: "Add")]
    public void Add()
    {
        target = gameObject.GetComponent<Text>();
        defaultText = target.text;
        string keyID = "GUI_ID_STRING_";
        string[] split = target.text.Trim().Split(' ');
        int count = split.Length;
        if (count == 1)
        {
            keyID += split[0].ToUpper();
        }
        else if (count <= 5)
        {
            keyID += split[0].ToUpper() + "_" + split[count - 1].ToUpper();
        }
        else
        {
            keyID += split[0].ToUpper() + "_" + split[1].ToUpper();
        }
        locID = keyID;
    }
    public override string Text 
	{
		get { return target != null ? target.text : string.Empty; }
		set {
			if (target == null) return;

            var text = value ?? string.Empty;
            if (toUpper) text = text.ToUpper();
			target.text = text;
		}
	}
}
