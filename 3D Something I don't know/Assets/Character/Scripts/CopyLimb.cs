using UnityEngine;

public class CopyLimb : MonoBehaviour
{
    [SerializeField] private Transform targetLimb;
    private Configurable_joint m_Configurable_joint;

    Quaternion targetInitialRotation;

    // Start is called before the first frame update
    void Start()
    {
        this.m_Configurable_joint = this.GetComponent<Configurable_joint>();
        this.targetInitialRotation = this.targetLimb.transform.localRotation;
    }

    private void FixedUpdate()
    {
        this.m_Configurable_joint.targetRotation = CopyRotation();
    }

    private Quaternion CopyRotation()
    {
        return Quaternion.Inverse(this.targetLimb.localRotation) * this.targetInitialRotation;
    }
}
