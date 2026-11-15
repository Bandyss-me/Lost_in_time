using System;
using UnityEngine;

public class respawn_script : MonoBehaviour
{
    [SerializeField] private Transform start;

    private void OnTriggerEnter(Collider col)
    {
        CharacterController cc = col.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        col.transform.position = start.position;
        col.transform.rotation = start.rotation;
        if (cc) cc.enabled = true;
    }
}
