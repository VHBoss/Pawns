using System;
using UnityEngine;

public enum ControlType
{
    None,
    Click,
    Drag
}

public class ConnectorController : MonoBehaviour
{
    public bool OnlyOneConnection;

    public static Action<LineRenderer> OnLineRemoved;

    private Plane m_Plane;
    private Camera m_MainCamera;
    private LineRenderer m_Renderer;
    private float m_LineWidth = 0.07f;
    private float m_DragDelta = 0.2f;
    private Vector3 m_StartPosition;
    private ControlType m_Type = ControlType.None;
    private Material m_LineMaterial;
    private Pawn m_SelectedPawn;
    private Collider m_Selected;

    void Start()
    {
        m_MainCamera = Camera.main;
        m_Plane = new Plane(Vector3.up, new Vector3(0, 0.684f, 0));

        Shader shader = Shader.Find("Unlit/Color");
        m_LineMaterial = new Material(shader);

        MainController.AddController(this);
    }

    public void CustomUpdate()
    {
        if(m_Renderer != null)
        {
            if (m_Type == ControlType.None)
            {
                float sqDist = Vector3.SqrMagnitude(m_StartPosition - GetCursorPosition());
                if (sqDist > m_DragDelta * m_DragDelta)
                {
                    m_Type = ControlType.Drag;
                }
            }

            DragEndPoint();

            if (m_Type == ControlType.Click && Input.GetMouseButtonDown(0))
            {
                Stop();
            }

            if (Input.GetMouseButtonUp(0))
            {
                if(m_Type == ControlType.None)
                {
                    m_Type = ControlType.Click;
                }
                else if(m_Type == ControlType.Drag)
                {
                    Stop();
                }
            }
        }
    }

    void DragEndPoint()
    {
        Ray ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);

        if (m_Plane.Raycast(ray, out float enter))
        {
            Vector3 cursorPosition = ray.GetPoint(enter);
            cursorPosition.y = m_Renderer.GetPosition(0).y;
            m_Renderer.SetPosition(1, cursorPosition);
        }
    }

    public void StartDraw(Collider collider)
    {
        m_Selected = collider;

        MainController.State = GameState.CreateConnector;

        m_StartPosition = GetCursorPosition();

        GameObject line = new GameObject("Line");
        m_Renderer = line.AddComponent<LineRenderer>();
        m_Renderer.material = m_LineMaterial;
        m_Renderer.startWidth = m_Renderer.endWidth = m_LineWidth;

        Vector3 startPosition = collider.transform.position;
        m_Renderer.SetPosition(0, startPosition);
        m_Renderer.SetPosition(1, startPosition);

        m_Type = ControlType.None;

        m_SelectedPawn = collider.GetComponentInParent<Pawn>();
        MainController.HighlightAvailableConnectors(m_SelectedPawn);
    }

    void Stop()
    {
        Ray ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Collider go = hit.collider;

            if (go != null && go.CompareTag("Respawn"))
            {
                Pawn pawn = go.GetComponentInParent<Pawn>();

                if(pawn == m_SelectedPawn)
                {
                    CancelDraw();
                    return;
                }

                int connectorIndex = go.transform.GetSiblingIndex() - 1; //- CubeBody

                if (!OnlyOneConnection || pawn.Activate(connectorIndex, m_Renderer))
                {
                    int selectedConnectorIndex = m_Selected.transform.GetSiblingIndex() - 1; //- CubeBody
                    m_SelectedPawn.Activate(selectedConnectorIndex, m_Renderer);
                    m_SelectedPawn.AddLine(selectedConnectorIndex, m_Renderer);

                    pawn.AddLine(connectorIndex, m_Renderer);

                    m_Renderer.SetPosition(1, go.transform.position);

                    EndDraw();
                }
                else
                {
                    CancelDraw();
                }
            }
        }
        else
        {
            CancelDraw();
        }
    }

    void CancelDraw()
    {
        Destroy(m_Renderer.gameObject);
        EndDraw();
    }

    void EndDraw()
    {
        MainController.ResetConnectors();
        MainController.State = GameState.Idle;
        m_Type = ControlType.None;
        m_Renderer = null;
        m_Selected = null;
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
}
