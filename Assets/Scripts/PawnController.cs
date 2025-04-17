using Unity.VisualScripting;
using UnityEngine;

enum PawnState
{
    Inside,
    Outside
}

public class PawnController : MonoBehaviour
{
    [SerializeField] Material m_DefaultMaterial;

    private Plane m_Plane;
    private Camera m_MainCamera;
    private Vector3 m_StartOffset;
    private PawnState m_State = PawnState.Inside;
    private Pawn m_PickedPawn = null;

    void Start()
    {
        m_MainCamera = Camera.main;
        m_Plane = new Plane(Vector3.up, Vector3.zero);
        MainController.AddController(this);
    }

    public void CustomUpdate()
    {
        if (MainController.State == GameState.CreateConnector) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Collider go = hit.collider;

                if (go.CompareTag("Player"))
                {
                    m_PickedPawn = go.GetComponentInParent<Pawn>();
                    m_StartOffset = m_PickedPawn.transform.position - GetCursorPosition();
                }
                else if (go.CompareTag("Respawn"))
                {
                    MainController.ConnectorController.StartDraw(go);
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (m_PickedPawn != null)
            {
                m_PickedPawn.Reset();

                if (m_State == PawnState.Outside)
                {
                    m_PickedPawn.Destroy();
                }

                m_PickedPawn = null;
            }
        }

        if (m_PickedPawn != null)
        {
            DragPawn();
        }
    }

    Vector3 GetCursorPosition()
    {
        Ray ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);

        if (m_Plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return Vector3.zero;
    }

    void DragPawn()
    {
        Vector3 pos = GetCursorPosition() + m_StartOffset;
        m_PickedPawn.transform.position = pos;

        float size = MainController.BoardSize * 0.5f;
        bool outside = pos.x > size || pos.x < -size || pos.z > size || pos.z < -size;

        if (outside && m_State == PawnState.Inside)
        {
            m_State = PawnState.Outside;
            ChangeColor(MainController.Settings.DeleteMaterial);
        }
        else if (!outside && m_State == PawnState.Outside)
        {
            m_State = PawnState.Inside;
            ChangeColor(m_DefaultMaterial);
        }

        m_PickedPawn.UpdateLines();
    }

    void ChangeColor(Material mat)
    {
        Renderer[] renderers = m_PickedPawn.transform.GetComponentsInChildren<Renderer>();
        foreach (var item in renderers)
        {
            item.material = mat;
        }
    }
}
