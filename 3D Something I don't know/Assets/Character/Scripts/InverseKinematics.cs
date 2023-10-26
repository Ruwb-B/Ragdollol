#if UNITY_EDITOR
using UnityEditor;
#endif
//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public class InverseKinematics : MonoBehaviour
{
    /// <summary>
    /// Chain length of bones
    /// </summary>
    public int chainLength = 3;

    /// <summary>
    /// Target the chain should bent to
    /// </summary>
    public Transform target;
    public Transform pole;

    /// <summary>
    /// Solver iterations per update
    /// </summary>
    [Header("Solver Parameters")]
    public int iterations = 10;

    /// <summary>
    /// Distance when the solver stops
    /// </summary>
    public float delta = 0.001f;

    /// <summary>
    /// Strength of going back to the start position.
    /// </summary>
    [Range(0, 1)]
    public float snapBackStrength = 1f;

    /// <summary>
    /// Other
    /// </summary>
    protected float[] bonesLength; // Target to Origin
    protected float completeLength;
    protected Transform[] bones;
    protected Vector3[] positions;
    protected Vector3[] startDirectionSucc;
    protected Quaternion[] startRotationBone;
    protected Quaternion startRotationTarget;
    protected Transform root;

    // Called before the first frame update
    private void Awake()
    {
        Init();
    }

    void Init()
    {
        // Initial arrays
        bones = new Transform[chainLength + 1];
        positions = new Vector3[chainLength + 1];
        bonesLength = new float[chainLength];
        startDirectionSucc = new Vector3[chainLength + 1];
        startRotationBone = new Quaternion[chainLength + 1];

        // Find root
        root = this.transform;
        for (int i = 0; i <= chainLength; i++)
        {
            if (root == null)
            {
                throw new UnityException("The chain value is longer than the ancestor chain!");
            }
            root = root.parent;
        }

        // Init target
        if (target == null)
        {
            target = new GameObject(gameObject.name + " Target").transform;
            SetPositionRootSpace(target, GetPositionRootSpace(transform));
        }
        startRotationTarget = GetRotationRootSpace(target);


        // Init data
        Transform current = this.transform;
        completeLength = 0;
        for (int i = bones.Length - 1; i >= 0; i--)
        {
            bones[i] = current;
            startRotationBone[i] = GetRotationRootSpace(current);

            if (i == bones.Length - 1)
            {
                //leaf
                startDirectionSucc[i] = GetPositionRootSpace(target) - GetPositionRootSpace(current);
            }
            else
            {
                //mid bone
                startDirectionSucc[i] = GetPositionRootSpace(bones[i + 1]) - GetPositionRootSpace(current);
                bonesLength[i] = startDirectionSucc[i].magnitude;
                completeLength += bonesLength[i];
            }

            current = current.parent;
        }

    }


    // Update is called once per frame
    private void LateUpdate()
    {
        ResolveIK();
    }


    private void ResolveIK()
    {
        if (target == null)
        {
            return;
        }

        if (bonesLength.Length != chainLength)
        {
            Init();
        }

        // Fabric

        //  root
        //  (bone0) (bonelen 0) (bone1) (bonelen 1) (bone2)...
        //     x------------------x--------------------x---...

        //get position
        for (int i = 0; i < bones.Length; i++)
        {
            positions[i] = GetPositionRootSpace(bones[i]);
        }

        Vector3 targetPosition = GetPositionRootSpace(target);
        Quaternion targetRotation = GetRotationRootSpace(target);

        //1st is possible to reach?
        if ((targetPosition - GetPositionRootSpace(bones[0])).sqrMagnitude >= completeLength * completeLength)
        {
            //the target is out of reach
            //just strech it
            Vector3 direction = (targetPosition - positions[0]).normalized;

            //set everything after root
            for (int i = 1; i < positions.Length; i++)
            {
                positions[i] = positions[i - 1] + direction * bonesLength[i - 1];
            }
        }
        else
        {
            for (int i = 0; i < positions.Length - 1; i++)
            {
                positions[i + 1] = Vector3.Lerp(positions[i + 1], positions[i] + startDirectionSucc[i], snapBackStrength);
            }

            for (int iteration = 0; iteration < iterations; iteration++)
            {

                //back
                for (int i = positions.Length - 1; i > 0; i--)
                {
                    if (i == positions.Length - 1)
                    {
                        positions[i] = targetPosition; //set it to target
                    }
                    else
                    {
                        positions[i] = positions[i + 1] + (positions[i] - positions[i + 1]).normalized * bonesLength[i]; //set in line on distance
                    }
                }

                //forward
                for (int i = 1; i < positions.Length; i++)
                {
                    positions[i] = positions[i - 1] + (positions[i] - positions[i - 1]).normalized * bonesLength[i - 1];
                }

                //close enough?
                if ((positions[positions.Length - 1] - targetPosition).sqrMagnitude < delta * delta)
                {
                    break;
                }
            }
        }

        //move toward pole
        if (pole != null)
        {
            Vector3 polePosition = GetPositionRootSpace(pole);
            for (int i = 1; i < positions.Length - 1; i++)
            {
                Plane plane = new Plane(positions[i + 1] - positions[i - 1], positions[i - 1]);
                Vector3 projectedPole = plane.ClosestPointOnPlane(polePosition);
                Vector3 projectedBone = plane.ClosestPointOnPlane(positions[i]);
                float angle = Vector3.SignedAngle(projectedBone - positions[i - 1], projectedPole - positions[i - 1], plane.normal);
                positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (positions[i] - positions[i - 1]) + positions[i - 1];
            }
        }

        //set position & rotation
        for (int i = 0; i < positions.Length; i++)
        {
            if (i == positions.Length - 1)
            {
                SetRotationRootSpace(bones[i], Quaternion.Inverse(targetRotation) * startRotationTarget * Quaternion.Inverse(startRotationBone[i]));
            }
            else
            {
                SetRotationRootSpace(bones[i], Quaternion.FromToRotation(startDirectionSucc[i], positions[i + 1] - positions[i]) * Quaternion.Inverse(startRotationBone[i]));
            }
            SetPositionRootSpace(bones[i], positions[i]);
        }
    }


    private Vector3 GetPositionRootSpace(Transform current)
    {
        if (root == null)
        {
            return current.position;
        }
        else
        {
            return Quaternion.Inverse(root.rotation) * (current.position - root.position);
        }
    }


    private void SetPositionRootSpace(Transform current, Vector3 position)
    {
        if (root == null)
        {
            current.position = position;
        }
        else
        {
            current.position = root.rotation * position + root.position;
        }
    }


    private Quaternion GetRotationRootSpace(Transform current)
    {
        //inverse(after) * before >= rot: before -> after
        if (root == null)
        {
            return current.rotation;
        }
        else
        {
            return Quaternion.Inverse(current.rotation) * root.rotation;
        }
    }


    private void SetRotationRootSpace(Transform current, Quaternion rotation)
    {
        if(root == null)
        {
            current.rotation = rotation;
        }
        else
        {
            current.rotation = root.rotation * rotation;
        }
    }


    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        var current = this.transform;
        for (int i = 0; i < chainLength && current != null && current.parent != null; i++)
        {
            var scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
            Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
            Handles.color = Color.green;
            Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
            current = current.parent;
        }
#endif
    }
}
