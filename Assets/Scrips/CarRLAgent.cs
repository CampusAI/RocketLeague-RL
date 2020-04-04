using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using MLAgents.Policies;
// using System;
using System.Threading;
 
namespace UnityStandardAssets.Vehicles.Car
{
    public class CarRLAgent : Agent
    {
        private string team;
        private GameObject own_goal, other_goal, ball;
        private Transform parent;
        private List<GameObject> friends, enemies;
        private Vector3 initial_position;
        private Quaternion initial_rotation;

        private CarController car_controller;
        private BehaviorParameters m_BehaviorParameters;
        private Rigidbody self_rBody, ball_rBody;
        private GoalCheck goalCheck;

        // private AgentAgentHelper AgentHelper = new AgentAgentHelper();

        void Start()
        {
            // Debug.Log(Thread.CurrentThread.ManagedThreadId);
            Random.seed = Thread.CurrentThread.ManagedThreadId;
            team = this.gameObject.tag;
            initial_position = this.transform.position;
            initial_rotation = this.transform.rotation;
            parent = this.transform.parent;
            ball = parent.Find("Soccer Ball").gameObject;
            m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
            car_controller = GetComponent<CarController>();
            self_rBody = GetComponent<Rigidbody>();
            ball_rBody = ball.GetComponent<Rigidbody>();
            goalCheck = ball.GetComponent<GoalCheck>();
            GameObject car_sphere = this.transform.Find("Sphere").gameObject;
            if (team == "Blue")
            {
                m_BehaviorParameters.TeamId = 0;  // Set team id
                car_sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
                own_goal = parent.Find("Blue_goal").gameObject;
                other_goal = parent.Find("Red_goal").gameObject;
                friends = new List<GameObject>();

                foreach (GameObject go in AgentHelper.FindGameObjectInChildWithTag(parent, team))
                {
                    if (go.Equals(this.gameObject)) // I do this to exclude itself (not sure if its a good idea yet)
                        continue;
                    friends.Add(go);
                }
                enemies = new List<GameObject>(AgentHelper.FindGameObjectInChildWithTag(parent, "Red"));
            }
            else if (team == "Red")
            {
                m_BehaviorParameters.TeamId = 1;  // Set team id
                car_sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                own_goal = parent.Find("Red_goal").gameObject;
                other_goal = parent.Find("Blue_goal").gameObject;
                friends = new List<GameObject>();
                foreach (GameObject go in AgentHelper.FindGameObjectInChildWithTag(parent, team))
                {
                    if (go.Equals(this.gameObject)) // I do this to exclude itself (not sure if its a good idea yet)
                        continue;
                    friends.Add(go);
                }
                enemies = new List<GameObject>(AgentHelper.FindGameObjectInChildWithTag(parent, "Blue"));
            }
        }

        public override void OnEpisodeBegin()
        {
            goalCheck.ResetGame();
            Vector3 new_pos = new Vector3(initial_position.x, initial_position.y, initial_position.z);
            float noise = AgentHelper.NextGaussian(0, 7);
            // Debug.Log(noise);
            new_pos.x += noise;
            noise = AgentHelper.NextGaussian(0, 5);
            new_pos.z += noise;
            this.transform.position = new_pos; 

            this.self_rBody.velocity = Vector3.zero;
            this.transform.rotation = initial_rotation;
        }


        public override void CollectObservations(VectorSensor sensor) {
            Vector3 ball_relative_pos =
                transform.InverseTransformDirection(ball.transform.position - transform.position);
            sensor.AddObservation(ball_relative_pos.x / 200.0f);
            sensor.AddObservation(ball_relative_pos.y / 200.0f);
            sensor.AddObservation(ball_relative_pos.z / 200.0f);

            Vector3 ball_relative_vel =
                transform.InverseTransformDirection(ball_rBody.velocity);
            sensor.AddObservation(ball_relative_vel.x / 50.0f);
            sensor.AddObservation(ball_relative_vel.y / 50.0f);
            sensor.AddObservation(ball_relative_vel.z / 50.0f);
            
            Vector3 velocity_relative = transform.InverseTransformDirection(self_rBody.velocity);
            sensor.AddObservation(velocity_relative.x / 50f);  // Drift speed
            sensor.AddObservation(velocity_relative.z / 50f);

            Vector3 other_goal_relative_pos =
                transform.InverseTransformDirection(other_goal.transform.position - transform.position);
            sensor.AddObservation(other_goal_relative_pos.x / 200.0f);
            sensor.AddObservation(other_goal_relative_pos.z / 200.0f);
        }

        float goal_reward = 0;
        public void goal(string scoring_team) {
            if (scoring_team == team) {
                //AddReward(1.0f);
                goal_reward += 1.0f;
            }
            else {
                //AddReward(-1.0f);
                goal_reward -= 1.0f;
            }
        }

        public void TouchedBall() {
            // Debug.Log("Touched");
            //AddReward(0.01f);
        }

        public override void OnActionReceived(float[] vectorAction)
        {
            if (goal_reward > 0) {
                AddReward(goal_reward);
                EndEpisode();
            }
            goal_reward = 0f;
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