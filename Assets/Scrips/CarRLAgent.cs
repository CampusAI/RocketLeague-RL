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
        private GameObject own_goal, other_goal, ball;
        private List<GameObject> friends, enemies;
        private Vector3 initial_position;
        private Quaternion initial_rotation;

        private CarController car_controller;
        private BehaviorParameters m_BehaviorParameters;
        private Rigidbody rBody;


        void Start()
        {
            team = this.gameObject.tag;
            initial_position = this.transform.position;
            initial_rotation = this.transform.rotation;
            m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
            car_controller = GetComponent<CarController>();
            rBody = GetComponent<Rigidbody>();
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
            // Debug.Log("Episode starting");
            this.transform.position = initial_position;
            this.rBody.velocity = Vector3.zero;
            this.transform.rotation = initial_rotation;
        }


        public override void CollectObservations(VectorSensor sensor) {
            Vector3 ball_relative_pos =
                transform.InverseTransformDirection(ball.transform.position - transform.position);
            sensor.AddObservation(ball_relative_pos.x / 190.0f);
            sensor.AddObservation(ball_relative_pos.y / 190.0f);
            sensor.AddObservation(ball_relative_pos.z / 190.0f);
            
            Vector3 velocity_relative = transform.InverseTransformDirection(rBody.velocity);
            sensor.AddObservation(velocity_relative.x / 100f);  // Drift speed
            sensor.AddObservation(velocity_relative.z / 100f);

            Vector3 other_goal_relative_pos =
                transform.InverseTransformDirection(other_goal.transform.position - transform.position);
            sensor.AddObservation(other_goal_relative_pos.x / 190.0f);
            sensor.AddObservation(other_goal_relative_pos.z / 190.0f);
        }

        public void goal(string scoring_team) {
            if (scoring_team == team) {
                AddReward(1.0f);
            }
            else {
                AddReward(-1.0f);
            }
        }

        public void TouchedBall() {
            AddReward(0.001f);
        }

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