using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderModel : MonoBehaviour
{
    [SerializeField]
    private GameObject smallColliderRoot = null;

    [SerializeField]
    private GameObject largeColliderRoot = null;

    [SerializeField]
    private bool countAsWall = true;
    public bool CountAsWall
    { get { return countAsWall; } }

    private bool useLargeColliders = false;

    private void Awake()
    {
        SetUseLargeColliders(false, true);
    }

    public void SetUseLargeColliders(bool setUseLargeColliders, bool force = false)
    {
        if(useLargeColliders != setUseLargeColliders || force)
        {
            useLargeColliders = setUseLargeColliders;

            if(smallColliderRoot != null)
                smallColliderRoot.SetActive(!useLargeColliders);
            
            if(largeColliderRoot != null)
                largeColliderRoot.SetActive(useLargeColliders);
        }
    }
}
