using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using MLAgents.Policies;


// namespace UnityStandardAssets.Vehicles.Car
// {
public class GoalCheck_coach : MonoBehaviour
{
    public GameObject ball_spawn_point, coach_go;
    public int blue_score = 0;
    public int red_score = 0;

    private Coach coach;
    Vector3 blue_goal_pos, red_goal_pos;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = ball_spawn_point.transform.position;
        blue_goal_pos = transform.parent.Find("Blue_goal").gameObject.transform.position;
        red_goal_pos = transform.parent.Find("Red_goal").gameObject.transform.position;
        // coach_go = transform.parent.Find("Coach_Blue").gameObject;
        coach = transform.parent.Find("Coach_Blue").gameObject.GetComponent<Coach>();
    }

    public void ResetGame(Dictionary<string, float> pars = null)
    {
        ResetBall(pars);
        blue_score = 0;
        red_score = 0;
    }
    public void ResetBall(Dictionary<string, float> pars = null)
    {
        float ball_pos_offset = 0;
        float ball_rot_offset = 0;
        float ball_vel_offset = 0;
        float total_random = 0;
        if (pars != null)
        {
            ball_pos_offset = pars["ball_pos_offset"];
            ball_rot_offset = pars["ball_rot_offset"];
            ball_vel_offset = pars["ball_vel_offset"];
            total_random = pars["total_random"];
        }

        float x_pos = ball_spawn_point.transform.position.x + 15f + Random.Range(-ball_pos_offset, ball_pos_offset);
        float z_pos = ball_spawn_point.transform.position.z + Random.Range(-ball_pos_offset, ball_pos_offset);
        if (total_random == 1)
        {
            x_pos = ball_spawn_point.transform.position.x + Random.Range(-80, 80);
            z_pos = ball_spawn_point.transform.position.z + Random.Range(-40, 40);
        }
        Vector3 new_pos = new Vector3(x_pos, ball_spawn_point.transform.position.y - 2.0f, z_pos);
        this.transform.position = new_pos;
        GetComponent<Rigidbody>().velocity = new Vector3(AgentHelper.NextGaussian(0, ball_vel_offset), 0, AgentHelper.NextGaussian(0, ball_vel_offset));
        GetComponent<Rigidbody>().angularVelocity = new Vector3(AgentHelper.NextGaussian(0, ball_rot_offset), AgentHelper.NextGaussian(0, ball_rot_offset), AgentHelper.NextGaussian(0, 5));
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Blue_goal")
        {
            red_score = red_score + 1;
            coach.scored_autogoal();
        }
        if (collision.gameObject.name == "Red_goal")
        {
            blue_score = blue_score + 1;
            coach.scored_goal();
        }
        // If collided with a car
        if (collision.gameObject.tag == "Blue" || collision.gameObject.tag == "Red")
        {
            coach.touched_ball();
            // foreach (GameObject player in players)
            //     player.gameObject.GetComponent<FollowerRL>().TouchedBall(collision.gameObject.tag);
            // GiveFinalRewardsAndEnd(1f, -1f);
        }
    }
}