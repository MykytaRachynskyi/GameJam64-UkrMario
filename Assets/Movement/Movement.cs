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

    private void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        
        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            jump = true;
            m_MyAudioSource.Play();
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
        controller.Move(horizontalMove * Time.fixedDeltaTime, jump);
        jump = false;
    }
}
