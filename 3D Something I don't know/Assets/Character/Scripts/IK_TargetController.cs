using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK_TargetController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform root;
    public float maxDistance = 10f;

    private Transform mainCameraTransform;

    private void Start()
    {
        mainCameraTransform = mainCamera.GetComponent<Transform>();
    }

    private void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, maxDistance) && !(raycastHit.collider.tag == "Ungrabbable"))
        {
            transform.position = raycastHit.point;
            Debug.DrawLine(mainCameraTransform.position, raycastHit.point, Color.red);
        }
        else
        {
            transform.position = ray.GetPoint(maxDistance);
            Debug.DrawLine(mainCameraTransform.position, transform.position, Color.blue);
        }
    }
}
