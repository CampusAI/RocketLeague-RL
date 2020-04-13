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
    public class CarRLAgent_1v1 : Agent
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
        private GoalCheck_1v1 goalCheck;

        // private AgentAgentHelper AgentHelper = new AgentAgentHelper();

        public override void Initialize()
        {
            Time.timeScale = 1;
            // Debug.Log(Thread.CurrentThread.ManagedThreadId);
            //Random.seed = Thread.CurrentThread.ManagedThreadId;
            team = this.gameObject.tag;
            initial_position = this.transform.position;
            initial_rotation = this.transform.rotation;
            parent = this.transform.parent;
            ball = parent.Find("Soccer Ball").gameObject;
            m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
            car_controller = GetComponent<CarController>();
            self_rBody = GetComponent<Rigidbody>();
            ball_rBody = ball.GetComponent<Rigidbody>();
            goalCheck = ball.GetComponent<GoalCheck_1v1>();
            GameObject car_sphere = this.transform.Find("Sphere").gameObject;
            if (team == "Blue")
            {
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
            float noise = AgentHelper.NextGaussian(0, 5);
            // Debug.Log(noise);
            new_pos.x += noise;
            noise = AgentHelper.NextGaussian(0, 3);
            new_pos.z += noise;
            this.transform.position = new_pos; 
            this.self_rBody.velocity = Vector3.zero;

            // Initial rotation woth noise
            this.transform.rotation = initial_rotation;
            var euler = this.transform.eulerAngles;
            euler.y += Random.Range(-20, 20);
            this.transform.eulerAngles = euler;
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

            Vector3 enemy_rel_pos =
                transform.InverseTransformDirection(enemies[0].transform.position - transform.position);
            sensor.AddObservation(enemy_rel_pos.x / 200f);
            sensor.AddObservation(enemy_rel_pos.z / 200f);

            Vector3 enemy_rel_vel =
                transform.InverseTransformDirection(enemies[0].GetComponent<Rigidbody>().velocity);
            sensor.AddObservation(enemy_rel_vel.x / 50f);
            sensor.AddObservation(enemy_rel_vel.z / 50f);

            if (this.team == "Blue") {
                sensor.AddObservation(goalCheck.blue_score);
                sensor.AddObservation(goalCheck.red_score);
            } else {
                sensor.AddObservation(goalCheck.red_score);
                sensor.AddObservation(goalCheck.blue_score);
            }
        }

        public void goal(string scoring_team) {
            if (scoring_team == team) {
                AddReward(0.5f);
            } else {
                AddReward(-0.5f);
            }
        }

        public override void OnActionReceived(float[] vectorAction)
        {
            // Reward forward speed
            // float forward_speed = transform.InverseTransformDirection(self_rBody.velocity).z;
            // if (forward_speed > 0.5) {
            //     Debug.Log("forward");
            //     AddReward(0.1f / maxStep);
            // }
            //AddReward(-5f / maxStep);
            //if (vectorAction[1] > 0) {
            //    AddReward(1.0f / maxStep);
            //}
            car_controller.Move(vectorAction[0], vectorAction[1], vectorAction[1], 0.0f);
            //draw_rew_dir(7);
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
            //Debug.DrawLine(transform.position, ball.transform.position, Color.black);
            //Debug.DrawLine(transform.position, own_goal.transform.position, Color.green);
            //Debug.DrawLine(transform.position, other_goal.transform.position, Color.yellow);
            // Debug.DrawLine(transform.position, enemies[0].transform.position, Color.magenta);
            // car_controller.Move(0f, 1f, 1f, 0f);
        }

        public string GetTeam() {return team;}

    }
}