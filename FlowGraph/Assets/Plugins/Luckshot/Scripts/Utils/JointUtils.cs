using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JointUtils
{
	public static void SetTargetRotation(this ConfigurableJoint joint, Quaternion targetRotation)
	{
		Vector3 right = joint.axis;
		Vector3 up = joint.secondaryAxis;
		Vector3 forward = -Vector3.Cross(up, right).normalized;

		Quaternion worldToJointRotation = Quaternion.LookRotation(forward, up);

		Quaternion appliedRotation = Quaternion.Inverse(worldToJointRotation);
		appliedRotation *= Quaternion.Inverse(targetRotation) * joint.transform.localRotation;
		appliedRotation *= worldToJointRotation;

		joint.targetRotation = appliedRotation;
	}
}
