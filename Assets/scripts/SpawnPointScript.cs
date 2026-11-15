using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.1f, 0f);
    [SerializeField] private bool matchRotation = false;

    public void Teleport(GameObject player)
    {
        if (player == null)
        {
            return;
        }

        Vector3 targetPos = transform.position + offset;
        Quaternion targetRot = matchRotation
            ? Quaternion.Euler(0f, transform.eulerAngles.y, 0f)
            : player.transform.rotation;

        CharacterController cc = player.GetComponent<CharacterController>();
        bool ccWasEnabled = cc != null && cc.enabled;
        if (ccWasEnabled) cc.enabled = false;

        player.transform.SetPositionAndRotation(targetPos, targetRot);

        if (ccWasEnabled) 
            cc.enabled = true;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.position = targetPos;
            rb.rotation = targetRot;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Physics.SyncTransforms();
    }
}