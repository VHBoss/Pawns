using CrazyPawn;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Idle,
    DragPawn,
    CreateConnector
}

public class MainController : MonoBehaviour
{
    private static MainController s_Instance;

    public static CrazyPawnSettings Settings => s_Instance.m_Settings;

    public static ConnectorController ConnectorController { get; private set; }
    public static PawnController PawnController { get; private set; }

    public static GameState State;
    public static float BoardSize;

    [SerializeField] CrazyPawnSettings m_Settings;
    [Space]
    [SerializeField] float m_BoardCellSize = 1.5f;
    [SerializeField] Pawn m_PawnPrefab;

    private static List<Pawn> m_Pawns = new List<Pawn>();

    void Awake()
    {
        s_Instance = this;
    }

    void Start()
    {
        CreateBoard();
        CreatePawns();
    }

    void Update()
    {
        //!Order is Important!
        PawnController.CustomUpdate();
        ConnectorController.CustomUpdate();
    }

    void CreateBoard()
    {
        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = "Board";

        board.GetComponent<Collider>().enabled = false;

        float sizeY = 1f;
        BoardSize = m_Settings.CheckerboardSize * m_BoardCellSize;
        board.transform.localScale = new Vector3(BoardSize, sizeY, BoardSize);
        board.transform.position = new Vector3(0, -sizeY*0.5f, 0);

        Shader shader = Shader.Find("BoardShaders/Checker");
        Material mat = new Material(shader);
        board.GetComponent<Renderer>().material = mat;

        mat.SetFloat("_CellSize", m_BoardCellSize);
        mat.SetColor("_ColorA", m_Settings.BlackCellColor);
        mat.SetColor("_ColorB", m_Settings.WhiteCellColor);
    }

    void CreatePawns()
    {
        float radius = m_Settings.InitialZoneRadius;

        for (int i = 0; i < m_Settings.InitialPawnCount; i++)
        {
            Vector2 randPoint = Random.insideUnitCircle * radius;
            Vector3 pos = new Vector3(randPoint.x, 0, randPoint.y);
            Pawn pawn = Instantiate(m_PawnPrefab, pos, Quaternion.identity);
            pawn.name = "Pawn_" + i;
            m_Pawns.Add(pawn);
        }
    }

    internal static void AddController(MonoBehaviour controller)
    {
        switch (controller)
        {
            case ConnectorController cc: ConnectorController = cc; break;
            case PawnController cc: PawnController = cc; break;
        }
    }

    internal static void HighlightAvailableConnectors(Pawn selectedPawn)
    {
        foreach (var pawn in m_Pawns)
        {
            if (pawn == selectedPawn) continue;

            pawn.ShowAwailableConnectors();
        }
    }

    internal static void ResetConnectors()
    {
        foreach (var pawn in m_Pawns)
        {
            pawn.Reset();
        }
    }

    internal static void DeletePawn(Pawn selectedPawn)
    {
        m_Pawns.Remove(selectedPawn);
    }
}
