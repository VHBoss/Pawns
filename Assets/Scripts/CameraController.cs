using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float m_MoveSpeed = 0.2f;
    [SerializeField] float m_RotateSpeed = 5;
    [SerializeField] float m_ZoomSpeed = 1;
    [SerializeField] float m_SmoothTime = 0.1f;
    [SerializeField] float m_MinZoom = -11;
    [SerializeField] float m_MaxZoom = 0;
    [SerializeField] Transform m_MoveCamera;
    [SerializeField] Transform m_RotationCamera;
    [SerializeField] Transform m_ZoomCamera;

    private Vector3 m_MoveTarget;
    private Vector3 m_ZoomTarget;
    private Vector3 m_Velocity = Vector3.zero;
    private Vector3 m_ZoomVelocity = Vector3.zero;

    void Start()
    {
        m_MoveTarget = m_MoveCamera.position;
        m_ZoomTarget = new Vector3(0, 0, m_ZoomCamera.localPosition.z);
    }

    void Update()
    {
        UpdateCamera();
        UpdateControls();
    }

    void UpdateControls()
    {

        if (Input.GetMouseButton(1))
        {
            m_MoveCamera.Rotate(0, Input.GetAxis("Mouse X") * m_RotateSpeed, 0, Space.World);
        }

        if (Input.GetMouseButton(2))
        {
            float speed = m_MoveSpeed - (m_ZoomTarget.z - m_MaxZoom) * 0.022f;//0.022 = 200/9000
            Vector3 pos = new Vector3(-Input.GetAxis("Mouse X") * speed, 0, -Input.GetAxis("Mouse Y") * speed);
            Move(pos);
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            float direction = Input.GetAxis("Mouse ScrollWheel");
            Zoom(direction, m_ZoomSpeed, true);
        }
    }

    void Move(Vector3 newPos)
    {
        m_MoveTarget += m_MoveCamera.rotation * newPos;
        m_MoveCamera.position = m_MoveTarget;
    }

    void Zoom(float direction, float zSpeed, bool startScreen)
    {
        float zoom = m_ZoomTarget.z;
        zoom += Mathf.Sign(direction) * zSpeed;
        zoom = Mathf.Clamp(zoom, m_MinZoom, m_MaxZoom);
        zoom = Mathf.Round(zoom);
        if (zoom != m_ZoomTarget.z)
        {
            m_ZoomTarget.z = zoom;
        }
    }

    void UpdateCamera()
    {
        m_MoveCamera.position = Vector3.SmoothDamp(m_MoveCamera.position, m_MoveTarget, ref m_Velocity, m_SmoothTime);
        m_ZoomCamera.localPosition = Vector3.SmoothDamp(m_ZoomCamera.localPosition, m_ZoomTarget, ref m_ZoomVelocity, m_SmoothTime);
    }
}
