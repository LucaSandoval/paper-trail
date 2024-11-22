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
    //private bool isZoomingOut = false;   // Track if zooming out is in progress
    private GameObject currentObject; // Object that is currently the subject

    public bool enabled = false; // Whether or not this method should run 

    void Start()
    {
        originalFOV = mainCamera.fieldOfView;
        originalPosition = mainCamera.transform.position;
        originalRotation = mainCamera.transform.rotation;
    }

    void Update()
    {
        if (enabled)
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
                        Vector3 targPos = CalculateOverheadPosition(currentObject.transform);
                        StartCoroutine(ZoomToTarget(targPos, 1f));
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
        
        
    }

    private IEnumerator ZoomToTarget(Vector3 focusPoint, float duration = 1f)
    {
        isZoomingIn = true;
        float elapsedTime = 0f;

        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        float startFOV = mainCamera.fieldOfView;

        Vector3 targetPosition = new Vector3(
            focusPoint.x,                    
            startPosition.y,                 
            focusPoint.z                     
        );

        // Get target rotation (looking down at object)
        Quaternion targetRotation = currentObject.transform.rotation;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Use smoothstep interpolation for more natural movement
            float smoothT = t * t * (3f - 2f * t);

            // Update all camera properties simultaneously
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, zoomFOV, smoothT);
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothT);

            yield return null;
        }

        // Ensure we reach the exact target values
        mainCamera.fieldOfView = zoomFOV;
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;

        isZoomingIn = false;
    }

    // Helper method to calculate the exact position above an object
    private Vector3 CalculateOverheadPosition(Transform targetTransform)
    {
        return new Vector3(
            targetTransform.position.x,
            targetTransform.position.y + hoverDistance,
            targetTransform.position.z
        );
    }



    private IEnumerator ResetZoom()
    {
        //isZoomingOut = true;

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

        //isZoomingOut = false;
    }

    public void RegisterOriginalCameraSettings(Vector3 origPos, Quaternion origRot) {
        originalPosition = origPos;
        originalRotation = origRot;
    }

}
