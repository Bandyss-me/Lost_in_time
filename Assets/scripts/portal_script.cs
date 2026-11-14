using System;
using UnityEngine;

public class portal_script : MonoBehaviour
{
    public Transform otherPortal;

    private Vector3 pos;
    private Quaternion rot;

    private void OnTriggerEnter(Collider col)
    {
        if (!(col.gameObject.GetComponent<player_movement>() || col.gameObject.GetComponent<player_movement_offline>()))
        {
            return;
        }
        Quaternion delta = otherPortal.rotation * Quaternion.Inverse(transform.rotation);
        pos = col.transform.position-transform.position;
        rot = Quaternion.Inverse(transform.rotation) * col.transform.rotation;
        CharacterController cc = col.gameObject.GetComponent<CharacterController>();
        cc.enabled = false;
        col.transform.position = otherPortal.position + delta*pos;
        col.transform.rotation = otherPortal.rotation * rot;
        cc.enabled = true;
    }
}
