using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
namespace nano.vs2
{

	public class VSGetSet
	{
		internal object invokeTarget;
		internal Type dataType;

		public virtual object GetValue()
		{
			return null;
		}

		public virtual bool SetValue(object data)
		{
			return false;
		}

		// ----------------- STATTIC -------------------

		public const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public |
								   BindingFlags.NonPublic;


		public static VSGetSet Create(object target, string property)
		{
			if (target == null)
			{
				Debug.LogWarning("Target is null !");
				return null;
			}

			if (VSGetSet_GO.SPECIAL_PROPERTIES.Contains(property))
			{
				return new VSGetSet_GO(target, property);
			}

			var targetT = target.GetType();
			var prop1 = GetMember(target, targetT, property);


			if (prop1 != null) return prop1;
			string property2 = null;

			if (property.StartsWith("m_"))
			{
				//IMPORTANT : WORKAROUND FOR UNITY'S BUILT-IN CLASSES
				property2 = property[2].ToString().ToLower() + property.Substring(3);

				if (property2.StartsWith("material"))
				{
					property2 = property2.Replace("material", "sharedMaterial");
				}
				
				var prop = GetMember(target, targetT, property2);
				if (prop != null)
				{
					// Debug.Log($"Map success: {property} --> {property2}" );
					return prop;
				}
			}

			// Debug.LogWarning("Property not found: " + target + ":" + property + " :: " + property2);

			return null;
		}

		public class ReflectionCache
		{
			Type type;
			Dictionary<string, FieldInfo> fields = new Dictionary<string, FieldInfo>();
			Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();
			Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();

			FieldInfo GetField(string fieldName)
			{
				if (fields.TryGetValue(fieldName, out var result)) return result;
				result = type.GetField(fieldName, FLAGS);
				fields.Add(fieldName, result);
				return result;
			}

			PropertyInfo GetProperty(string propertyName)
			{
				if (properties.TryGetValue(propertyName, out var result)) return result;
				result = type.GetProperty(propertyName, FLAGS);
				properties.Add(propertyName, result);
				return result;
			}

			MethodInfo GetMethod(string methodName)
			{
				if (methods.TryGetValue(methodName, out var result)) return result;
				result = type.GetMethod(methodName, FLAGS);
				methods.Add(methodName, result);
				return result;
			}
			
			public static Dictionary<Type, ReflectionCache> rcache = new Dictionary<Type, ReflectionCache>();
			public static FieldInfo GetField(Type type, string fieldName)
			{
				if (!rcache.TryGetValue(type, out ReflectionCache cache))
				{
					cache = new ReflectionCache() { type = type };
					rcache.Add(type, cache);
				}
				return cache.GetField(fieldName);
			}
			public static PropertyInfo GetProperty(Type type, string propertyName)
			{
				if (!rcache.TryGetValue(type, out ReflectionCache cache))
				{
					cache = new ReflectionCache() { type = type };
					rcache.Add(type, cache);
				}
				return cache.GetProperty(propertyName);
			}

			public static MethodInfo GetMethod(Type type, string methodName)
			{
				if (!rcache.TryGetValue(type, out ReflectionCache cache))
				{
					cache = new ReflectionCache() { type = type };
					rcache.Add(type, cache);
				}
				return cache.GetMethod(methodName);
			}

		}

		private static VSGetSet GetMember(object target, Type targetT, string property)
		{
			var arr = property.Split('.');
			PropertyInfo propertyInfo = targetT.GetProperty(arr[0], FLAGS);

			List<MemberData> queueFields = new List<MemberData>();
			if (propertyInfo == null)
			{
				FieldInfo fieldInfo = ReflectionCache.GetField(targetT, arr[0]);
				if (fieldInfo == null)
				{
					return null;
				}
				queueFields.Add(new FieldData() { field = fieldInfo });
			}
			else
			{
				queueFields.Add(new PropertyData() { prop = propertyInfo });
			}
			
			MemberData data = null;

			var lastMember = queueFields[0];
			
			for (var i = 1; i < arr.Length; i++)
			{
				if (arr[i] == "Array") // processing an Array or List: Array.data[i].
				{
					var arrayInfo = queueFields[i-1];
					
					i++;
					
					var arrIndex = arr[i].Substring(5, arr[i].Length-6);
					
					//Debug.Log("ArrayIndex: " + arrIndex + " --> " + arrayInfo.MemberType.GetGenericArguments()[0]);
				
					if (arrayInfo.MemberType.IsGenericType)
					{
						data = new ArrayData()
						{
							arrayIndex = int.Parse(arrIndex),
							arrayType = arrayInfo.MemberType.GetGenericArguments()[0]
						};	
					}
					else
					{
						data = new ArrayData()
						{
							arrayIndex = int.Parse(arrIndex),
							arrayType = arrayInfo.MemberType.GetElementType()
						};
					}
					
					queueFields.Add(data);
					lastMember = data;
					continue;
				}
				
				var propInfo1 = ReflectionCache.GetProperty(lastMember.MemberType, arr[i]);
				if (propInfo1 == null)
				{
					var fieldInfo1 = ReflectionCache.GetField(lastMember.MemberType, arr[i]);
					if (fieldInfo1 == null)
					{
						Debug.LogWarning("some thing wrong: " + arr[i] + " --> " + lastMember.MemberType);
						return null;
					}
					data = new FieldData() { field = fieldInfo1 };
				}
				else
				{
					data = new PropertyData() { prop = propInfo1 };
				}
				queueFields.Add(data);
				lastMember = data;
			}
			return new VSGet_Set_Member()
			{
				invokeTarget = target,
				members = queueFields,
				dataType = lastMember.MemberType
			};
		}
	}

