﻿using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
#endif

namespace UnityEngine.UI
{
	/// <summary>
	/// UIEffect.
	/// </summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Graphic))]
	[DisallowMultipleComponent]
	public class UIEffect : BaseMeshEffect
#if UNITY_EDITOR
		, ISerializationCallbackReceiver
#endif
	{
		/// <summary>
		/// Additional shadow.
		/// </summary>
		[System.Serializable]
		public class AdditionalShadow
		{
			/// <summary>
			/// How far is the blurring shadow from the graphic.
			/// </summary>
			[Range(0, 1)]
			public float shadowBlur = 0.25f;

			/// <summary>
			/// Shadow effect mode.
			/// </summary>
			public ShadowStyle shadowMode = ShadowStyle.Shadow;

			/// <summary>
			/// Color for the shadow effect.
			/// </summary>
			public Color shadowColor = Color.black;

			/// <summary>
			/// How far is the shadow from the graphic.
			/// </summary>
			public Vector2 effectDistance = new Vector2(1f, -1f);

			/// <summary>
			/// Should the shadow inherit the alpha from the graphic?
			/// </summary>
			public bool useGraphicAlpha = true;
		}

		//################################
		// Constant Members.
		//################################
		public const string shaderName = "UI/Hidden/UI-Effect";

		/// <summary>
		/// Tone effect mode.
		/// </summary>
		public enum ToneMode
		{
			None = 0,
			Grayscale,
			Sepia,
			Nega,
			Pixel,
			Mono,
			Cutoff,
			Hue,
		}

		/// <summary>
		/// Color effect mode.
		/// </summary>
		public enum ColorMode
		{
			None = 0,
			Set,
			Add,
			Sub,
		}

		/// <summary>
		/// Shadow effect mode.
		/// </summary>
		[Obsolete("ShadowMode has been deprecated. Use ShadowStyle instead (UnityUpgradable) -> ShadowStyle", true)]
		public enum ShadowMode
		{
			[Obsolete("ShadowMode.None has been deprecated. Use ShadowStyle.None instead (UnityUpgradable) -> ShadowStyle.None", true)]
			None = 0,
			[Obsolete("ShadowMode.Shadow has been deprecated. Use ShadowStyle.Shadow instead (UnityUpgradable) -> ShadowStyle.Shadow", true)]
			Shadow,
			[Obsolete("ShadowMode.Outline has been deprecated. Use ShadowStyle.Outline instead (UnityUpgradable) -> ShadowStyle.Outline", true)]
			Outline,
			[Obsolete("ShadowMode.Outline8 has been deprecated. Use ShadowStyle.Outline8 instead (UnityUpgradable) -> ShadowStyle.Outline8", true)]
			Outline8,
			[Obsolete("ShadowMode.Shadow3 has been deprecated. Use ShadowStyle.Shadow3 instead (UnityUpgradable) -> ShadowStyle.Shadow3", true)]
			Shadow3,
		}

		/// <summary>
		/// Shadow effect style.
		/// </summary>
		public enum ShadowStyle
		{
			None = 0,
			Shadow,
			Outline,
			Outline8,
			Shadow3,
		}

		/// <summary>
		/// Blur effect mode.
		/// </summary>
		public enum BlurMode
		{
			None = 0,
			Fast,
			Medium,
			Detail,
		}


		//################################
		// Static Members.
		//################################
		static readonly List<UIVertex> s_Verts = new List<UIVertex>();

		//################################
		// Public or Serialize Members.
		//################################
		/// <summary>
		/// Tone effect level between 0(no effect) and 1(complete effect).
		/// </summary>
		public float toneLevel{ get { return m_ToneLevel; } set { m_ToneLevel = Mathf.Clamp(value, 0, 1); SetDirty(); } }

		[SerializeField][Range(0, 1)] float m_ToneLevel = 1;

		/// <summary>
		/// How far is the blurring from the graphic.
		/// </summary>
		public float blur { get { return m_Blur; } set { m_Blur = Mathf.Clamp(value, 0, 2); SetDirty(); } }

		[SerializeField][Range(0, 1)] float m_Blur = 0.25f;

		/// <summary>
		/// How far is the blurring shadow from the graphic.
		/// </summary>
		public float shadowBlur { get { return m_ShadowBlur; } set { m_ShadowBlur = Mathf.Clamp(value, 0, 2); SetDirty(); } }

		[SerializeField][Range(0, 1)] float m_ShadowBlur = 0.25f;

		/// <summary>
		/// Shadow effect mode.
		/// </summary>
		public ShadowStyle shadowStyle { get { return m_ShadowStyle; } set { m_ShadowStyle = value; SetDirty(); } }

		[Obsolete("UIEffect.shadowMode is obsolete, use UIEffect.shadowStyle instead. (UnityUpgradable) -> shadowStyle")]
		public ShadowStyle shadowMode { get { return m_ShadowStyle; } set { m_ShadowStyle = value; SetDirty(); } }

		[SerializeField][FormerlySerializedAs("m_ShadowMode")]  ShadowStyle m_ShadowStyle;

		/// <summary>
		/// Tone effect mode.
		/// </summary>
		public ToneMode toneMode { get { return m_ToneMode; } }

		[SerializeField] ToneMode m_ToneMode;

		/// <summary>
		/// Color effect mode.
		/// </summary>
		public ColorMode colorMode { get { return m_ColorMode; } }

		[SerializeField] ColorMode m_ColorMode;

		/// <summary>
		/// Blur effect mode.
		/// </summary>
		public BlurMode blurMode { get { return m_BlurMode; } }

		[SerializeField] BlurMode m_BlurMode;

		/// <summary>
		/// Color for the shadow effect.
		/// </summary>
		public Color shadowColor { get { return m_ShadowColor; } set { m_ShadowColor = value; SetDirty(); } }

		[SerializeField] Color m_ShadowColor = Color.black;

		/// <summary>
		/// How far is the shadow from the graphic.
		/// </summary>
		public Vector2 effectDistance { get { return m_EffectDistance; } set { m_EffectDistance = value; SetDirty(); } }

		[SerializeField] Vector2 m_EffectDistance = new Vector2(1f, -1f);

		/// <summary>
		/// Should the shadow inherit the alpha from the graphic?
		/// </summary>
		public bool useGraphicAlpha { get { return m_UseGraphicAlpha; } set { m_UseGraphicAlpha = value; SetDirty(); } }

		[SerializeField] bool m_UseGraphicAlpha = true;

		/// <summary>
		/// Color for the color effect.
		/// </summary>
		[Obsolete("UIEffect.color is obsolete, use UIEffect.effectColor instead. (UnityUpgradable) -> effectColor")]
		public Color color { get { return m_EffectColor; } set { m_EffectColor = value; SetDirty(); } }

		/// <summary>
		/// Color for the color effect.
		/// </summary>
		public Color effectColor { get { return m_EffectColor; } set { m_EffectColor = value; SetDirty(); } }

		[SerializeField][FormerlySerializedAs("m_Color")] Color m_EffectColor = Color.white;

		/// <summary>
		/// Effect material.
		/// </summary>
		public virtual Material effectMaterial { get { return m_EffectMaterial; } }

		[SerializeField] Material m_EffectMaterial;


		/// <summary>
		/// Additional Shadows.
		/// </summary>
		public List<AdditionalShadow> additionalShadows { get { return m_AdditionalShadows; } }

		[SerializeField] List<AdditionalShadow> m_AdditionalShadows = new List<AdditionalShadow>();


		//################################
		// MonoBehaior Callbacks.
		//################################
		/// <summary>
		/// This function is called when the object becomes enabled and active.
		/// </summary>
		protected override void OnEnable()
		{
			graphic.material = effectMaterial;
			base.OnEnable();
		}

		/// <summary>
		/// This function is called when the behaviour becomes disabled () or inactive.
		/// </summary>
		protected override void OnDisable()
		{
			graphic.material = null;
			base.OnDisable();
		}

		/// <summary>
		/// Modifies the mesh.
		/// </summary>
		public override void ModifyMesh(VertexHelper vh)
		{
			if (!isActiveAndEnabled)
			{
				return;
			}

			UIVertex vt;
			vh.GetUIVertexStream(s_Verts);

			//================================
			// Effect modify original vertices.
			//================================
			{
				// Pack some effect factors to 1 float.
				Vector2 factor = new Vector2(
									PackToFloat(toneLevel, 0, blur, 0),
									PackToFloat(effectColor.r, effectColor.g, effectColor.b, effectColor.a)
								 );

				for (int i = 0; i < s_Verts.Count; i++)
				{
					vt = s_Verts[i];

					// Set UIEffect prameters to vertex.
					vt.uv1 = factor;
					s_Verts[i] = vt;
				}
			}

			//================================
			// Append shadow vertices.
			//================================
			{
				var inputVertCount = s_Verts.Count;
				var start = 0;
				var end = inputVertCount;

				// Additional Shadows.
				for (int i = additionalShadows.Count - 1; 0 <= i; i--)
				{
					AdditionalShadow shadow = additionalShadows[i];
					ApplyShadow(s_Verts, ref start, ref end, shadow.shadowMode, toneLevel, shadow.shadowBlur, shadow.effectDistance, shadow.shadowColor, shadow.useGraphicAlpha);
				}

				// Shadow.
				ApplyShadow(s_Verts, ref start, ref end, shadowStyle, toneLevel, shadowBlur, effectDistance, shadowColor, useGraphicAlpha);
			}

			vh.Clear();
			vh.AddUIVertexTriangleStream(s_Verts);

			s_Verts.Clear();
		}

#if UNITY_EDITOR

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			var obj = this;
			EditorApplication.delayCall += () =>
			{
				if (Application.isPlaying || !obj)
					return;
				
				var mat = GetMaterial(Shader.Find(shaderName), toneMode, colorMode, blurMode);
				if(m_EffectMaterial == mat)
					return;
					
				graphic.material = m_EffectMaterial = mat;
				EditorUtility.SetDirty(this);
				EditorApplication.delayCall +=AssetDatabase.SaveAssets;
			};
		}

