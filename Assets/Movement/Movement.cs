using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Movement : MonoBehaviour
{
    public CharacterController controller;
    float horizontalMove = 0f;

    public float runSpeed = 40f;
    bool jump = false;
    bool isGrounded = true;
    public AudioSource m_MyAudioSource;

    [SerializeField] Animator m_Animator;

    float m_PreviousHorizontalMove = 0f;
    bool m_WasGrounded = false;

    private void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jump = true;
            m_MyAudioSource.Play();

            m_Animator.SetTrigger("JumpTrigger");
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Entered");
        if (collision.gameObject.CompareTag("Cubes"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        Debug.Log("Exited");
        if (collision.gameObject.CompareTag("Cubes"))
        {
            isGrounded = false;
        }
    }

    void FixedUpdate()
    {
        bool isMoving = horizontalMove != 0f;

        if (m_PreviousHorizontalMove == 0f && horizontalMove != 0f)
        {
            // started moving
            m_Animator.SetBool("IsRunning", true);
        }
        else if (m_PreviousHorizontalMove != 0f && horizontalMove == 0f)
        {
            // stopped moving
            m_Animator.SetBool("IsRunning", false);
        }

        if (!isGrounded)
        {
            // falling
            m_Animator.SetBool("IsFalling", true);
        }
        else
        {
            // not Falling
            m_Animator.SetBool("IsFalling", false);
        }

        controller.Move(horizontalMove * Time.fixedDeltaTime, jump);
        jump = false;

        m_WasGrounded = isGrounded;
        m_PreviousHorizontalMove = horizontalMove;
    }
}
