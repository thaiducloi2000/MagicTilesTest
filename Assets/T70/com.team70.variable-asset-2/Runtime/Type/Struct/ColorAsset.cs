using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nano.va2
{
	[CreateAssetMenu(fileName = "new.color.asset", menuName = "VAsset2/Primity/Color", order = 110)]
	public class ColorAsset : VAssetT<Color>, ISupportJson
	{
		override protected bool IsEqualT(Color v1, Color v2)
		{
			return v1 == v2;
		}
		
		public bool FromJson(string json)
		{
			throw new System.NotImplementedException();
		}

		public string ToJson()
		{
			throw new System.NotImplementedException();
		}
	}
}
