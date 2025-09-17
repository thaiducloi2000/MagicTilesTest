using System.Text;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using com.team70;
using UnityEngine.UI;
#if UNITY_EDITOR
#endif

namespace com.team70
{
	public interface IDataChangeCallback
	{
		void SetDataChangeCallback(Action<object> callback);
	}
	
	public interface IPrefabModuleDataLogic<T> : IPrefabModuleLogic
	{
		T GetData();
		void SetData(T data);
	}

	public partial class PrefabModule : MonoBehaviour
	{
		void Awake()
		{
			Logic_Awake();
		}
		
		public void OnEnable()
		{
			Localize_OnEnable();
		}
		
		public void OnDisable()
		{
			Localize_OnDisable();
		}
		
		public static PrefabModule Load(string id, Transform parent)
		{
			if (string.IsNullOrEmpty(id))
			{
				Debug.LogWarning("PrefabModule:: Id shouldn't be null or empty!");
				return null;
			}

			var module = Resources.Load<PrefabModule>(id);
			if (module != null) return Instantiate(module, parent);
			Debug.LogWarning("Prefab module not found: " + id);
			return null;
		}

		[ContextMenu("DebugGetAllElement")]
		public void DebugGetAllElement()
		{
			Debug.Log("DebugGetAllElement----------------------->");
			for (int i = 0; i < lstElement.Count; i++)
			{
				Debug.Log($"{lstElement[i].id} ___ {lstElement[i].componentType}");
			}
		}
	}

}
