using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace com.team70
{
	[Serializable] public class ElementInfo
	{
		public string id;
		public List<Component> component = new List<Component>();
		public string componentType;

		public void CleanUp()
		{
			var list = new List<Component>();
			var h = new HashSet<Component>();
			for (var i = 0; i < component.Count; i++)
			{
				Component c = component[i];
				if (c == null) continue;
				if (h.Contains(c)) continue;

				h.Add(c);
				list.Add(c);
			}

			component = list;
		}
	}

	public partial class PrefabModule
	{
		public List<ElementInfo> lstElement;

		public T GetElementT<T>(string id, bool debugLog = true) where T : Component
		{
			var count = lstElement.Count;
			for (var i = 0; i < count; i++)
			{
				ElementInfo elementInfo = lstElement[i];
				if (elementInfo.id != id) continue;
				try
				{
					return (T)elementInfo.component[0];
				}
				catch (Exception e)
				{
					if (debugLog) Debug.LogError($"{this.gameObject.name} Can't Cast _ Element Name {id} from type {elementInfo.component[0].GetType()} to {typeof(T)}\n {e}");
					return null;
				}
			}
			if (debugLog) Debug.LogWarning($"{gameObject} : elementId {id} not found!");
			return null;
		}

		public T[] GetElementsT<T>(string id, bool debugLog = true) where T : Component
		{
			var count = lstElement.Count;
			for (var i = 0; i < count; i++)
			{
				ElementInfo elementInfo = lstElement[i];
				if (elementInfo.id != id) continue;
				var elements = elementInfo.component.Select((item, index) => (T)item).ToArray();
				if (elements.Length == 0 && debugLog) Debug.LogWarning($"{gameObject} with elementId: {id} should not be empty []");
				for (var j = 0; j < elements.Length; j++)
				{
					if (elements[j] == null && debugLog) Debug.LogWarning($"{gameObject} with elementId {id} has null element at index: " + j);
				}
				return elements;
			}
			if (debugLog) Debug.LogWarning($"{gameObject} : elementId {id} not found!");
			return null;
		}

		[ContextMenu("Clean Up")]
		public void CleanUp()
		{
#if UNITY_EDITOR
			for (var i = 0; i < lstElement.Count; i++)
			{
				lstElement[i].CleanUp();
			}

			EditorUtility.SetDirty(this);
#endif
		}

		//-------- HELPER -----------

		public void GetElements<T1, T2>
		(out T1 t1, string id1,
		 out T2 t2, string id2
		)
			where T1 : Component
			where T2 : Component
		{
			t1 = GetElementT<T1>(id1);
			t2 = GetElementT<T2>(id2);
		}

		public void GetElements<T1, T2, T3>
		(out T1 t1, string id1,
		 out T2 t2, string id2,
		 out T3 t3, string id3
		)
			where T1 : Component
			where T2 : Component
			where T3 : Component
		{
			t1 = GetElementT<T1>(id1);
			t2 = GetElementT<T2>(id2);
			t3 = GetElementT<T3>(id3);
		}

		public void GetElements<T1, T2, T3, T4>
		(out T1 t1, string id1,
		 out T2 t2, string id2,
		 out T3 t3, string id3,
		 out T4 t4, string id4
		)
			where T1 : Component
			where T2 : Component
			where T3 : Component
			where T4 : Component
		{
			t1 = GetElementT<T1>(id1);
			t2 = GetElementT<T2>(id2);
			t3 = GetElementT<T3>(id3);
			t4 = GetElementT<T4>(id4);
		}

		public void GetElements<T1, T2, T3, T4, T5>
		(out T1 t1, string id1,
		 out T2 t2, string id2,
		 out T3 t3, string id3,
		 out T4 t4, string id4,
		 out T5 t5, string id5
		)
			where T1 : Component
			where T2 : Component
			where T3 : Component
			where T4 : Component
			where T5 : Component
		{
			t1 = GetElementT<T1>(id1);
			t2 = GetElementT<T2>(id2);
			t3 = GetElementT<T3>(id3);
			t4 = GetElementT<T4>(id4);
			t5 = GetElementT<T5>(id5);
		}

		public void GetElements<T1, T2, T3, T4, T5, T6>
		(out T1 t1, string id1,
		 out T2 t2, string id2,
		 out T3 t3, string id3,
		 out T4 t4, string id4,
		 out T5 t5, string id5,
		 out T6 t6, string id6
		)
			where T1 : Component
			where T2 : Component
			where T3 : Component
			where T4 : Component
			where T5 : Component
			where T6 : Component
		{
			t1 = GetElementT<T1>(id1);
			t2 = GetElementT<T2>(id2);
			t3 = GetElementT<T3>(id3);
			t4 = GetElementT<T4>(id4);
			t5 = GetElementT<T5>(id5);
			t6 = GetElementT<T6>(id6);
		}

		public void GetElements<T1, T2, T3, T4, T5, T6, T7>
		(out T1 t1, string id1,
		 out T2 t2, string id2,
		 out T3 t3, string id3,
		 out T4 t4, string id4,
		 out T5 t5, string id5,
		 out T6 t6, string id6,
		 out T7 t7, string id7
		)
			where T1 : Component
			where T2 : Component
			where T3 : Component
			where T4 : Component
			where T5 : Component
			where T6 : Component
			where T7 : Component
		{
			t1 = GetElementT<T1>(id1);
			t2 = GetElementT<T2>(id2);
			t3 = GetElementT<T3>(id3);
			t4 = GetElementT<T4>(id4);
			t5 = GetElementT<T5>(id5);
			t6 = GetElementT<T6>(id6);
			t7 = GetElementT<T7>(id7);
		}

		public void GetElements<T1, T2, T3, T4, T5, T6, T7, T8>
		(out T1 t1, string id1,
		 out T2 t2, string id2,
		 out T3 t3, string id3,
		 out T4 t4, string id4,
		 out T5 t5, string id5,
		 out T6 t6, string id6,
		 out T7 t7, string id7,
		 out T8 t8, string id8
		)
			where T1 : Component
			where T2 : Component
			where T3 : Component
			where T4 : Component
			where T5 : Component
			where T6 : Component
			where T7 : Component
			where T8 : Component
		{
			t1 = GetElementT<T1>(id1);
			t2 = GetElementT<T2>(id2);
			t3 = GetElementT<T3>(id3);
			t4 = GetElementT<T4>(id4);
			t5 = GetElementT<T5>(id5);
			t6 = GetElementT<T6>(id6);
			t7 = GetElementT<T7>(id7);
			t8 = GetElementT<T8>(id8);
		}

		public void GetElements<T1, T2, T3, T4, T5, T6, T7, T8, T9>
		(out T1 t1, string id1,
		 out T2 t2, string id2,
		 out T3 t3, string id3,
		 out T4 t4, string id4,
		 out T5 t5, string id5,
		 out T6 t6, string id6,
		 out T7 t7, string id7,
		 out T8 t8, string id8,
		 out T9 t9, string id9
		)
			where T1 : Component
			where T2 : Component
			where T3 : Component
			where T4 : Component
			where T5 : Component
			where T6 : Component
			where T7 : Component
			where T8 : Component
			where T9 : Component
		{
			t1 = GetElementT<T1>(id1);
			t2 = GetElementT<T2>(id2);
			t3 = GetElementT<T3>(id3);
			t4 = GetElementT<T4>(id4);
			t5 = GetElementT<T5>(id5);
			t6 = GetElementT<T6>(id6);
			t7 = GetElementT<T7>(id7);
			t8 = GetElementT<T8>(id8);
			t9 = GetElementT<T9>(id9);
		}

		public void GetElements<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
		(out T1 t1, string id1,
		 out T2 t2, string id2,
		 out T3 t3, string id3,
		 out T4 t4, string id4,
		 out T5 t5, string id5,
		 out T6 t6, string id6,
		 out T7 t7, string id7,
		 out T8 t8, string id8,
		 out T9 t9, string id9,
		 out T10 t10, string id10
		)
			where T1 : Component
			where T2 : Component
			where T3 : Component
			where T4 : Component
			where T5 : Component
			where T6 : Component
			where T7 : Component
			where T8 : Component
			where T9 : Component
			where T10 : Component
		{
			t1 = GetElementT<T1>(id1);
			t2 = GetElementT<T2>(id2);
			t3 = GetElementT<T3>(id3);
			t4 = GetElementT<T4>(id4);
			t5 = GetElementT<T5>(id5);
			t6 = GetElementT<T6>(id6);
			t7 = GetElementT<T7>(id7);
			t8 = GetElementT<T8>(id8);
			t9 = GetElementT<T9>(id9);
			t10 = GetElementT<T10>(id10);
		}

		public void GetElements<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
		(out T1 t1, string id1,
		 out T2 t2, string id2,
		 out T3 t3, string id3,
		 out T4 t4, string id4,
		 out T5 t5, string id5,
		 out T6 t6, string id6,
		 out T7 t7, string id7,
		 out T8 t8, string id8,
		 out T9 t9, string id9,
		 out T10 t10, string id10,
		 out T11 t11, string id11
		)
			where T1 : Component
			where T2 : Component
			where T3 : Component
			where T4 : Component
			where T5 : Component
			where T6 : Component
			where T7 : Component
			where T8 : Component
			where T9 : Component
			where T10 : Component
			where T11 : Component
		{
			t1 = GetElementT<T1>(id1);
			t2 = GetElementT<T2>(id2);
			t3 = GetElementT<T3>(id3);
			t4 = GetElementT<T4>(id4);
			t5 = GetElementT<T5>(id5);
			t6 = GetElementT<T6>(id6);
			t7 = GetElementT<T7>(id7);
			t8 = GetElementT<T8>(id8);
			t9 = GetElementT<T9>(id9);
			t10 = GetElementT<T10>(id10);
			t11 = GetElementT<T11>(id11);
		}

		public void GetElements<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
		(out T1 t1, string id1,
		 out T2 t2, string id2,
		 out T3 t3, string id3,
		 out T4 t4, string id4,
		 out T5 t5, string id5,
		 out T6 t6, string id6,
		 out T7 t7, string id7,
		 out T8 t8, string id8,
		 out T9 t9, string id9,
		 out T10 t10, string id10,
		 out T11 t11, string id11,
		 out T12 t12, string id12
		)
			where T1 : Component
			where T2 : Component
			where T3 : Component
			where T4 : Component
			where T5 : Component
			where T6 : Component
			where T7 : Component
			where T8 : Component
			where T9 : Component
			where T10 : Component
			where T11 : Component
			where T12 : Component
		{
			t1 = GetElementT<T1>(id1);
			t2 = GetElementT<T2>(id2);
			t3 = GetElementT<T3>(id3);
			t4 = GetElementT<T4>(id4);
			t5 = GetElementT<T5>(id5);
			t6 = GetElementT<T6>(id6);
			t7 = GetElementT<T7>(id7);
			t8 = GetElementT<T8>(id8);
			t9 = GetElementT<T9>(id9);
			t10 = GetElementT<T10>(id10);
			t11 = GetElementT<T11>(id11);
			t12 = GetElementT<T12>(id12);
		}
	}
}
