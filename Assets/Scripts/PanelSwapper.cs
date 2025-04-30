using System.Collections.Generic;
using UnityEngine;

public class PanelSwapper : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> panelList = new List<GameObject>();

    public static PanelSwapper Instance { get; private set; } = null;

    void Start()
    {
        Instance = this;
    }


    public void SwitchPanel(int i)
    {
        foreach (GameObject panel in panelList)
        {
            panel.SetActive(false);
        }
        panelList[i].SetActive(true);
    }
}
