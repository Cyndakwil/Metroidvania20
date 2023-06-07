using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementComponent : MonoBehaviour
{
    public float _moveSpeed;
    public float _jumpHeight;

    public float _maxRigidbodyVelocity;

    private Rigidbody2D _rb;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        float direction = ctx.ReadValue<float>();
        if (direction != 0.0f)
        {
            transform.localScale = new Vector3(direction, 1, 1);

            Vector2 movementInfo = new Vector2(direction * _moveSpeed, 0);

            _rb.AddForce(movementInfo);
        }
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (_rb.velocity.y == 0.0f)
        {
            float active = ctx.ReadValue<float>();

            Vector2 movementInfo = new Vector2(0, active);

            movementInfo.y *= Mathf.Sqrt(_jumpHeight * -2 * (Physics2D.gravity.y * _rb.gravityScale));

            _rb.AddForce(movementInfo, ForceMode2D.Impulse);
        }
    }

    void FixedUpdate()
    {
        _rb.velocity = new Vector2(Mathf.Clamp(_rb.velocity.x, -_maxRigidbodyVelocity, _maxRigidbodyVelocity), _rb.velocity.y);   
    }
}