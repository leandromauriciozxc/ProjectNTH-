using UnityEngine;

public class PlayerTeleporter : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform objectToTeleport;

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    public void TeleportTo(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("PlayerTeleporter: Target is null.");
            return;
        }

        // Disable CharacterController before teleporting
        characterController.enabled = false;

        // Move player
        objectToTeleport.position = target.position;

        // Face the same direction as the target
        objectToTeleport.rotation = target.rotation;

        // Enable CharacterController again
        characterController.enabled = true;
    }
}