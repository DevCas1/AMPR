using UnityEngine;

/* Manages the interpolation factor that InterpolatedTransforms use to position themselves.
   Must be attached to a single object in each scene, such as a gamecontroller.
   It is critical this script's execution order is set before InterpolatedTransform.
 */
public class InterpolationController : MonoBehaviour
{
    private const int LAST_FIXED_UPDATE_SIZE = 2;

    private float[] _lastFixedUpdateTimes; // Stores the last two times at which a FixedUpdate occured.
    private int _newTimeIndex; // Keeps track of which index is storing the newest value.

    // The proportion of time since the previous FixedUpdate relative to fixedDeltaTime
    private static float _interpolationFactor;
    public static float InterpolationFactor
    {
        get => _interpolationFactor;
    }

    // Initializes the array of FixedUpdate times.
    public void Start()
    {
        _lastFixedUpdateTimes = new float[LAST_FIXED_UPDATE_SIZE];
        _newTimeIndex = 0;
    }

    // Record the time of the current FixedUpdate and remove the oldest value.
    public void FixedUpdate() => _lastFixedUpdateTimes[_newTimeIndex = OldTimeIndex()] = Time.fixedTime; // Set new index to the older stored time & store new time.

    // Sets the interpolation factor
    public void Update()
    {
        float newerTime = _lastFixedUpdateTimes[_newTimeIndex];
        float olderTime = _lastFixedUpdateTimes[OldTimeIndex()];

        _interpolationFactor = newerTime != olderTime ? (Time.time - newerTime) / (newerTime - olderTime) : 1;
    }

    // The index of the older stored time
    private int OldTimeIndex() => (_newTimeIndex == 0 ? 1 : 0);
}