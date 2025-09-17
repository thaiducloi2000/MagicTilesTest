using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace nano.vs2
{
	
	public class SliderToState : MonoBehaviour {
		public int fromState = 0;
		public int toState = 1;
		public Slider slider;

		public VisualState visualState;


		// Use this for initialization
		void Start () {
			if(slider == null)
			{
				slider = GetComponent<Slider>();
				if(slider == null) return;
			}
			if(visualState == null)
			{
				visualState = GetComponent<VisualState>();
				if(visualState == null) return;
			}
			slider.onValueChanged.AddListener(OnValudeChanged) ;
		}

		private void OnValudeChanged(float amount)
		{
			visualState.SetStateAmount(fromState, toState, amount);
		}
	}

}