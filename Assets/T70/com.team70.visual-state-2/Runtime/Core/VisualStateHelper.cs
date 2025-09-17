using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace nano.vs2
{
	/*internal interface IValueUpdater
	{
		void 
	}*/
	public class VSAnimData
	{
		public float time;
		public float beginTime;
		public float beginValue;
		public float endValue;
	}
	public class VisualStateHelper : MonoBehaviour {
		
		
		private static VisualStateHelper _instance;
		public static VisualStateHelper Instance
		{
			get
			{
				if(_instance == null)
				{
					_instance = new GameObject("@VSHelper").AddComponent<VisualStateHelper>();
				}
				return _instance;
			}
		}
        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(this.gameObject);
            }
            _instance = this;

            var p = transform.parent;
            transform.SetParent(null, false);
            {
	            DontDestroyOnLoad(gameObject);    
            }
            transform.SetParent(p, false);
        }
        public Action OnUpdate;

		void Update()
		{
			if(OnUpdate != null)
			{
				OnUpdate();
			}
		}
	}
}