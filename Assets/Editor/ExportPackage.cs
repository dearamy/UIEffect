using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using BlurMode = UnityEngine.UI.UIEffect.BlurMode;
using ColorMode = UnityEngine.UI.UIEffect.ColorMode;
using ToneMode = UnityEngine.UI.UIEffect.ToneMode;
using UnityEngine.UI;
using System.IO;

namespace UnityEditor.UI
{
	public static class ExportPackage
	{
		const string kPackageName = "UIEffect.unitypackage";
		static readonly string[] kAssetPathes =
		{
			"Assets/UIEffect",
		};

		[MenuItem("Export Package/" + kPackageName)]
		[InitializeOnLoadMethod]
		static void Export()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			// Export package
			AssetDatabase.ExportPackage(kAssetPathes, kPackageName, ExportPackageOptions.Recurse | ExportPackageOptions.Default);
			UnityEngine.Debug.Log("Export successfully : " + kPackageName);
		}


		[MenuItem("Export Package/Generate Material")]
		static void Export2()
		{
			// Export materials.
			AssetDatabase.StartAssetEditing();
			CreateMaterialVariants(
				Shader.Find(UIEffect.shaderName)
				, (ToneMode[])Enum.GetValues(typeof(ToneMode))
				, (ColorMode[])Enum.GetValues(typeof(ColorMode))
				, (BlurMode[])Enum.GetValues(typeof(BlurMode))
			);

			CreateMaterialVariants(
				Shader.Find(UIEffectCapturedImage.shaderName)
				, new []{ ToneMode.None, ToneMode.Grayscale, ToneMode.Sepia, ToneMode.Nega, ToneMode.Pixel, ToneMode.Hue, }
				, (ColorMode[])Enum.GetValues(typeof(ColorMode))
				, (BlurMode[])Enum.GetValues(typeof(BlurMode))
			);

			AssetDatabase.StopAssetEditing();
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		static void CreateMaterialVariants(Shader shader, ToneMode[] tones, ColorMode[] colors, BlurMode[] blurs)
		{
			var combinations = (from tone in tones
				from color in colors
				from blur in blurs
				select new { tone, color, blur }).ToArray();

			for (int i = 0; i < combinations.Length; i++)
			{
				var comb = combinations[i];
				EditorUtility.DisplayProgressBar("Genarate Effect Material", UIEffect.GetVariantName(shader, comb.tone, comb.color, comb.blur), (float)i/combinations.Length);
				GetOrCreateMaterialVariant(shader, comb.tone, comb.color, comb.blur);
			}
			EditorUtility.ClearProgressBar();
		}

		public static Material GetOrCreateMaterialVariant(Shader shader, UIEffect.ToneMode tone, UIEffect.ColorMode color, UIEffect.BlurMode blur)
		{
			bool isDefault = 0 == tone && 0 == color && 0 == blur;
			Material mat = UIEffect.GetMaterial(shader, tone, color, blur);

			if (!mat)
			{
				mat = new Material(shader);

				if (0 < tone)
					mat.EnableKeyword("UI_TONE_" + tone.ToString().ToUpper());
				if (0 < color)
					mat.EnableKeyword("UI_COLOR_" + color.ToString().ToUpper());
				if (0 < blur)
					mat.EnableKeyword("UI_BLUR_" + blur.ToString().ToUpper());

				string defaultName = Path.GetFileName(shader.name);
				mat.name = UIEffect.GetVariantName(shader, tone, color, blur);

				string materialPath = "Assets/UIEffect/Materials/" + defaultName + ".mat";
				if (isDefault)
				{
					Directory.CreateDirectory(Path.GetDirectoryName(materialPath));
					AssetDatabase.CreateAsset(mat, materialPath);
					AssetDatabase.SaveAssets();
					//					AssetDatabase.Refresh();
				}
				else
				{
					mat.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy;
					AssetDatabase.AddObjectToAsset(mat, materialPath);
					//					AssetDatabase.Refresh();
				}
			}
			return mat;
		}
	}
}