using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using MLAgents.Policies;


namespace UnityStandardAssets.Vehicles.Car
{

    public class GoalCheck : MonoBehaviour
    {
        public GameObject ball_spawn_point;
        public int blue_score = 0;
        public int red_score = 0;

        private List<GameObject> players;
        private float game_length = 100;
        private float start_time;

        // Start is called before the first frame update
        void Start()
        {
            transform.position = ball_spawn_point.transform.position;
            players = new List<GameObject>();
            players.AddRange(GameObject.FindGameObjectsWithTag("Blue"));
            players.AddRange(GameObject.FindGameObjectsWithTag("Red"));
            start_time = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time - start_time > game_length) {
                reset();
                foreach (GameObject player in players)
                    player.gameObject.GetComponent<CarRLAgent>().AgentReset();
                start_time = Time.time;
            }
        }

        public void reset() {
            transform.position = ball_spawn_point.transform.position;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        private void OnCollisionEnter(Collision collision)
        {
            //Debug.Log(collision.gameObject.name);
            if (collision.gameObject.name == "Blue_goal")
            {
                Debug.Log("Red scored!");
                red_score = red_score + 1;
                foreach (GameObject player in players)
                    player.gameObject.GetComponent<CarRLAgent>().goal("Red");
                reset();
            }
            if (collision.gameObject.name == "Red_goal")
            {
                Debug.Log("Blue scored!");
                blue_score = blue_score + 1;
                foreach (GameObject player in players)
                    player.gameObject.GetComponent<CarRLAgent>().goal("Blue");
                reset();
            }
        }
    }
}