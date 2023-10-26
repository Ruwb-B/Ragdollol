using UnityEngine;

public class HandGrabbing : MonoBehaviour
{
    //public float breakForce = 1000f;
    public IK_TargetController target_IK;

    private GameObject grabbedObject;
    private Rigidbody hand_RB;
    private bool isGrabbed = false;

    //0 is left mouse button for left hand, 1 is right mouse button for right hand, 2 is the scroller (not used)
    [Range(0, 2)] [SerializeField] private int whichHand = 0;

    private Transform parentOject;
    private bool hasConfigJoint;
    private float length = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        hand_RB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(whichHand))
        {
            if (!isGrabbed)
            {
                GrabObject();
            }
            else
            {
                ReleaseObject();
            }
        }
    }


    private void GrabObject()
    {
        if (grabbedObject == null || grabbedObject.tag == "Ungrabbable")
        {
            return;
        }

        isGrabbed = true;

        grabbedObject.tag = "Ungrabbable";

        length = GetMaxLength(grabbedObject);
        target_IK.maxDistance += length;

        hasConfigJoint = grabbedObject.GetComponent<ConfigurableJoint>();

        if (!hasConfigJoint)
        {
            parentOject = grabbedObject.transform.parent;
            grabbedObject.transform.SetParent(transform);

            ConfigurableJoint joint = grabbedObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = hand_RB;

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;


            JointDrive drive;

            drive = joint.angularXDrive;
            drive.positionSpring = 800f;
            joint.angularXDrive = drive;

            drive = joint.angularYZDrive;
            drive.positionSpring = 800f;
            joint.angularYZDrive = drive;
        }
        else
        {
            FixedJoint joint = grabbedObject.AddComponent<FixedJoint>();
            joint.connectedBody = hand_RB;
        }

    }

    private void ReleaseObject()
    {
        if (!hasConfigJoint)
        {
            Destroy(grabbedObject.GetComponent<ConfigurableJoint>());
            grabbedObject.transform.SetParent(parentOject);
        }
        else
        {
            Destroy(grabbedObject.GetComponent<FixedJoint>());
        }

        target_IK.maxDistance -= length;

        grabbedObject.tag = "Untagged";
        
        isGrabbed = false;
        parentOject = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isGrabbed)
        {
            grabbedObject = other.gameObject;
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (!isGrabbed)
    //    {
    //        grabbedObject = collision.gameObject;
    //        Debug.Log("Touch Object");
    //    }
    //}

    private void OnTriggerExit(Collider other)
    {
        if (!isGrabbed)
        {
            grabbedObject = null;
        }
    }

    //private void OnCollisionExit(Collision collision)
    //{
    //    if (!isGrabbed)
    //    {
    //        grabbedObject = null;
    //    }
    //}

    private float GetMaxLength(GameObject obj)
    {
        Vector3 size = obj.GetComponent<Collider>().bounds.size;
        float max = size.x;
        if(size.y > max)
        {
            max = size.y;
        }
        if(size.z > max)
        {
            max = size.z;
        }
        return max;
    }
}
