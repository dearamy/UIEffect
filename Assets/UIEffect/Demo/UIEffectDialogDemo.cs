using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIEffectDialogDemo : MonoBehaviour {

	public void Open()
	{
		gameObject.SetActive(true);
		GetComponent<Animator>().SetTrigger("Open");
	}

	public void Close()
	{
		GetComponent<Animator>().SetTrigger("Close");
	}

	public void Closed()
	{
		gameObject.SetActive(false);
	}

	public void CaptureBackground()
	{
		GetComponentInChildren<UIEffectCapturedImage>().Capture();
	}


	void OnDestroy()
	{
		if (shared_0 != null)
		{
			shared_0.Unload(true);
			shared_0 = null;
		}
	}

	AssetBundle shared_0;

	public void Load(string name)
	{
		UnityEngine.Profiling.Profiler.BeginSample("Load Object");
		if (shared_0 == null)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Load Shared Object");
			shared_0 = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/shared_0");
			UnityEngine.Profiling.Profiler.EndSample();
		}

		UnityEngine.Profiling.Profiler.BeginSample("Load bundle Object");
		var bundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + name);
		UnityEngine.Profiling.Profiler.EndSample();

		UnityEngine.Profiling.Profiler.BeginSample("Load asset");
		var prefab = bundle.LoadAsset<GameObject>(name);
		UnityEngine.Profiling.Profiler.EndSample();

		UnityEngine.Profiling.Profiler.BeginSample("unLoad asset");
		bundle.Unload(false);
		bundle = null;
		UnityEngine.Profiling.Profiler.EndSample();

		UnityEngine.Profiling.Profiler.BeginSample("Instantiate Object");
		var go = GameObject.Instantiate(prefab,transform, false);
		UnityEngine.Profiling.Profiler.EndSample();

		UnityEngine.Profiling.Profiler.EndSample();
	}
}
