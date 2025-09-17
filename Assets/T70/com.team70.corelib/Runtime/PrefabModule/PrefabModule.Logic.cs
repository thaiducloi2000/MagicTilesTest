using System;
using System.Reflection;
using UnityEngine;
namespace com.team70
{
	public interface IPrefabModuleLogic
	{
		void Init(PrefabModule module, object initData = null);
	}

	[Serializable]
	public class PrefabModuleScript
	{
		// static readonly List<string> PMClasses = SerializableType.GetDerrivedTypeNames(typeof(IPrefabModuleLogic));

		public bool autoAttach;
		public bool autoRef;
		public bool autoInit;
		public SerializableType classType;

		public IPrefabModuleLogic AttachLogic(PrefabModule module)
		{
			Type scriptType = classType.cacheType;
			if (scriptType == null) return null;

			var logic = (IPrefabModuleLogic)module.gameObject.AddComponent(scriptType);
			if (logic == null) return null;
			if (autoRef) module.AttachRef(logic);
			if (autoInit) logic.Init(module);

			return logic;
		}
	}

	public partial class PrefabModule
	{
		public PrefabModuleScript script;
		public bool logicAttached;
		[NonSerialized] public IPrefabModuleLogic logic;

		public T GetLogicT<T>() where T : IPrefabModuleLogic
		{
			return (T)logic;
		}
		
		void Logic_Awake()
		{
			if (script != null && script.autoAttach) AttachLogic();
		}

		public void AttachRef(object targetScript)
		{
			var scriptType = targetScript.GetType();

			foreach (var item in lstElement)
			{
				if (item == null) continue;

				var field = scriptType.GetField(item.id, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field == null) continue;

				var c0 = item.component[0];

				var fieldType = field.FieldType;
				if (fieldType == typeof(GameObject))
				{
					// Debug.Log("Found a Field: " + item.id);
					field.SetValue(targetScript, c0.gameObject);
					continue;
				}

				if (typeof(Component).IsAssignableFrom(fieldType)) // single
				{
					// var c = go.GetComponent(fieldType);
					// Debug.Log("Found a Field: " + fieldType + " --> " + item.id + " ---> " + c);
					field.SetValue(targetScript, c0);
					continue;
				}

				if (fieldType.IsArray) //fieldType.IsGenericType && 
				{
					Type elmType = fieldType.GetElementType();
					var arr = Array.CreateInstance(elmType, item.component.Count);

					// Debug.Log("ElementType: " + elmType + " | " + arr.GetType());
					// copy data in
					for (var i = 0; i < arr.Length; i++)
					{
						Component c = item.component[i]; //.gameObject.GetComponent(elmType);
						arr.SetValue(c, i);
					}

					field.SetValue(targetScript, arr);
					continue;
				}

				Debug.LogWarning("Not yet supported: " + fieldType + " --> " + item.id);
			}
		}

		public void AttachLogicAndInit(PrefabModule module, object initData = null, Action<object> onDataChangeCallback = null)
		{
			IPrefabModuleLogic pm = AttachLogic();
			pm.Init(module, initData);
			if (onDataChangeCallback != null) SetDataChangeCallback(onDataChangeCallback);
		}

		public T AttachLogic<T>(Action<object> onDataChangeCallback = null) where T : IPrefabModuleLogic
		{
			return (T)AttachLogic(onDataChangeCallback);
		}

		public IPrefabModuleLogic AttachLogic(Action<object> onDataChangeCallback = null)
		{
			if (script == null) return null;
			if (logic != null) return logic;

			if (logicAttached)
			{
				// Debug.LogWarning("Logic attached! did you clone? trying to reuse logic: ");
				logic = T70.GetComponent<IPrefabModuleLogic>(transform);

				if (logic == null)
				{
					Debug.LogWarning("Something wrong - attached logic is null?");
				}
			}

			if (logic == null)
			{
				logic = script.AttachLogic(this);
				if (logic == null) return null;
			}

			logicAttached = true;
			if (localizeHook != null && logic is IPrefabModuleLocalize)
			{
				var pmLocalize = (IPrefabModuleLocalize)logic;
				if (pmLocalize != null)
				{
					localizeHook.onLocalizeChange -= pmLocalize.OnLocalizeChange;
					localizeHook.onLocalizeChange += pmLocalize.OnLocalizeChange;
					RefreshLocalize();
					pmLocalize.OnLocalizeChange();
				}
			}

			if (onDataChangeCallback != null) SetDataChangeCallback(onDataChangeCallback);
			return logic;
		}

		// public IPrefabModuleDataLogic<T> AttachLogicT<T>(Action<object> onDataChangeCallback = null)
		// {
		// 	return (IPrefabModuleDataLogic<T>) AttachLogic(onDataChangeCallback);
		// }

		public void SetLogicData<T>(T data)
		{
			((IPrefabModuleDataLogic<T>)logic).SetData(data);
		}

		public T GetLogicData<T>()
		{
			return ((IPrefabModuleDataLogic<T>)logic).GetData();
		}

		public void SetDataChangeCallback(Action<object> onDataChangeCallback)
		{
			if (logic == null)
			{
				Debug.LogWarning("SetDataChange Callback only works after attach logic!");
				return;
			}

			if (onDataChangeCallback == null) return;
			var logicCB = logic as IDataChangeCallback;
			logicCB?.SetDataChangeCallback(onDataChangeCallback);
		}
	}
}
