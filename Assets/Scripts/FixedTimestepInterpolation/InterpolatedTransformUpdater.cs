using UnityEngine;

/* Used to allow a later script execution order for FixedUpdate than in GameplayTransform.
   It is critical this script runs after all other scripts that modify a transform from FixedUpdate.
 */
public class InterpolatedTransformUpdater : MonoBehaviour
{
    private InterpolatedTransform _interpolatedTransform;

    private void Awake() => _interpolatedTransform = GetComponent<InterpolatedTransform>();

    private void FixedUpdate() => _interpolatedTransform.LateFixedUpdate();
}