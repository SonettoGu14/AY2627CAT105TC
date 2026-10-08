using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysicsController : MonoBehaviour
{

    public Rigidbody rb;
    public float moveForce = 5f;

    public float speed = 5f;

    bool canJump = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if(Input.GetKey(KeyCode.W))
        // {
        //     // Apply a force to the rigidbody of the player
        //     // Change the player's rigidbody's velocity to move the player forward
        //     rb.AddForce(Vector3.forward * moveForce);
            
        // }

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        rb.velocity = new Vector3(horizontalInput * speed, rb.velocity.y, verticalInput * speed);


        if(Input.GetKeyDown(KeyCode.Space) && canJump == true)
        {
            canJump = false;
            rb.AddForce(Vector3.up * moveForce);
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
            Debug.Log(1);
        }
    }


}
