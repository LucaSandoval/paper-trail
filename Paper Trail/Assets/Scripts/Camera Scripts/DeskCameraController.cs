using UnityEngine;

public class DeskCameraController : MonoBehaviour
{
    public float sensitivity = 5f;         // Sensitivity of mouse movement
    public float rotationSmoothness = 10f; // Smoothness of camera rotation
    public Vector2 xRotationLimits = new Vector2(-15f, 15f); // Min and max X-axis rotation (local pitch)
    public Vector2 yRotationLimits = new Vector2(-30f, 30f); // Min and max Y-axis rotation (local yaw)

    private Vector2 currentRotation;      // Current local rotation values
    private Vector2 targetRotation;       // Target local rotation values
    private Quaternion initialRotation;   // Initial rotation of the camera

    void Start()
    {
        // Store the initial local rotation of the camera
        initialRotation = transform.localRotation;

        // Initialize the current and target rotations as zero (relative to initial rotation)
        currentRotation = targetRotation = Vector2.zero;
    }

    void Update()
    {
        HandleInput();
        SmoothRotate();
    }

    private void HandleInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("Paper"))
        {
            // Do not apply the pan!
            // Reset Camera!

            SmoothRotate();
            transform.localRotation = initialRotation;
        }
        else
        {
            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Update target rotation based on mouse input and sensitivity
            targetRotation.x -= mouseY * sensitivity; // Invert to match expected pitch control
            targetRotation.y += mouseX * sensitivity;

            // Clamp rotation within the defined local limits
            targetRotation.x = Mathf.Clamp(targetRotation.x, xRotationLimits.x, xRotationLimits.y);
            targetRotation.y = Mathf.Clamp(targetRotation.y, yRotationLimits.x, yRotationLimits.y);
        }
       
    }

    private void SmoothRotate()
    {
        // Smoothly interpolate the current rotation towards the target rotation
        currentRotation = Vector2.Lerp(currentRotation, targetRotation, Time.deltaTime * rotationSmoothness);

        // Apply the smoothed rotation relative to the initial rotation
        Quaternion xQuat = Quaternion.AngleAxis(currentRotation.x, Vector3.right);
        Quaternion yQuat = Quaternion.AngleAxis(currentRotation.y, Vector3.up);
        transform.localRotation = initialRotation * yQuat * xQuat;
    }
}
