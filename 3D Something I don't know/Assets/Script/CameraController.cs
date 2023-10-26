using UnityEngine;
using Cinemachine;
 
public class CameraController : MonoBehaviour, AxisState.IInputAxisProvider
{
    public string HorizontalInput = "Mouse X";
    public string VerticalInput = "Mouse Y";

    private bool isLocked = false;

    private void Start()
    {
        //Hide the Cursor
        Cursor.lockState = CursorLockMode.Locked;
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            isLocked = !isLocked;
            if (isLocked)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public float GetAxisValue(int axis)
    {
        if (isLocked)
        {
            return 0.0f;
        }

        switch (axis)
        {
            case 0: return Input.GetAxis(HorizontalInput);
            case 1: return Input.GetAxis(VerticalInput);
            default: return 0;
        }
    }
}
