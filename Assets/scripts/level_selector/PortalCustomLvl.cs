using UnityEngine;

public class PortalCustomLvl : MonoBehaviour
{
    public GameObject player;
    public GameObject spawn;

    void OnTriggerEnter(Collider other)
    {
        if (spawn == null || other.gameObject != player) 
            return;

        spawn.GetComponent<SpawnPoint>().Teleport(player);
    }
}