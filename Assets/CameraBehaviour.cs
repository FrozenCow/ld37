using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{


	void Update ()
	{
	  var player = GameObject.FindGameObjectsWithTag("Player").SingleOrDefault(o => o.activeInHierarchy);
	  if (player == null)
	    return;
	  var camera = GetComponent<Camera>();
    var isPaused = Mathf.Approximately(Time.timeScale, 0.0f);

    // Rotation
    var currentRotation = transform.eulerAngles.z;
	  var gravityRotation = Vector2.down.GetAngleToward(Physics2D.gravity.normalized);
    var newRotation = Mathf.MoveTowardsAngle(currentRotation, gravityRotation, 5.0f);
	  var deltaRotation = newRotation - currentRotation;
    //var deltaRotation = Mathf.Min(Mathf.Max(currentRotation, -1.0f), 1.0f);
    transform.Rotate(0, 0, deltaRotation, Space.World);

    // Position
	  var targetPosition = isPaused
	    ? Vector2.zero
	    : player.transform.position.ToVector2();

	  var targetOrtho = isPaused
	    ? 3
	    : 1;

    // Combine position and ortho, so that we can move toward a single vector (all interpolating values are in sync).
    var combinedCurrentVector = new Vector3(transform.position.x, transform.position.y, camera.orthographicSize);
    var combinedTargetVector = new Vector3(targetPosition.x, targetPosition.y, targetOrtho);

    var combinedNewVector = Vector3.MoveTowards(combinedCurrentVector, combinedTargetVector, 0.2f);

    transform.position = new Vector3(combinedNewVector.x, combinedNewVector.y, transform.position.z);
	  camera.orthographicSize = combinedNewVector.z;
	}
}
