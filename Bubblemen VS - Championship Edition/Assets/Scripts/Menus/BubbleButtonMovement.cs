using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleButtonMovement : MonoBehaviour 
{
    public float floatHeight = 2.0f;
    public float offset = 100;
    private float defaultY, angle = 0;

	// Use this for initialization
	void Start () 
    {
        defaultY = transform.position.y;
	}
	
	// Update is called once per frame
	void Update () 
    {
        Vector3 buttonPos = transform.position;

        // Make bubble buttons float up and down
        angle += offset * Time.deltaTime;
        if (angle > 360)
            angle -= 360;
        transform.position = new Vector3(buttonPos.x, defaultY + floatHeight * Mathf.Sin(angle * Mathf.PI/180), buttonPos.z);
    } 
}
