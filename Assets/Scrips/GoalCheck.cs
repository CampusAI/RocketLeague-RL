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
        Vector3 blue_goal_pos, red_goal_pos;

        // Start is called before the first frame update
        void Start()
        {
            transform.position = ball_spawn_point.transform.position;
            players = new List<GameObject>();
            players.AddRange(AgentHelper.FindGameObjectInChildWithTag(transform.parent, "Blue"));
            players.AddRange(AgentHelper.FindGameObjectInChildWithTag(transform.parent, "Red"));
            // blue_goal_pos = transform.parent.Find("Blue_goal").gameObject.transform.position;
            // red_goal_pos = transform.parent.Find("Red_goal").gameObject.transform.position;
        }

        public void ResetGame()
        {
            ResetBall();
            blue_score = 0;
            red_score = 0;
        }

        public void ResetBall()
        {
            transform.position = ball_spawn_point.transform.position;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
        private void OnCollisionEnter(Collision collision)
        {
            //Debug.Log(collision.gameObject.name);
            if (collision.gameObject.name == "Blue_goal")
            {
                red_score = red_score + 1;
                foreach (GameObject player in players)
                    player.gameObject.GetComponent<CarRLAgent>().goal("Red");
                ResetBall();
            }
            if (collision.gameObject.name == "Red_goal")
            {
                blue_score = blue_score + 1;
                foreach (GameObject player in players)
                    player.gameObject.GetComponent<CarRLAgent>().goal("Blue");
                ResetBall();
            }
            // If collided with a car
            if (collision.gameObject.tag == "Blue" || collision.gameObject.tag == "Red")
            {
                foreach (GameObject player in players)
                    player.gameObject.GetComponent<CarRLAgent>().TouchedBall(collision.gameObject.tag);
            }
        }

        public void FixedUpdate()
        {
            CheckWin();
            // RewardField();
            RewardBallVelocity();
        }

        public void RewardBallVelocity()
        {
            float epsilon = 4.0f;
            float ball_towards_red = 0.0f;
            float x_vel = this.gameObject.GetComponent<Rigidbody>().velocity.x;
            if (x_vel > epsilon)
                ball_towards_red = 1;
            else if (x_vel < -epsilon)
                ball_towards_red = -1;

            float reward = 0.2f / players[0].GetComponent<CarRLAgent>().maxStep;
            if (ball_towards_red != 0.0f)
                foreach (GameObject player in players)
                {
                    CarRLAgent script = player.GetComponent<CarRLAgent>();
                    if (script.GetTeam() == "Blue")
                    {
                        script.add_reward(ball_towards_red * reward);
                    }
                    else if (script.GetTeam() == "Red")
                    {
                        script.add_reward(-ball_towards_red * reward);
                    }
                    else
                    {
                        throw new System.Exception("UNKNOWN AGENT TAG");
                    }
                }

        }

        public void CheckWin()
        {
            int step = players[0].GetComponent<CarRLAgent>().StepCount;
            int max_steps = players[0].GetComponent<CarRLAgent>().maxStep;
            if (step >= max_steps - 10)
            { // To be sure episode terminates here
                if (blue_score == red_score)
                {
                    GiveFinalRewardsAndEnd(0f, 0f);
                }
                else if (blue_score > red_score)
                {
                    GiveFinalRewardsAndEnd(1f, -1f);
                }
                else
                {
                    GiveFinalRewardsAndEnd(-1f, 1f);
                }
            }
        }

        void GiveFinalRewardsAndEnd(float blue_reward, float red_reward)
        {
            foreach (GameObject player in players)
            {
                CarRLAgent script = player.GetComponent<CarRLAgent>();

                if (script.GetTeam() == "Blue")
                {
                    script.SetReward(blue_reward);
                    script.EndEpisode();
                }
                else if (script.GetTeam() == "Red")
                {
                    script.SetReward(red_reward);
                    script.EndEpisode();
                }
                else
                {
                    throw new System.Exception("UNKNOWN AGENT TAG");
                }
            }
        }
    }
}