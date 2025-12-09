using UnityEngine;
using UnityEngine.Serialization;

public class Menu_Platform : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [FormerlySerializedAs("Jump_Force")] public float jumpForce;

    void OnCollisionEnter2D(Collision2D Other)
    {
        // Add force when player fall from top
        if (Other.relativeVelocity.y <= 0f)
        {
            Rigidbody2D rigid = Other.collider.GetComponent<Rigidbody2D>();

            if (rigid != null)
            {
                Vector2 force = rigid.linearVelocity;
                force.y = jumpForce;
                rigid.linearVelocity = force;

                // Play jump sound
                GetComponent<AudioSource>().Play();
            }
        }
    }
}
