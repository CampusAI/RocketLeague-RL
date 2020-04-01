using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalCheck : MonoBehaviour
{
    public GameObject ball_spawn_point;
    public int blue_score = 0;
    public int red_score = 0;


    // Start is called before the first frame update
    void Start()
    {
        transform.position = ball_spawn_point.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.gameObject.name);
        if(collision.gameObject.name == "Blue_goal")
        {
            Debug.Log("Red scored!");
            transform.position = ball_spawn_point.transform.position;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            red_score = red_score + 1;

        }
        if (collision.gameObject.name == "Red_goal")
        {
            Debug.Log("Blue scored!");
            transform.position = ball_spawn_point.transform.position;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            blue_score = blue_score + 1;
        }
    }
}
