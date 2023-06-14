using UnityEngine;

public sealed class FreeCamera : MonoBehaviour
{
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float speedMultiplier = 10.0f;
    [SerializeField] private float mouseSensitivity = 3.0f;
    
    private float pitch;
    private float yaw;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // When the application is not in focus or the mouse cursor is not locked, don't update the camera
        if (!Application.isFocused || Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetMouseButtonDown(0))
                Cursor.lockState = CursorLockMode.Locked;

            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = CursorLockMode.None;

        // Rest of the camera update logic...
        yaw += mouseSensitivity * Input.GetAxis("Mouse X");
        pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");

        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float speed = this.speed * (Input.GetKey(KeyCode.LeftShift) ? speedMultiplier : 1.0f);

        Vector3 move = transform.right * x + transform.forward * z;

        transform.position += move * speed * Time.deltaTime;
    }
}