		public static Material GetMaterial(Shader shader, UIEffect.ToneMode tone, UIEffect.ColorMode color, UIEffect.BlurMode blur)
		{
			string variantName = Path.GetFileName(shader.name)
			                     + (0 < tone ? "-" + tone : "")
			                     + (0 < color ? "-" + color : "")
			                     + (0 < blur ? "-" + blur : "");

			var path = AssetDatabase.FindAssets("t:Material " + variantName)
				.Select(x => AssetDatabase.GUIDToAssetPath(x))
				.SingleOrDefault(x => Path.GetFileNameWithoutExtension(x) == variantName);

			return path != null
				? AssetDatabase.LoadAssetAtPath<Material>(path)
					: null;
		}
#endif

		//################################
		// Internal Method.
		//################################
		/// <summary>
		/// Append shadow vertices.
		/// * It is similar to Shadow component implementation.
		/// </summary>
		static void ApplyShadow(List<UIVertex> verts, ref int start, ref int end, ShadowStyle mode, float toneLevel, float blur, Vector2 effectDistance, Color color, bool useGraphicAlpha)
		{
			if (ShadowStyle.None == mode)
				return;

			var factor = new Vector2(
							 PackToFloat(toneLevel, 0, blur, 0),
							 PackToFloat(color.r, color.g, color.b, 1)
						 );

			// Append Shadow.
			ApplyShadowZeroAlloc(s_Verts, ref start, ref end, effectDistance.x, effectDistance.y, factor, color, useGraphicAlpha);

			// Append Shadow3.
			if (ShadowStyle.Shadow3 == mode)
			{
				ApplyShadowZeroAlloc(s_Verts, ref start, ref end, effectDistance.x, 0, factor, color, useGraphicAlpha);
				ApplyShadowZeroAlloc(s_Verts, ref start, ref end, 0, effectDistance.y, factor, color, useGraphicAlpha);
			}

			// Append Outline.
			else if (ShadowStyle.Outline == mode)
			{
				ApplyShadowZeroAlloc(s_Verts, ref start, ref end, effectDistance.x, -effectDistance.y, factor, color, useGraphicAlpha);
				ApplyShadowZeroAlloc(s_Verts, ref start, ref end, -effectDistance.x, effectDistance.y, factor, color, useGraphicAlpha);
				ApplyShadowZeroAlloc(s_Verts, ref start, ref end, -effectDistance.x, -effectDistance.y, factor, color, useGraphicAlpha);
			}

			// Append Outline8.
			else if (ShadowStyle.Outline8 == mode)
			{
				ApplyShadowZeroAlloc(s_Verts, ref start, ref end, effectDistance.x, -effectDistance.y, factor, color, useGraphicAlpha);
				ApplyShadowZeroAlloc(s_Verts, ref start, ref end, -effectDistance.x, effectDistance.y, factor, color, useGraphicAlpha);
				ApplyShadowZeroAlloc(s_Verts, ref start, ref end, -effectDistance.x, -effectDistance.y, factor, color, useGraphicAlpha);
				ApplyShadowZeroAlloc(s_Verts, ref start, ref end, -effectDistance.x, 0, factor, color, useGraphicAlpha);
				ApplyShadowZeroAlloc(s_Verts, ref start, ref end, 0, -effectDistance.y, factor, color, useGraphicAlpha);
				ApplyShadowZeroAlloc(s_Verts, ref start, ref end, effectDistance.x, 0, factor, color, useGraphicAlpha);
				ApplyShadowZeroAlloc(s_Verts, ref start, ref end, 0, effectDistance.y, factor, color, useGraphicAlpha);
			}
		}

