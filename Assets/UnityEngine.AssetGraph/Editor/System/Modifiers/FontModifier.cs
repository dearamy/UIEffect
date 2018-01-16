using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using UnityEngine.AssetGraph;
using Model=UnityEngine.AssetGraph.DataModel.Version2;

namespace UnityEngine.AssetGraph.Modifiers {

	/*
	 * Code template for Font modifier.
	 * You can copy and create your CustomModifier.
	 */ 

	[Serializable] 
	[CustomModifier("Default Modifier(Font)", typeof(Font))]
	public class FontModifier : IModifier {
		
		public FontModifier () {}

        public bool IsModified (UnityEngine.Object[] assets, List<AssetReference> group) {
			//var anim = assets[0] as Font;

			// Do your work here

			var changed = false;
			return changed; 
		}

        public void Modify (UnityEngine.Object[] assets, List<AssetReference> group) {
			//var anim = assets[0] as Font;

			// Do your work here
		}

		public void OnInspectorGUI (Action onValueChanged) {
			GUILayout.Label("Implement your modifier for this type.");
		}
	}

}