	internal class VSGetSet_GO : VSGetSet
	{
		const string IS_ACTIVE = "m_IsActive";
		public const string M_BIT = "m_Bits";
		internal static HashSet<string> SPECIAL_SETTER = new HashSet<string>()
		{
			M_BIT
		};
		internal static HashSet<string> SPECIAL_PROPERTIES = new HashSet<string>()
		{
			IS_ACTIVE
		};
		public string propertyName;

		public VSGetSet_GO(object target, string property)
		{
			invokeTarget = target;
			propertyName = property;
			RefreshDataType();
		}

		VSGetSet_GO RefreshDataType()
		{
			if (propertyName == IS_ACTIVE)
			{
				dataType = typeof(bool);
				return this;
			}

			Debug.LogWarning("Property not implemented : " + propertyName);

			return null;
		}

		public override object GetValue()
		{

			if (invokeTarget == null)
			{
				Debug.LogWarning("Something wrong, invoke target or property is null");
				return null;
			}

			if (propertyName == IS_ACTIVE)
			{
				return (invokeTarget as GameObject).activeSelf;
			}
			Debug.LogWarning("Property not implement :: " + propertyName + " target: " + invokeTarget);
			return null;
		}

		public override bool SetValue(object data)
		{
			if (invokeTarget == null) return false;
			var obj = invokeTarget as Object;

			if (invokeTarget is Object && obj == null)
			{
				Debug.LogWarning(this + " InvokeTarget should not be null !");
				return false;
			}

			if (propertyName == IS_ACTIVE)
			{
				(invokeTarget as GameObject).SetActive((bool)data);
				return true;
			}

			Debug.LogWarning("Property not implement :: " + propertyName + " target: " + invokeTarget);
			return false;
		}
	}
	internal class FieldData : MemberData
	{
		public FieldInfo field;
		public override Type MemberType { get { return field.FieldType; } }
		
		public override object GetValue(object target)
		{
			return field.GetValue(target);
		}

		public override bool SetValue(object target, object value)
		{
			field.SetValue(target, value);
			return true;
		}
	}

	internal class ArrayData : MemberData 
	{
		public int arrayIndex;
		public Type arrayType;

		public override Type MemberType { get { return arrayType; } }

		public override object GetValue(object target)
		{
			return ((IList)target)[arrayIndex];
		}

		public override bool SetValue(object target, object value)
		{
			((IList)target)[arrayIndex] = value;
			return true;
		}
	}

	internal class PropertyData : MemberData
	{
		public PropertyInfo prop;
		public override Type MemberType { get { return prop.PropertyType; } }

		public override object GetValue(object target)
		{
			return prop.GetValue(target, null);
		}

		public override bool SetValue(object target, object value)
		{
			prop.SetValue(target, value, null);
			return true;
		}
	}
	internal class MemberData
	{
		public object data;

		public virtual Type MemberType { get { return null; } }
		public bool isArrayOrList { get 
		{
			var mType = MemberType;
			return mType.IsArray || (mType.IsGenericType && mType.GetGenericTypeDefinition() == typeof(List<>));
		}}

		public virtual object GetValue(object target)
		{
			return null;
		}

		public virtual bool SetValue(object target, object value)
		{
			return false;
		}
	}

	internal class VSGet_Set_Member : VSGetSet
	{
		internal List<MemberData> members;
		public override bool SetValue(object data)
		{
			if (invokeTarget == null) return false;

			var obj = invokeTarget as Object;
			if (invokeTarget is Object && obj == null)
			{
				//Because Unity overloads == null operator
				Debug.LogWarning("Something wrong, invoke target or property is null: " + invokeTarget + ":" + data);
				return false;
			}
			// if(data == null){
			//     Debug.Log(dataType + "  "+ members.Count);
			// }
			if (members.Count == 1)
			{
				members[0].SetValue(invokeTarget, data);
				return true;
			}

			for (var i = 0; i < members.Count; i++)
			{
				var item = members[i];
				if (i == 0)
				{
					item.data = item.GetValue(invokeTarget);
				}
				else
				{
					if (i == members.Count - 1)//last
					{

						item.SetValue(members[i - 1].data, data);
						item.data = data;
						break;
					}
					item.data = item.GetValue(members[i - 1].data);
				}
			}

			for (int i = members.Count - 2; i >= 0; i--)
			{
				var item = members[i];
				members[i + 1].SetValue(item.data, members[i + 1].data);
				if (i == 0)
				{
					item.SetValue(invokeTarget, item.data);
				}
			}

			return true;
		}
	}



}