using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControll : MonoBehaviour
{
    public float rotationSpeed = 1f;
    public float maxLookDownValue = -30f;
    public float maxLookUpValue = 60f;

    public Transform target;
    //public float stomachOffset;
    //public ConfigurableJoint hipJoint;
    //public ConfigurableJoint spineJoint;

    private float mouseX, mouseY;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }


    void FixedUpdate()
    {
        CameraControl();
    }

    private void CameraControl()
    {
        this.mouseX += Input.GetAxis("Mouse X") * rotationSpeed;
        this.mouseY -= Input.GetAxis("Mouse Y") * rotationSpeed;
        this.mouseY = Mathf.Clamp(mouseY, this.maxLookDownValue, this.maxLookUpValue);

        Quaternion rootRotation = Quaternion.Euler(mouseY, mouseX, 0f);

        this.target.rotation = rootRotation;

        //this.hipJoint.targetRotation = Quaternion.Euler(0f, -mouseX, 0f);
        //this.spineJoint.targetRotation = Quaternion.Euler(-mouseY + stomachOffset, 0f, 0f);
    }
}
