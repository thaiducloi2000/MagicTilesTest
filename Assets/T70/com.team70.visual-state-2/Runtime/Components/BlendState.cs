using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace nano.vs2
{
	
public class BlendState : MonoBehaviour {

	public VisualState visualState;
	public Slider[] sliders;
	[SerializeField]
	List<AmountData> amounts;

	void Start () {
		if(sliders == null) return;
			if(visualState == null)
			{
				visualState = GetComponent<VisualState>();
				if(visualState == null) return;
			}
			amounts = new List<AmountData>();
			for(int i = 0; i < sliders.Length; i++)
			{
				amounts.Add(new AmountData());
				var index = i;
				sliders[index].onValueChanged.AddListener(onValueChanged);
			}
		}

        private void onValueChanged(float arg0)
        {
            for(int i = 0; i < sliders.Length; i++)
			{
				var a = amounts[i];
				a.amount = sliders[i].value;
				amounts[i] = a;
			}
			visualState.ApplyBlend(amounts);
        }
    }
}