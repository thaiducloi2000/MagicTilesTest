using System.Collections.Generic;
using UnityEngine;

namespace com.team70
{
	public class IndexedString
	{
		public struct Token
		{
			public int stIndex;
			public int edIndex;
		}

		public string source;
		public List<Token> tokens;
		
		public bool GetTokens<T0>(ref T0 t0)
		{
			var tk = tokens[0];
			return Parser.TryParse(source, tk.stIndex, tk.edIndex, ref t0);
		}
	}
}