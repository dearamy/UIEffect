using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using UnityEngine.AssetGraph;
using Model=UnityEngine.AssetGraph.DataModel.Version2;

namespace UnityEngine.AssetGraph.Modifiers {
	
	/*
	 * Code template for Animator modifier.
	 * You can copy and create your CustomModifier.
	 */ 
	[Serializable] 
	[CustomModifier("Default Modifier(Animator)", typeof(Animator))]
	public class AnimatorModifier : IModifier {
		
		public AnimatorModifier () {}

        public bool IsModified (UnityEngine.Object[] assets, List<AssetReference> group) {
			//var anim = assets[0] as Animator;

			// Do your work here

			var changed = false;
			return changed; 
		}

        public void Modify (UnityEngine.Object[] assets, List<AssetReference> group) {
			//var anim = assets[0] as Animator;

			// Do your work here
		}

		public void OnInspectorGUI (Action onValueChanged) {
			GUILayout.Label("Implement your modifier for this type.");
		}
	}
}