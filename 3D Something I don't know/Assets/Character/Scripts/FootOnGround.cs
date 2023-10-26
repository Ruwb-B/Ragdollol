using UnityEngine;

public class FootOnGround : MonoBehaviour
{
    public PlayerController playerController;

    private void OnCollisionEnter(Collision collision)
    {
        if (playerController.jumpDelay == false)
        {
            this.playerController.isGrounded = true;
        }
    }
}
