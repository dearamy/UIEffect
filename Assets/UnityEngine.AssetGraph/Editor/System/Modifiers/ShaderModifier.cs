using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using UnityEngine.AssetGraph;
using Model=UnityEngine.AssetGraph.DataModel.Version2;

namespace UnityEngine.AssetGraph.Modifiers {

	/*
	 * Code template for Shader modifier.
	 * You can copy and create your CustomModifier.
	 */ 

	[Serializable] 
	[CustomModifier("Default Modifier(Shader)", typeof(Shader))]
	public class ShaderModifier : IModifier {
		[SerializeField] int maximumLOD;

		public ShaderModifier () {}

        public bool IsModified (UnityEngine.Object[] assets, List<AssetReference> group) {
			var shader = assets[0] as Shader;

			var changed = false;

			if (shader.maximumLOD != this.maximumLOD) {
				changed = true; 
			}

			return changed; 
		}

        public void Modify (UnityEngine.Object[] assets, List<AssetReference> group) {
			var shader = assets[0] as Shader;

			shader.maximumLOD = this.maximumLOD;
		}

		public void OnInspectorGUI (Action onValueChanged) {
			using (new GUILayout.HorizontalScope()) {
				GUILayout.Label("Maximum LOD");

				var changedVal = (int)EditorGUILayout.Slider(this.maximumLOD, 0, 1000);
				if (changedVal != this.maximumLOD) {
					this.maximumLOD = changedVal;
					onValueChanged();
				}
			}
		}
	}
	
}