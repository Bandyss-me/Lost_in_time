using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelSavingLoadingEditor : MonoBehaviour
{
    [SerializeField] private GameObject m_levelEditorObj;
    private LevelEditor m_levelEditor;
    
    void Start()
    {
        m_levelEditor = m_levelEditorObj.GetComponent<LevelEditor>();
    }

    public void loadFrom(string fileName)
    {
        string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, "custom_levels", fileName + ".json"));
        Level level = JsonUtility.FromJson<Level>(json);
        
        foreach (Level.Object obj in level.objects)
        {
            LevelEditor.LevelEditorObject levelObj = new LevelEditor.LevelEditorObject();

            levelObj.objectID = obj.ID;

            levelObj.gameObject = Instantiate(m_levelEditor.getPrefabs()[obj.ID]);

            Func<Level.NewVec3, Vector3> toVec3 = vec3 => new Vector3(vec3.x, vec3.y, vec3.z);
            Func<Level.NewQuaternion, Quaternion> toQuat = vec3 => new Quaternion(vec3.w, vec3.x, vec3.y, vec3.z);
            
            levelObj.gameObject.transform.position = toVec3(obj.position);
            levelObj.gameObject.transform.localScale = toVec3(obj.scale);
            levelObj.gameObject.transform.rotation = toQuat(obj.rotation);
            
            m_levelEditor.addToObj(levelObj);
        }
    }
    
    public void saveTo(string fileName)
    {
        Level level = new Level();

        level.objects = new List<Level.Object>();

        foreach (LevelEditor.LevelEditorObject obj in m_levelEditor.getObjects())
        {
            if (!obj.gameObject)
                continue;
            
            Level.Object levelObj = new Level.Object();

            levelObj.ID = obj.objectID;

            Func<Vector3, Level.NewVec3> toNewVec3 = vec3 => new Level.NewVec3(vec3.x, vec3.y, vec3.z); 
            Func<Quaternion, Level.NewQuaternion> toNewQuat = quat => new Level.NewQuaternion(quat.w, quat.x, quat.y, quat.z); 
            
            levelObj.position = toNewVec3(obj.gameObject.transform.position);
            levelObj.scale = toNewVec3(obj.gameObject.transform.localScale);
            levelObj.rotation = toNewQuat(obj.gameObject.transform.rotation);
            
            level.objects.Add(levelObj);
        }

        string json = JsonUtility.ToJson(level, true);
        
        string dir = Path.Combine(Application.persistentDataPath, "custom_levels");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, fileName + ".json"), json);
    }
}
