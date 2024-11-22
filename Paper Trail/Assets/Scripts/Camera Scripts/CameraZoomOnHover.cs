using UnityEngine;
using System.Collections;
public class CameraZoomOnHover : MonoBehaviour
{
    public Camera mainCamera;
    public float zoomSpeed = 5f;          // Speed of zooming
    public float zoomFOV = 30f;           // Field of view to zoom in to
    public float hoverDistance = 0.3f;   // Distance from object during zoom
    private float originalFOV = 60f;     // Original field of view
    private Vector3 originalPosition;    // Original camera position
    private Quaternion originalRotation; // Original camera rotation
    //public Transform targetObject;       // The object to zoom in on

    private bool isZoomingIn = false;    // Track if zooming in is in progress
    private bool isZoomingOut = false;   // Track if zooming out is in progress
    private GameObject currentObject; // Object that is currently the subject

    void Start()
    {
        originalFOV = mainCamera.fieldOfView;
        originalPosition = mainCamera.transform.position;
        originalRotation = mainCamera.transform.rotation;
    }

    void Update()
    {
        if (isZoomingIn)
        {
            return; // Dont override IEnumerator during run time
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject hitObject = hit.transform.gameObject;

            if (hitObject.CompareTag("Paper"))
            {
                // If it's a new object, switch focus
                if (hitObject != currentObject)
                {
                    currentObject = hitObject; // Update the current object
                    StopAllCoroutines();      // Stop any ongoing zoom animations
                    StartCoroutine(ZoomToTarget(currentObject.transform.position));
                }
            }
            else if (currentObject != null)
            {
                // If no object is hit but we had a focused object, reset the zoom
                currentObject = null; // Clear the current object
                StopAllCoroutines();  // Stop any ongoing zoom animations
                StartCoroutine(ResetZoom());
            }
        }
        
    }

    private IEnumerator ZoomToTarget(Vector3 focusPoint)
    {
        isZoomingIn = true;

        // Store the starting position and rotation of the camera
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        // Calculate the target position and target rotation
        Vector3 direction = (mainCamera.transform.position - focusPoint).normalized;
        Vector3 targetPosition = focusPoint + direction * hoverDistance;
        Quaternion targetRotation = Quaternion.LookRotation(focusPoint - mainCamera.transform.position);

        while (Mathf.Abs(mainCamera.fieldOfView - zoomFOV) > 0.01f)
        {
            // Smoothly adjust the camera's field of view
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, zoomFOV, Time.deltaTime * zoomSpeed);

            // Smoothly move the camera into position
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, Time.deltaTime * zoomSpeed);

            // Smoothly rotate the camera toward the target rotation
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * zoomSpeed);

            yield return null;
        }

        // Snap to final values to avoid floating-point inaccuracies
        mainCamera.fieldOfView = zoomFOV;
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;

        isZoomingIn = false;
    }


    private IEnumerator ResetZoom()
    {
        isZoomingOut = true;

        while (Mathf.Abs(mainCamera.fieldOfView - originalFOV) > 0.01f)
        {
            // Smoothly reset the camera's field of view
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, originalFOV, Time.deltaTime * zoomSpeed);

            // Smoothly reset the camera's position and rotation
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, originalPosition, Time.deltaTime * zoomSpeed);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, originalRotation, Time.deltaTime * zoomSpeed);

            yield return null;
        }

        // Snap to original values to avoid floating-point inaccuracies
        mainCamera.fieldOfView = originalFOV;
        mainCamera.transform.position = originalPosition;
        mainCamera.transform.rotation = originalRotation;

        isZoomingOut = false;
    }
}
