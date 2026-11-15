using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelBuilding : MonoBehaviour
{
    public GameObject player;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene("LevelSelectorPage");
        }
    }

    void Start()
    {
        
        string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, "custom_levels", StaticLevelTransfer.levelName + ".json"));
        Level level = JsonUtility.FromJson<Level>(json);
        
        var prefabs = Resources.LoadAll<GameObject>("level_objects_prefabs");

        GameObject spawn = new GameObject();
        List<GameObject> portals = new();
        bool pS = false;
        
        foreach (Level.Object obj in level.objects)
        {
            LevelEditor.LevelEditorObject levelObj = new LevelEditor.LevelEditorObject();

            levelObj.objectID = obj.ID;

            levelObj.gameObject = Instantiate(prefabs[obj.ID]);
            
            Func<Level.NewVec3, Vector3> toVec3 = vec3 => new Vector3(vec3.x, vec3.y, vec3.z);
            Func<Level.NewQuaternion, Quaternion> toQuat = vec3 => new Quaternion( vec3.x, vec3.y, vec3.z, vec3.w);
            
            levelObj.gameObject.transform.position = toVec3(obj.position);
            levelObj.gameObject.transform.localScale = toVec3(obj.scale);
            levelObj.gameObject.transform.rotation = toQuat(obj.rotation);

            if (obj.ID == 2)
            {
                levelObj.gameObject.GetComponent<SpawnPoint>().Teleport(player);
                spawn = levelObj.gameObject;
            }

            if (obj.ID == 3)
            {
                pS = true;
                portals.Add(levelObj.gameObject);
            }

            if (obj.ID == 4)
            {
                levelObj.gameObject.GetComponent<Renderer>().enabled = false;
            }
        }
        
        if (pS)
        {
            foreach (var portal in portals)
            {
                portal.GetComponent<PortalCustomLvl>().player = player;
                portal.GetComponent<PortalCustomLvl>().spawn = spawn;

                portal.GetComponent<Renderer>().enabled = false;
            }
        }
    }
    
    
}
