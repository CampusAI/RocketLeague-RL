using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using MLAgents.Policies;


namespace UnityStandardAssets.Vehicles.Car
{
    public class GoalCheck_1v1 : MonoBehaviour
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
            blue_goal_pos = transform.parent.Find("Blue_goal").gameObject.transform.position;
            red_goal_pos = transform.parent.Find("Red_goal").gameObject.transform.position;
        }

        public void ResetGame()
        {
            ResetBall();
            blue_score = 0;
            red_score = 0;
        }

        public void ResetBall()
        {
            float x_pos = ball_spawn_point.transform.position.x + Random.Range(-70, 70);
            float z_pos = ball_spawn_point.transform.position.z + Random.Range(-30, 30);
            Vector3 new_pos = new Vector3(x_pos, ball_spawn_point.transform.position.y, z_pos);
            this.transform.position = new_pos;
            GetComponent<Rigidbody>().velocity = new Vector3(AgentHelper.NextGaussian(0, 6), 0, AgentHelper.NextGaussian(0, 6));
            GetComponent<Rigidbody>().angularVelocity = new Vector3(AgentHelper.NextGaussian(0, 5), AgentHelper.NextGaussian(0, 5), AgentHelper.NextGaussian(0, 5));
        }
        private void OnCollisionEnter(Collision collision)
        {
            //Debug.Log(collision.gameObject.name);
            // if (collision.gameObject.name == "Blue_goal")
            // {
            //     red_score = red_score + 1;
            //     foreach (GameObject player in players)
            //         player.gameObject.GetComponent<CarRLAgent_1v1>().goal("Red");
            //     ResetBall();
            // }
            // if (collision.gameObject.name == "Red_goal")
            // {
            //     blue_score = blue_score + 1;
            //     foreach (GameObject player in players)
            //         player.gameObject.GetComponent<CarRLAgent_1v1>().goal("Blue");
            //     ResetBall();
            // }
            // If collided with a car
            if (collision.gameObject.tag == "Blue" || collision.gameObject.tag == "Red")
            {
                // foreach (GameObject player in players)
                //     player.gameObject.GetComponent<CarRLAgent_1v1>().TouchedBall(collision.gameObject.tag);
                GiveFinalRewardsAndEnd(1f, -1f);
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
            Vector3 vel_dir = this.gameObject.GetComponent<Rigidbody>().velocity;
            Vector3 ball_to_blue = blue_goal_pos - this.transform.position;
            Vector3 ball_to_red = red_goal_pos - this.transform.position;
            vel_dir.y = 0;
            ball_to_blue.y = 0;
            ball_to_red.y = 0;

            float factor = 0.5f/(players[0].GetComponent<CarRLAgent_1v1>().maxStep);
            float red_reward = factor*Mathf.Cos(Vector3.Angle(vel_dir, ball_to_blue)*Mathf.PI/180);
            float blue_reward = factor*Mathf.Cos(Vector3.Angle(vel_dir, ball_to_red)*Mathf.PI/180);
            
            if ((Mathf.Abs(red_reward) + Mathf.Abs(blue_reward))/factor < 0.5)
                return;

            foreach (GameObject player in players)
            {
                CarRLAgent_1v1 script = player.GetComponent<CarRLAgent_1v1>();
                if (script.GetTeam() == "Blue")
                {
                    script.add_reward(blue_reward - red_reward);
                }
                else if (script.GetTeam() == "Red")
                {
                    script.add_reward(red_reward - blue_reward);
                }
                else
                {
                    throw new System.Exception("UNKNOWN AGENT TAG");
                }
            }
        }
        public void CheckWin()
        {
            int step = players[0].GetComponent<CarRLAgent_1v1>().StepCount;
            int max_steps = players[0].GetComponent<CarRLAgent_1v1>().maxStep;
            if (step >= max_steps - 10)
            {
                if (blue_score == red_score)
                {
                    GiveFinalRewardsAndEnd(0f, 0f);
                }
                else if (blue_score > red_score)
                {
                    GiveFinalRewardsAndEnd(3f, -3f);
                }
                else
                {
                    GiveFinalRewardsAndEnd(-3f, 3f);
                }
            }
        }

        void GiveFinalRewardsAndEnd(float blue_reward, float red_reward)
        {
            foreach (GameObject player in players)
            {
                CarRLAgent_1v1 script = player.GetComponent<CarRLAgent_1v1>();

                if (script.GetTeam() == "Blue")
                {
                    script.add_reward(blue_reward);
                    script.EndEpisode();
                }
                else if (script.GetTeam() == "Red")
                {
                    script.add_reward(red_reward);
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