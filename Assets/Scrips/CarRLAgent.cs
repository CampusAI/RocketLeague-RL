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
        // private DecisionRequester m_DecisionRequester;
        private Rigidbody self_rBody, ball_rBody;
        private GoalCheck goalCheck;

        // private float episode_reward = 0; // To debug rewards

        public override void Initialize()
        {
            // Time.timeScale = 5;
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
            goalCheck = ball.GetComponent<GoalCheck>();
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

        public string GetTeam() { return team; }

        public override void OnEpisodeBegin()
        {
            goalCheck.ResetGame();
            Vector3 new_pos = new Vector3(initial_position.x, initial_position.y, initial_position.z);
            float noise = AgentHelper.NextGaussian(0, 15);
            // Debug.Log(noise);
            new_pos.x += noise;
            noise = AgentHelper.NextGaussian(0, 7);
            new_pos.z += noise;
            this.transform.position = new_pos;
            this.self_rBody.velocity = Vector3.zero;

            // Initial rotation woth noise
            this.transform.rotation = initial_rotation;
            var euler = this.transform.eulerAngles;
            euler.y += Random.Range(-30, 30);
            this.transform.eulerAngles = euler;
        }


        // CONTROL ######################################################
        public override float[] Heuristic()
        {
            var action = new float[2];
            float steer = Input.GetAxis("Horizontal");
            float acc = Input.GetAxis("Vertical");
            action[1] = acc;
            action[0] = steer;
            return action;
        }

        public override void OnActionReceived(float[] vectorAction)
        {
            if (ball.transform.position.y < 0f)
            {
                EndEpisode();
            }
            car_controller.Move(vectorAction[0], vectorAction[1], vectorAction[1], 0.0f);
        }


        // OBESRVATIONS #################################################
        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 ball_relative_pos =
                transform.InverseTransformDirection(ball.transform.position - transform.position);
            sensor.AddObservation(ball_relative_pos.x / 200.0f);
            sensor.AddObservation((ball_relative_pos.y-2.1f) / 200.0f); // Make it zero if at same height
            sensor.AddObservation(ball_relative_pos.z / 200.0f);

            Vector3 ball_relative_vel =
                transform.InverseTransformDirection(ball_rBody.velocity);
            sensor.AddObservation(ball_relative_vel.x / 50.0f);
            sensor.AddObservation(ball_relative_vel.y / 50.0f);
            sensor.AddObservation(ball_relative_vel.z / 50.0f);

            Vector3 velocity_relative = transform.InverseTransformDirection(self_rBody.velocity);
            sensor.AddObservation(velocity_relative.x / 50f);  // Drift speed
            sensor.AddObservation(velocity_relative.z / 50f);

            Vector3 own_goal_relative_pos =
                transform.InverseTransformDirection(own_goal.transform.position - transform.position);
            sensor.AddObservation(own_goal_relative_pos.x / 200.0f);
            sensor.AddObservation(own_goal_relative_pos.z / 200.0f);

            Vector3 other_goal_relative_pos =
                transform.InverseTransformDirection(other_goal.transform.position - transform.position);
            sensor.AddObservation(other_goal_relative_pos.x / 200.0f);
            sensor.AddObservation(other_goal_relative_pos.z / 200.0f);

            foreach (GameObject enemy in enemies)
            {
                Vector3 enemy_rel_pos =
                    transform.InverseTransformDirection(enemy.transform.position - transform.position);
                sensor.AddObservation(enemy_rel_pos.x / 200f);
                sensor.AddObservation(enemy_rel_pos.z / 200f);

                Vector3 enemy_rel_vel =
                    transform.InverseTransformDirection(enemy.GetComponent<Rigidbody>().velocity);
                sensor.AddObservation(enemy_rel_vel.x / 50f);
                sensor.AddObservation(enemy_rel_vel.z / 50f);
            }

            foreach (GameObject friend in friends)
            {
                Vector3 friend_rel_pos =
                    transform.InverseTransformDirection(friend.transform.position - transform.position);
                sensor.AddObservation(friend_rel_pos.x / 200f);
                sensor.AddObservation(friend_rel_pos.z / 200f);

                Vector3 friend_rel_vel =
                    transform.InverseTransformDirection(friend.GetComponent<Rigidbody>().velocity);
                sensor.AddObservation(friend_rel_vel.x / 50f);
                sensor.AddObservation(friend_rel_vel.z / 50f);
            }

            // Raycasts
            Vector3 ray_pos = transform.position;
            ray_pos.y += 10.0f; // So that it ignores cars and ball
            RaycastHit front_hit, back_hit;
            float lidar_range = 100.0f;
            // Front ray
            Vector3 front_ray_dir = transform.TransformDirection(new Vector3(0.0f, 0.0f, 1.0f));
            Ray front_ray = new Ray(ray_pos, front_ray_dir);
            Physics.Raycast(front_ray, out front_hit);
            float front_distance = (Mathf.Clamp(front_hit.distance - 2f, 0, lidar_range)) / lidar_range;
            sensor.AddObservation(front_distance);
            // Back ray
            Vector3 back_ray_dir = transform.TransformDirection(new Vector3(0.0f, 0.0f, -1.0f));
            Ray back_ray = new Ray(ray_pos, back_ray_dir);
            Physics.Raycast(back_ray, out back_hit);
            float back_distance = (Mathf.Clamp(back_hit.distance - 2f, 0, lidar_range)) / lidar_range;
            sensor.AddObservation(back_distance);

            // Debug raycasts
            // Debug.DrawLine(ray_pos, ray_pos + (Mathf.Clamp(front_hit.distance - 2f, 0, lidar_range))*front_ray_dir.normalized, Color.red, 0.1f);
            // Debug.DrawLine(ray_pos, ray_pos + (Mathf.Clamp(back_hit.distance - 2f, 0, lidar_range))*back_ray_dir.normalized, Color.red, 0.1f);
            // if (this.gameObject.name == "CarSoccerRL_Blue")
            // {
                // Debug.Log("Ball: ");
                // Debug.Log(ball_relative_pos);
                // Debug.Log("Front: " + front_distance);
                // Debug.Log("Back: " + back_distance);
            // }

            // In case we wanna add something but not retrain whole model
            sensor.AddObservation(0.0f);
            sensor.AddObservation(0.0f);
            sensor.AddObservation(0.0f);
            sensor.AddObservation(0.0f);
        }


        // REWARDS ###################################################### 
        public void goal(string scoring_team)
        {
            if (scoring_team == team)
            {
                add_reward(1.0f);
            }
            else
            {
                add_reward(-1.0f);
            }
        }

        public void TouchedBall(string touching_team)
        {
            float reward = 0.05f / maxStep;
            if (touching_team == team)
            {
                add_reward(reward);
            }
            else
            {
                add_reward(-reward);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            float reward = - 0.05f / maxStep;
            if (collision.gameObject.tag != "Ball")
            {
                add_reward(reward);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            float reward = - 0.05f / maxStep;
            if (collision.gameObject.tag != "Ball")
            {
                add_reward(reward);
            }
        }

        public void add_reward(float reward)
        { // This is to debug episode reward
            // if (this.gameObject.name == "CarSoccerRL_Blue")
            // {
            //     episode_reward += reward;
            //     Debug.Log("Adding: " + reward.ToString());
            //     Debug.Log("Summing:" + episode_reward.ToString());
            // }
            AddReward(reward);
        }
    }
}