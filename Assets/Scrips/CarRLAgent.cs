using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using MLAgents.Policies;

namespace UnityStandardAssets.Vehicles.Car
{
    public class CarRLAgent : Agent
    {
        private string team;
        private GameObject own_goal;
        private GameObject other_goal;
        private GameObject ball;
        private List<GameObject> friends;
        private List<GameObject> enemies;

        private CarController car_controller;
        private BehaviorParameters m_BehaviorParameters;

        void Start()
        {
            team = this.gameObject.tag;
            m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
            car_controller = GetComponent<CarController>();
            ball = GameObject.Find("Soccer Ball");
            GameObject car_sphere = this.transform.Find("Sphere").gameObject;
            if (team == "Blue")
            {
                m_BehaviorParameters.TeamId = 0;  // Set team id
                car_sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
                own_goal = GameObject.Find("Blue_goal");
                other_goal = GameObject.Find("Red_goal");
                friends = new List<GameObject>();
                foreach (GameObject go in GameObject.FindGameObjectsWithTag(team))
                {
                    if (go.Equals(this.gameObject)) // I do this to exclude itself (not sure if its a good idea yet)
                        continue;
                    friends.Add(go);
                }
                enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Red"));
            }
            else if (team == "Red")
            {
                m_BehaviorParameters.TeamId = 1;  // Set team id
                car_sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                own_goal = GameObject.Find("Red_goal");
                other_goal = GameObject.Find("Blue_goal");
                friends = new List<GameObject>();
                foreach (GameObject go in GameObject.FindGameObjectsWithTag(team))
                {
                    if (go.Equals(this.gameObject)) // I do this to exclude itself (not sure if its a good idea yet)
                        continue;
                    friends.Add(go);
                }
                enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Blue"));
            }
        }

        public override void OnEpisodeBegin()
        {
            Debug.Log("starting");
        }

        public void SetResetParameters()
        {
            Debug.Log("reset");
        }

        public override void AgentReset()
        {
            Debug.Log("agent reset");

        }

        // public override void CollectObservations() {
        //     Debug.Log("observing");
        // }


        public override void OnActionReceived(float[] vectorAction)
        {
            AddReward(-1f / 3000f);  // Existential penalty
            car_controller.Move(vectorAction[0], vectorAction[1], vectorAction[1], 0.0f);
        }

        public override float[] Heuristic()
        {
            var action = new float[2];
            float steer = Input.GetAxis("Horizontal");
            float acc = Input.GetAxis("Vertical");
            action[1] = acc;
            action[0] = steer;
            Debug.Log(acc);
            return action;
        }

        private void FixedUpdate()
        {
            Debug.DrawLine(transform.position, ball.transform.position, Color.black);
            Debug.DrawLine(transform.position, own_goal.transform.position, Color.green);
            Debug.DrawLine(transform.position, other_goal.transform.position, Color.yellow);
            // Debug.DrawLine(transform.position, enemies[0].transform.position, Color.magenta);
            // car_controller.Move(0f, 1f, 1f, 0f);
        }
    }
}