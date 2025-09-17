using System;
using UnityEngine;
public partial class LocalizeV2 : ScriptableObject
{
	public class LocalizedText : MonoBehaviour
#if VISUAL_STATE
    , vietlabs.vs.IVisualStateListener
#endif
	{
		public string locID;
		public string defaultText;

#if UNITY_EDITOR
		[NonSerialized] public string originalText;
		public void RestoreOriginal(bool destroyMe)
		{
			Text = originalText;
			if (destroyMe) Destroy(this);
		}
#endif

		public virtual string Text
		{
			get { return string.Empty; }
			set { }
		}

		public void Refresh()
		{
			if (string.IsNullOrEmpty(locID)) return;
			Text = GetWithFallback(locID, defaultText);
			//Debug.Log("Refresh : "+ locID + "\n" + LocallizeV2.Get(locID));
		}


		void OnEnable()
		{
			OnChange -= Refresh;
			OnChange += Refresh;
			Refresh();
		}

		void OnDisable()
		{
			OnChange -= Refresh;
		}

#if VISUAL_STATE
        public void OnVSChange() { Refresh(); }
#endif
	}
}
