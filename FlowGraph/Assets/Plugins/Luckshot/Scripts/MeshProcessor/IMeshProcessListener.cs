using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IMeshProcessListener
{
	public void NotifyMeshProcessorChanged(MeshProcessor meshProcessor);
}
