using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luckshot.Callbacks;
using NaughtyAttributes;

public class VisualsModel : MonoBehaviour
{
	[SerializeField]
	private Renderer[] renderers = new Renderer[0];
	public Renderer[] Renderers => renderers;

	public void SetRenderers(Renderer[] inRenderer)
	{
		renderers = inRenderer;
		OnVisualsModelChanged(this);
	}

	[SerializeField]
	private Renderer[] highlightRenderers = null;
	public Renderer[] HighlightRenderers => highlightRenderers;

	public void SetHighlightRenderers(Renderer[] inRenderers)
	{
		highlightRenderers = inRenderers;
		OnVisualsModelChanged(this);
	}

	[SerializeField]
	private Material highlightOverrideMat = null;
	public Material HighlightOverrideMat => highlightOverrideMat;

	public void SetHighlightOverrideMat(Material inHighlightOverrideMat)
	{
		highlightOverrideMat = inHighlightOverrideMat;
		OnVisualsModelChanged(this);
	}

	public event System.Action<VisualsModel> OnVisualsModelChanged = delegate {};

	public LensManagerBool VisibleLens = new LensManagerBool(requests => LensUtils.AllTrue(requests, true));

	private Dictionary<Renderer, bool> rendererToDefaultVisibilityMap = new Dictionary<Renderer, bool>();

	private void Awake()
	{
		bool anyChanged = false;
		if (renderers == null || renderers.Length == 0)
		{
			renderers = CollectValidRenderers().ToArray();
			anyChanged = true;
		}

		if(highlightRenderers == null || highlightRenderers.Length == 0)
		{
			highlightRenderers = renderers;
			anyChanged = true;
		}

		foreach (var renderer in renderers)
			rendererToDefaultVisibilityMap.Add(renderer, renderer.enabled);

		if(anyChanged)
			OnVisualsModelChanged(this);
	}

	private void OnEnable()
	{
		VisibleLens.OnValueChanged += VisibleLens_OnValueChanged;
	}	

	private void OnDisable()
	{
		VisibleLens.OnValueChanged -= VisibleLens_OnValueChanged;
	}

	private void VisibleLens_OnValueChanged(bool visible)
	{
		foreach(var renderer in renderers)
		{
			if(visible)
			{
				if (rendererToDefaultVisibilityMap.TryGetValue(renderer, out bool defaultVisibility))
					renderer.enabled = defaultVisibility;
			}
			else
			{
				renderer.enabled = false;
			}
		}
	}


	[Button("Cache Filters")]
	private void CacheFilters()
	{
		if (Application.IsPlaying(this))
			return;

		renderers = CollectValidRenderers().ToArray();
		highlightRenderers = renderers;
	}

	private List<Renderer> CollectValidRenderers()
	{
		List<Renderer> validRenderers = new List<Renderer>();

		Renderer[] rends = transform.GetComponentsInChildren<Renderer>(true);
		for (int i = 0; i < rends.Length; i++)
		{
			if (rends[i] is MeshRenderer ||
				rends[i] is SkinnedMeshRenderer)
			{
				validRenderers.Add(rends[i]);
			}
		}

		return validRenderers;
	}
}
