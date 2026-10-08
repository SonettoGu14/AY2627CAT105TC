using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariablesDemo : MonoBehaviour
{

    string playerName = "Tom";
    int age = 25;
    float money = 100.5f;
    bool isAlive = true;


    int frameCount = 0;
    float timeCount = 0f;

    // Start is called before the first frame update
    void Start()
    {
        // This is a comment
        // The following three lines will print messages to the console
        //Debug.Log("This is a log message");
        //Debug.LogWarning("This is a warning message");
        //Debug.LogError("This is an error message");
    }

    // Update is called once per frame
    void Update()
    {
        frameCount = frameCount + 1; //This is the total frame we got
        // frameCount ++;

        timeCount = timeCount + Time.deltaTime; //This is the total time we got

        Debug.Log(frameCount / timeCount); //This is the FPS we got
    }
}