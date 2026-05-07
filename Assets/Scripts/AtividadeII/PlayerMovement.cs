using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Rotação")]
    [SerializeField] private bool rotateToMoveDirection = true;
    [SerializeField] private float rotationSpeed = 10f;

    private Vector2 moveInput;

    void Update()
    {
        LerInput();
        MoverPlayer();
        RotacionarPlayer();
    }

    private void LerInput()
    {
        moveInput = Vector2.zero;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            moveInput.y += 1f;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            moveInput.y -= 1f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            moveInput.x -= 1f;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            moveInput.x += 1f;

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
    }

    private void MoverPlayer()
    {
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void RotacionarPlayer()
    {
        if (!rotateToMoveDirection)
            return;

        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}