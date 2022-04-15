using UnityEngine;
using System.Collections;

public class AutoDestroyPFX : MonoBehaviour
{
	[SerializeField, AutoCache(searchChildren = true), UnityEngine.Serialization.FormerlySerializedAs("_ps")]
	private new ParticleSystem particleSystem = null;
	private WaitForSeconds aliveCheckWait = new WaitForSeconds(1f);

	private void OnEnable()
	{
		StartCoroutine(WaitForVFXEnd());
	}

	private IEnumerator WaitForVFXEnd()
	{
		while (particleSystem.IsAlive(true))
			yield return aliveCheckWait;

		Destroy(gameObject);
	}
}