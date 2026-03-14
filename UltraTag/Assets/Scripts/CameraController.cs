using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 20.0f;
    public float lookSpeed = 2.0f;
    public float zoomSpeed = 10.0f;

    private float _yaw = 0.0f;
    private float _pitch = 0.0f;
    private bool _isRightClickHeld = false;

    void Start()
    {
        _yaw = transform.eulerAngles.y;
        _pitch = transform.eulerAngles.x;
    }

    void Update()
    {
        // Hold right click to look around
        if (Input.GetMouseButtonDown(1))
            _isRightClickHeld = true;
        if (Input.GetMouseButtonUp(1))
            _isRightClickHeld = false;

        if (_isRightClickHeld)
        {
            _yaw   += Input.GetAxis("Mouse X") * lookSpeed;
            _pitch -= Input.GetAxis("Mouse Y") * lookSpeed;
            _pitch  = Mathf.Clamp(_pitch, -89f, 89f);  // prevent flipping upside down
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0.0f);
        }

        // WASD — move relative to where camera is facing
        float moveX = Input.GetAxis("Horizontal");  // A/D
        float moveZ = Input.GetAxis("Vertical");    // W/S

        Vector3 move = transform.right   * moveX
                     + transform.forward * moveZ;
        move.y = 0.0f;  // remove vertical component so WASD stays flat
        transform.position += move * moveSpeed * Time.deltaTime;

        // Q/E — move straight up and down
        if (Input.GetKey(KeyCode.E))
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.Q))
            transform.position -= Vector3.up * moveSpeed * Time.deltaTime;

        // Scroll wheel — zoom (move forward/backward)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.position += transform.forward * scroll * zoomSpeed;
    }
}