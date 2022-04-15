using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Luckshot/Path Mesh/Contour Data")]
public class PathMesh_ContourData : ScriptableObject
{
    public PathMesh.OffsetStrip[] offsetStrips = null;
}
