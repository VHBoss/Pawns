using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    [SerializeField] Renderer[] Connectors;

    private bool[] m_Activated = new bool[4];
    private Material m_DefaultMaterial;
    private Dictionary<LineRenderer, int> m_ConnectedLines = new Dictionary<LineRenderer, int>();
    private bool m_IsSwapped;
    private bool m_IsDestroyed;

    void Start()
    {
        m_DefaultMaterial = Connectors[0].sharedMaterial;
        ConnectorController.OnLineRemoved += OnLineRemoved;
    }

    void OnDestroy()
    {
        if (!m_IsDestroyed)
        {
            m_IsDestroyed = true;
            ConnectorController.OnLineRemoved -= OnLineRemoved;
        }
    }

    public bool Activate(int index, LineRenderer line)
    {
        if (m_Activated[index] == true)
        {
            return false;
        }
        else
        {
            m_Activated[index] = true;           
            return true;
        }
    }

    public void AddLine(int index, LineRenderer line)
    {
        m_ConnectedLines.Add(line, index);
    }

    public void ShowAwailableConnectors()
    {
        for (int i = 0; i < 4; i++)
        {
            if (!m_Activated[i] || !MainController.ConnectorController.OnlyOneConnection) 
            {
                Connectors[i].material = MainController.Settings.ActiveConnectorMaterial;
            }
        }
    }

    public void Reset()
    {
        for (int i = 0; i < 4; i++)
        {
            Connectors[i].material = m_DefaultMaterial;
        }

        m_IsSwapped = false;
    }

    public void UpdateLines()
    {
        if (!m_IsSwapped)
        {
            SwapEndpoints();
        }

        foreach (var item in m_ConnectedLines)
        {
            LineRenderer line = item.Key;
            int connectorIndex = item.Value;
            line.SetPosition(0, Connectors[connectorIndex].transform.position);
        }
    }

    void SwapEndpoints()
    {
        foreach (var item in m_ConnectedLines)
        {
            LineRenderer line = item.Key;
            int connectorIndex = item.Value;
            Vector3 pos0 = line.GetPosition(0);

            if (pos0 != Connectors[connectorIndex].transform.position)
            {
                line.SetPosition(0, Connectors[connectorIndex].transform.position);
                line.SetPosition(1, pos0);
            }
        }

        m_IsSwapped = true;
    }

    public void Destroy()
    {
        OnDestroy();

        gameObject.SetActive(false);//or Destroy()
        MainController.DeletePawn(this);

        foreach (var item in m_ConnectedLines)
        {
            ConnectorController.OnLineRemoved.Invoke(item.Key);
        }

        foreach (var item in m_ConnectedLines)
        {
            Destroy(item.Key.gameObject);
        }

        m_ConnectedLines.Clear();
    }

    void OnLineRemoved(LineRenderer renderer)
    {
        if (m_ConnectedLines.ContainsKey(renderer))
        {
            m_ConnectedLines.Remove(renderer);
        }
    }
}
