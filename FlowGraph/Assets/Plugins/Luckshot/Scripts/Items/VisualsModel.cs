using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luckshot.Callbacks;
using NaughtyAttributes;

public class VisualsModel : MonoBehaviour
{
	[SerializeField]
	private Renderer[] renderers = new Renderer[0];
	public Renderer[] Renderers
	{ get { return renderers; } }

	public void SetRenderers(Renderer[] inRenderer)
	{
		renderers = inRenderer;
		OnVisualsModelChanged(this);
	}

	[SerializeField]
	private Renderer[] highlightRenderers = null;
	public Renderer[] HighlightRenderers
	{ get { return highlightRenderers; } }

	public void SetHighlightRenderers(Renderer[] inRenderers)
	{
		highlightRenderers = inRenderers;
		OnVisualsModelChanged(this);
	}

	[SerializeField]
	private Material highlightOverrideMat = null;
	public Material HighlightOverrideMat
	{ get { return highlightOverrideMat; } }

	public void SetHighlightOverrideMat(Material inHighlightOverrideMat)
	{
		highlightOverrideMat = inHighlightOverrideMat;
		OnVisualsModelChanged(this);
	}

	public event System.Action<VisualsModel> OnVisualsModelChanged = delegate {};

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

		if(anyChanged)
			OnVisualsModelChanged(this);
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
