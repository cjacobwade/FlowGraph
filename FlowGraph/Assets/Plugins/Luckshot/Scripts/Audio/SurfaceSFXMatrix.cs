using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SurfaceSFXPair
{
    public PhysicMaterial material = null;

	//[FMODUnity.EventRef]
	public string audioEvent = string.Empty;
}

[CreateAssetMenu(fileName = "SurfaceSFXMatrix", menuName = "Luckshot/Surface SFX Matrix")]
public class SurfaceSFXMatrix : ScriptableObject
{
	//[SerializeField, FMODUnity.EventRef]
    [SerializeField]
	private string fallbackEvent = string.Empty;

	public SurfaceSFXPair[] sfxPairs = null;

    public string GetAudioEvent(PhysicMaterial material)
    {
        string audioEvent = fallbackEvent;

        for(int i =0; i < sfxPairs.Length; i++)
        {
            if(sfxPairs[i].material == material)
            {
                audioEvent = sfxPairs[i].audioEvent;
                break;
            }
        }

        return audioEvent;
    }
}
