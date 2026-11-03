using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Rigidbody Rigidbody;
    
    public float MoveSpeed = 5f;

    public Transform GroundCheck;
    public LayerMask GroundLayer;
    public float sphereRadius = 0.1f;
    public float jumpForce = 5f;
    public float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    private Vector2 input;

    private void Update()
    {
        jumpBufferCounter -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        Vector3 movement = input.y * transform.forward + input.x * transform.right;

        Rigidbody.linearVelocity = Rigidbody.linearVelocity.y * Vector3.up + MoveSpeed * Time.fixedDeltaTime * movement;

        if(jumpBufferCounter > 0f && IsGrounded())
        {
            Rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpBufferCounter = 0f;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(GroundCheck.position, 0.1f, GroundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (GroundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GroundCheck.position, sphereRadius);
    }
}
