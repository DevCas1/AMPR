using System;
using UnityEngine;
using NaughtyAttributes;

/* Interpolates an object to the transform at the latest FixedUpdate from the transform at the previous FixedUpdate.
   It is critical this script's execution order is set before all other scripts that modify a transform from FixedUpdate.
 */
[RequireComponent(typeof(InterpolatedTransformUpdater))]
public class InterpolatedTransform : MonoBehaviour
{
    [Flags]
    public enum LerpProperties
    {
        Position = 1 << 0,
        Rotation = 1 << 1,
        Scale = 1 << 2,
    }

    // Stores transform data
    private struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public TransformData(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }

    private const int LAST_TRANSFOR_SIZE = 2;

    [SerializeField, Tooltip("Specify which properties to lerp using the InterpolatedTransform component")]
    private LerpProperties _lerpProperties = (LerpProperties)~0;
    [SerializeField, EnableIf(nameof(_lerpPosition))]
    private Space _positionSpace;
    [SerializeField, EnableIf(nameof(_lerpRotation))]
    private Space _rotationSpace;

    private TransformData[] _lastTransforms; // Stores the transform of the object from the last two FixedUpdates
    private int _newTransformIndex; // Keeps track of which index is storing the newest value.

    private bool _lerpPosition => (_lerpProperties & LerpProperties.Position) == LerpProperties.Position;
    private bool _lerpRotation => (_lerpProperties & LerpProperties.Rotation) == LerpProperties.Rotation;

    // Initializes the list of previous orientations
    private void OnEnable() => ForgetPreviousTransforms();

    /* Resets the previous transform list to store only the objects's current transform. Useful to prevent
       interpolation when an object is teleported, for example.
     */
    public void ForgetPreviousTransforms()
    {
        Vector3 position = _positionSpace == Space.World ? transform.position : transform.localPosition;
        Quaternion rotation = _rotationSpace == Space.World ? transform.rotation : transform.localRotation;

        TransformData t = new TransformData(position, rotation, transform.localScale);

        _lastTransforms = new TransformData[LAST_TRANSFOR_SIZE];
        _lastTransforms[0] = t;
        _lastTransforms[1] = t;
        _newTransformIndex = 0;
    }

    // Runs after ofther scripts to save the objects's final transform & Set new index to the older stored transform.
    public void LateFixedUpdate() => _lastTransforms[_newTransformIndex = OldTransformIndex()] = new TransformData(_positionSpace == Space.World ? transform.position : transform.localPosition,
                                                                                                                   _rotationSpace == Space.World ? transform.rotation : transform.localRotation,
                                                                                                                   transform.localScale);

    //Interpolates the object transform to the latest FixedUpdate's transform
    private void Update()
    {
        TransformData newestTransform = _lastTransforms[_newTransformIndex];
        TransformData olderTransform = _lastTransforms[OldTransformIndex()];

        if ((_lerpProperties & LerpProperties.Position) == LerpProperties.Position)
        {
            Vector3 newPos = Vector3.Lerp(olderTransform.position, newestTransform.position, InterpolationController.InterpolationFactor);

            if (_positionSpace == Space.World)
                transform.position = newPos;
            else
                transform.localPosition = newPos;
        }

        if ((_lerpProperties & LerpProperties.Rotation) == LerpProperties.Rotation)
        {
            Quaternion newRot = Quaternion.Slerp(olderTransform.rotation, newestTransform.rotation, InterpolationController.InterpolationFactor);

            if (_rotationSpace == Space.World)
                transform.rotation = newRot;
            else
                transform.localRotation = newRot;
        }

        if ((_lerpProperties & LerpProperties.Scale) == LerpProperties.Scale)
            transform.localScale = Vector3.Lerp(olderTransform.scale, newestTransform.scale, InterpolationController.InterpolationFactor);
    }

    // The index of the older stored transform
    private int OldTransformIndex() => _newTransformIndex == 0 ? 1 : 0;
}