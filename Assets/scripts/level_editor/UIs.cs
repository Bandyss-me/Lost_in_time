using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIs : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private Button rowPrefab;
    public Button saveButton;
    public Button closeButton;
    public TMP_Text textBox;
    public GameObject pannel;
    
    private readonly List<Button> m_rows = new ();

    private void Awake()
    {
        pannel.SetActive(false);
        
        closeButton.onClick.AddListener(() =>
        {
            pannel.SetActive(false);
        });
    }

    public void rebuild(IReadOnlyList<GameObject> objects, Action<uint> onSelect)
    {
        foreach (Button b in m_rows)
            Destroy(b.gameObject);
        m_rows.Clear();

        for (uint i = 0; i < objects.Count; i++)
        {
            uint index = i;
            Button row = Instantiate(rowPrefab, content);
            row.GetComponentInChildren<TMP_Text>().text = $"{objects[(int)i].name}";
            row.onClick.AddListener(() => onSelect(index));
            m_rows.Add(row);
        }
    }

    public void highlight(uint selected)
    {
        for (int i = 0; i < m_rows.Count; i++)
            m_rows[i].image.color = (i == selected) ? Color.yellow : Color.white;
    }
}
