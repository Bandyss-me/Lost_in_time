using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectorUI : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private Button rowPrefab;
    [SerializeField] private Button openLevelEditor;
    
    private readonly List<Button> m_rows = new ();

    private void Awake()
    {
        openLevelEditor.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(2);
        });
        
        string dir = Path.Combine(Application.persistentDataPath, "custom_levels");
        List<string> names = new();

        foreach (var x in Directory.GetFiles(dir, "*.json"))
            names.Add(Path.GetFileNameWithoutExtension(x));   
        
        rebuild(names, indx =>
        {
            Debug.Log(indx);
        });
    }

    public void rebuild(IReadOnlyList<string> objects, Action<uint> onSelect)
    {
        foreach (Button b in m_rows)
            Destroy(b.gameObject);
        m_rows.Clear();

        for (uint i = 0; i < objects.Count; i++)
        {
            uint index = i;
            Button row = Instantiate(rowPrefab, content);
            row.GetComponentInChildren<TMP_Text>().text = $"{objects[(int)i]}";
            row.onClick.AddListener(() => onSelect(index));
            m_rows.Add(row);
        }
    }
}