using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LevelEditor : MonoBehaviour
{
    private UIs m_UIs;
    public GameObject m_uiGO;
    private GameObject[] m_objectPrefabs;
    [SerializeField] private Material m_cloakMaterial;
    [SerializeField] private GameObject m_objPrefabSelector;

    [SerializeField] private GameObject m_levelSavingObj;
    private LevelSavingLoadingEditor m_LevelSavingLoadingEditor;
    
    public float sensitivity = .1f;
    public enum KeyType
    {
        PLACE_OBJ,
        TOGGLE_MOUSE_FOLLOWING,
        CAM_FORWARD,
        CAM_BACKWARDS,
        CAM_LEFT,
        CAM_RIGHT,
        CUBE_FORWARD,
        CUBE_BACKWARDS,
        CUBE_LEFT,
        CUBE_RIGHT,
        CUBE_UP,
        CUBE_DOWN,
        TOGGLE_ROTATE_MODE,
        SCALE_TM_X,
        SCALE_TM_Y,
        SCALE_TM_Z,
        ROT_Z_UP,
        ROT_Z_DOWN,
        DELETE_OBJ,
        TOGGLE_OBJ_SELECTION,
        SAVE_LEVEL,
        SLOW_DOWN_TRANSFORM
    };
    
    public float maxDist = 30f;
    public float cloakSize = .05f;
    
    public Dictionary<KeyType, KeyCode> keyBindings = new()
    {
        { KeyType.PLACE_OBJ, KeyCode.P },
        { KeyType.TOGGLE_MOUSE_FOLLOWING, KeyCode.M },
        
        { KeyType.CAM_FORWARD, KeyCode.W },
        { KeyType.CAM_BACKWARDS, KeyCode.S },
        { KeyType.CAM_LEFT, KeyCode.A },
        { KeyType.CAM_RIGHT, KeyCode.D },
        
        { KeyType.CUBE_FORWARD, KeyCode.UpArrow },
        { KeyType.CUBE_BACKWARDS, KeyCode.DownArrow },
        { KeyType.CUBE_LEFT, KeyCode.LeftArrow },
        { KeyType.CUBE_RIGHT, KeyCode.RightArrow },
        { KeyType.CUBE_UP, KeyCode.Space },
        { KeyType.CUBE_DOWN, KeyCode.LeftControl },
        
        { KeyType.TOGGLE_ROTATE_MODE, KeyCode.R },
        { KeyType.SCALE_TM_X, KeyCode.X },
        { KeyType.SCALE_TM_Y, KeyCode.Y },
        { KeyType.SCALE_TM_Z, KeyCode.Z },
        { KeyType.ROT_Z_UP, KeyCode.Space},
        { KeyType.ROT_Z_DOWN, KeyCode.LeftControl },
        { KeyType.DELETE_OBJ, KeyCode.Delete },
        { KeyType.TOGGLE_OBJ_SELECTION, KeyCode.O},
        { KeyType.SAVE_LEVEL, KeyCode.U},
        { KeyType.SLOW_DOWN_TRANSFORM, KeyCode.LeftShift },
    };

    public struct LevelEditorObject
    {
        public GameObject gameObject;
        public uint objectID;
    };

    public float slowMult = 2;
    public float moveSpeed = .5f;
    public float objMoveSpeed = .2f;
    public float objScaleSpeed = .2f;
    public float objRotateSpeed = .2f;
    
    private uint m_selectedPrefabID = 0;
    private List<LevelEditorObject> m_objects = new ();
    private bool m_followMouse = true;
    private float m_yaw;
    private float m_pitch;
    private int m_selectedEditorObjectIndex = -1;
    private GameObject m_cloak;
    private Camera m_cam;
    private bool m_rotateMode = false;
    
    void Start()
    {
        m_UIs = m_uiGO.GetComponent<UIs>();
        
        m_UIs.saveButton.onClick.AddListener(() =>
        {
            m_LevelSavingLoadingEditor.saveTo(m_UIs.textBox.text);
        });
            
        m_LevelSavingLoadingEditor = m_levelSavingObj.GetComponent<LevelSavingLoadingEditor>();
        m_objPrefabSelector.SetActive(false);
        m_selectedPrefabID = 0;
        
        m_objectPrefabs = Resources.LoadAll<GameObject>("level_objects_prefabs");
     
        m_UIs.rebuild(m_objectPrefabs, (uint id) =>
        {
            m_selectedPrefabID = id;
            m_UIs.highlight(m_selectedPrefabID);
        });
        
        m_UIs.highlight(0);
        
        Vector3 eulerAngles = Camera.main.transform.eulerAngles;
        m_yaw = eulerAngles.y;
        m_pitch = eulerAngles.x > 180f ? eulerAngles.x - 360f : eulerAngles.x;
        Cursor.lockState = CursorLockMode.Locked;
            
    }
    
    private void placeObject(Vector3 position, uint objID)
    {
        LevelEditorObject obj = new()
        {
            objectID = objID,
            gameObject = Instantiate(m_objectPrefabs[objID], position, Quaternion.identity)
        };

        m_objects.Add(obj);
    }

    bool isPressed(KeyType type, bool held = false)
    {
        if (held == false)
            return Input.GetKeyDown(keyBindings[type]);
        return Input.GetKey(keyBindings[type]);
    }

    private void updateCloak()
    {
        m_cloak.transform.position = m_objects[m_selectedEditorObjectIndex].gameObject.transform.position;
        m_cloak.transform.localScale = m_objects[m_selectedEditorObjectIndex].gameObject.transform.localScale + new Vector3(2 * cloakSize, 2 * cloakSize, 2 * cloakSize);
        m_cloak.transform.rotation = m_objects[m_selectedEditorObjectIndex].gameObject.transform.rotation;
    }
    
    private void buildCloak()
    {
        if (m_cloak)
            Destroy(m_cloak);
        m_cloak = Instantiate(m_objects[m_selectedEditorObjectIndex].gameObject);
        m_cloak.GetComponent<Renderer>().material = m_cloakMaterial; 

        updateCloak();
    }

    public List<LevelEditorObject> getObjects()
    {
        return m_objects;
    }
    void Update()
    {
        if (isPressed(KeyType.TOGGLE_OBJ_SELECTION))
            m_objPrefabSelector.SetActive(!m_objPrefabSelector.activeSelf);
        
        if (isPressed(KeyType.TOGGLE_ROTATE_MODE))
            m_rotateMode = !m_rotateMode;
        
        if (m_selectedEditorObjectIndex == -1 && m_cloak)
            Destroy(m_cloak);

        if (m_selectedEditorObjectIndex >= 0 && !m_cloak)
            buildCloak();

        if (isPressed(KeyType.CAM_FORWARD, true))
        {
            var cam = Camera.main; 
            cam.transform.position += cam.transform.forward * moveSpeed * Time.deltaTime;
        }
        
        if (isPressed(KeyType.CAM_BACKWARDS, true))
        {
            var cam = Camera.main; 
            cam.transform.position -= cam.transform.forward * moveSpeed * Time.deltaTime;
        }
        
        if (isPressed(KeyType.CAM_LEFT, true))
        {
            var cam = Camera.main; 
            cam.transform.position -= Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up) * moveSpeed * Time.deltaTime;
        }
        
        if (isPressed(KeyType.CAM_RIGHT, true))
        {
            var cam = Camera.main; 
            cam.transform.position += Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up) * moveSpeed * Time.deltaTime;
        }
        
        if (isPressed(KeyType.TOGGLE_MOUSE_FOLLOWING))
        {
            m_followMouse = !m_followMouse;
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        }

        if (m_followMouse && !m_rotateMode)
        {
            m_yaw += Input.GetAxis("Mouse X") * sensitivity;
            m_pitch -= Input.GetAxis("Mouse Y") * sensitivity;
            m_pitch = Mathf.Clamp(m_pitch, -89f, 89f);

            Camera.main.transform.rotation = Quaternion.Euler(m_pitch, m_yaw, 0f);
        }

        if (m_rotateMode)
        {
            Transform target = m_objects[m_selectedEditorObjectIndex].gameObject.transform;
            Transform cam = Camera.main.transform;

            float dx = Input.GetAxis("Mouse X") * sensitivity;
            float dy = Input.GetAxis("Mouse Y") * sensitivity;

            float dz = 0f;
            if (isPressed(KeyType.ROT_Z_UP)) 
                dz += objRotateSpeed * Time.deltaTime;
            if (isPressed(KeyType.ROT_Z_DOWN)) 
                dz -= objRotateSpeed * Time.deltaTime;

            target.Rotate(cam.up, -dx, Space.World);
            target.Rotate(cam.right, dy, Space.World);
            target.Rotate(cam.forward, dz, Space.World);

            updateCloak();
        }
        
        if (isPressed(KeyType.PLACE_OBJ))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, maxDist))
                placeObject(hit.point, m_selectedPrefabID);
            else
                placeObject(ray.GetPoint(maxDist), m_selectedPrefabID);
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, maxDist))
            {
                int index = ((Func<int>)(() =>
                {
                    for (int i = 0; i < m_objects.Count; i++)
                    {
                        if (m_objects[i].gameObject == raycastHit.transform.gameObject)
                            return i;
                    }

                    return -1;
                }))();

                m_selectedEditorObjectIndex = index;
                if (index != -1)
                    buildCloak();
            }
        }

        if (m_rotateMode == false && (isPressed(KeyType.CUBE_FORWARD, true) || isPressed(KeyType.CUBE_BACKWARDS, true) || isPressed(KeyType.CUBE_LEFT, true) ||
                                                   isPressed(KeyType.CUBE_RIGHT, true) || isPressed(KeyType.CUBE_UP, true) || isPressed(KeyType.CUBE_DOWN, true)) 
            && m_selectedEditorObjectIndex != -1) 
        {
            Transform cam = Camera.main.transform;
            Vector3 forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            Vector3 up = Vector3.up;

            Transform target = m_objects[m_selectedEditorObjectIndex].gameObject.transform;
            Vector3 dir = Vector3.zero;

            if (isPressed(KeyType.CUBE_FORWARD, true)) 
                dir += forward;
            if (isPressed(KeyType.CUBE_BACKWARDS, true)) 
                dir -= forward;
            if (isPressed(KeyType.CUBE_RIGHT, true)) 
                dir += right;
            if (isPressed(KeyType.CUBE_LEFT, true)) 
                dir -= right;
            if (isPressed(KeyType.CUBE_UP, true)) 
                dir += up;
            if (isPressed(KeyType.CUBE_DOWN, true)) 
                dir -= up;

            target.position += dir.normalized * objMoveSpeed * Time.deltaTime;
            
            updateCloak();
        }

        if (isPressed(KeyType.DELETE_OBJ) && m_selectedEditorObjectIndex != -1)
        {
            Destroy(m_objects[m_selectedEditorObjectIndex].gameObject);
            Destroy(m_cloak);
            m_selectedEditorObjectIndex = -1;
            
        }
        
        if (m_rotateMode == false &&  (isPressed(KeyType.SCALE_TM_X, true) || isPressed(KeyType.SCALE_TM_Y, true) ||
             isPressed(KeyType.SCALE_TM_Z, true)) && m_selectedEditorObjectIndex != -1)
        {
            Vector3 scaleAdd = Vector3.zero;
            
            if (isPressed(KeyType.SCALE_TM_X, true))
                scaleAdd.x += Input.GetAxis("Mouse ScrollWheel") * objScaleSpeed;
            if (isPressed(KeyType.SCALE_TM_Y, true))
                scaleAdd.y += Input.GetAxis("Mouse ScrollWheel") * objScaleSpeed;
            if (isPressed(KeyType.SCALE_TM_Z, true))
                scaleAdd.z += Input.GetAxis("Mouse ScrollWheel") * objScaleSpeed;

            m_objects[m_selectedEditorObjectIndex].gameObject.transform.localScale += scaleAdd;
            
            updateCloak();
        }

        if (isPressed(KeyType.SAVE_LEVEL))
        {
            m_UIs.pannel.SetActive(true);
        }

        if (isPressed(KeyType.SLOW_DOWN_TRANSFORM))
        {
            objMoveSpeed /= slowMult;
            objScaleSpeed /= slowMult;
            objRotateSpeed /= slowMult;
        }

        if (Input.GetKeyUp(keyBindings[KeyType.SLOW_DOWN_TRANSFORM]))
        {
            objMoveSpeed *= slowMult;
            objScaleSpeed *= slowMult;
            objRotateSpeed *= slowMult;
        }
    }
}