		/// <summary>
		/// Append shadow vertices.
		/// * It is similar to Shadow component implementation.
		/// </summary>
		static void ApplyShadowZeroAlloc(List<UIVertex> verts, ref int start, ref int end, float x, float y, Vector2 factor, Color color, bool useGraphicAlpha)
		{
			// Check list capacity.
			var neededCapacity = verts.Count + end - start;
			if (verts.Capacity < neededCapacity)
				verts.Capacity = neededCapacity;

			// Append shadow vertices to the front of list.
			// * The original vertex is pushed backward.
			UIVertex vt;
			for (int i = start; i < end; ++i)
			{
				vt = verts[i];
				verts.Add(vt);

				Vector3 v = vt.position;
				vt.position.Set(v.x + x, v.y + y, v.z);

				Color vertColor = color;
				vertColor.a = useGraphicAlpha ? color.a * vt.color.a / 255 : color.a;
				vt.color = vertColor;

				// Set UIEffect prameters to vertex.
				vt.uv1 = factor;
				verts[i] = vt;
			}

			// Update next shadow offset.
			start = end;
			end = verts.Count;
		}

		/// <summary>
		/// Mark the UIEffect as dirty.
		/// </summary>
		void SetDirty()
		{
			if(graphic)
				graphic.SetVerticesDirty();
		}

		/// <summary>
		/// Pack 4 low-precision [0-1] floats values to a float.
		/// Each value [0-1] has 64 steps(6 bits).
		/// </summary>
		static float PackToFloat(float x, float y, float z, float w)
		{
			const int PRECISION = (1 << 6) - 1;
			return (Mathf.FloorToInt(w * PRECISION) << 18)
			+ (Mathf.FloorToInt(z * PRECISION) << 12)
			+ (Mathf.FloorToInt(y * PRECISION) << 6)
			+ Mathf.FloorToInt(x * PRECISION);
		}
	}
}
