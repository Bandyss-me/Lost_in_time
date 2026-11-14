using System;
using UnityEngine;

public class portal_script : MonoBehaviour
{
    public Transform otherPortal;

    private Vector3 pos;

    private void OnTriggerEnter(Collider col)
    {
        if (!(col.gameObject.GetComponent<player_movement>() || col.gameObject.GetComponent<player_movement_offline>()))
        {
            return;
        }
        pos = col.transform.position-transform.position;
        CharacterController cc = col.gameObject.GetComponent<CharacterController>();
        cc.enabled = false;
        col.transform.position = otherPortal.position + pos;
        cc.enabled = true;
    }
}
