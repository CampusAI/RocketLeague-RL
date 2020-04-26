// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Barracuda;
// using MLAgents;
// using MLAgents.Sensors;
using MLAgents.Policies;
// using MLAgents.SideChannels;
// // using System;
// using System.Threading;
using System.Collections;
using UnityEngine;
using MLAgents;
using Barracuda;
using MLAgents.Sensors;
using MLAgents.SideChannels;
using System.Collections.Generic;

// namespace UnityStandardAssets.Vehicles.Car
// {
public class Coach : Agent
{
    private string team;
    private GameObject own_goal, other_goal, ball;
    public GameObject car;
    private Transform parent, field_center;
    private List<GameObject> friends, enemies;

    private BehaviorParameters m_BehaviorParameters;
    private Rigidbody ball_rBody;
    private GoalCheck_1v1 goalCheck;

    // private AgentAgentHelper AgentHelper = new AgentAgentHelper();
    private float total_steps;
    private int StepCount = 0;
    private float episode_reward = 0; // To debug rewards

    // [HideInInspector]
    // public FloatPropertiesChannel m_FloatProperties;
    CurriculumManager cv_manager;

    public override void Initialize()
    {
        Debug.Log(maxStep);
        total_steps = maxStep / 5;//this.gameObject.GetComponent<DecisionRequester>().DecisionPeriod;
        cv_manager = new CurriculumManager();
        team = this.gameObject.tag;
        parent = this.transform.parent;
        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        // goalCheck = ball.GetComponent<GoalCheck_1v1>();
        field_center = this.transform.parent.Find("ball_spawn_point").transform;
        car = this.transform.parent.Find("CarSoccerRL_Blue").gameObject;  // TODO: This depends onm the team
        ball = this.transform.parent.Find("Soccer_Ball").gameObject;

        if (team == "Blue")
        {
            own_goal = parent.Find("Blue_goal").gameObject;
            other_goal = parent.Find("Red_goal").gameObject;
        }
        else if (team == "Red")
        {
            own_goal = parent.Find("Red_goal").gameObject;
            other_goal = parent.Find("Blue_goal").gameObject;
        }

    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode begin!");
        cv_manager.LogReward(episode_reward);
        var pars = cv_manager.GetParams();
        car.GetComponent<SimpleFollower>().Reset(pars);
        ball.GetComponent<GoalCheck_coach>().ResetGame(pars);

        episode_reward = 0;
        StepCount = 0;
    }
    Vector3 objective_point;
    public override float[] Heuristic()
    {
        Plane plane = new Plane(Vector3.up,0);
        float dist;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out dist))
        {
            objective_point = ray.GetPoint(dist);
        }
        float x_val = (objective_point.x - field_center.position.x)/90f;
        float z_val = (objective_point.z - field_center.position.z)/45f;
        return new float[]{x_val, z_val};
    }
    public string GetTeam() { return team; }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Car position
        Vector3 car_pos =
            field_center.InverseTransformDirection(car.transform.position);
        sensor.AddObservation(car_pos.x / 200.0f);
        sensor.AddObservation(car_pos.z / 200.0f);

        // Car velocity
        Vector3 car_velocity =
            field_center.InverseTransformDirection(car.GetComponent<Rigidbody>().velocity);
        sensor.AddObservation(car_velocity.x / 200.0f);
        sensor.AddObservation(car_velocity.z / 200.0f);

        // Car rotation
        var euler = car.transform.eulerAngles;
        sensor.AddObservation(euler.y / 180.0f);

        // Ball position
        Vector3 ball_pos =
            field_center.InverseTransformDirection(ball.transform.position);
        sensor.AddObservation(ball_pos.x / 200.0f);
        sensor.AddObservation(ball_pos.z / 200.0f);

        // Ball velocity
        Vector3 ball_velocity =
            field_center.InverseTransformDirection(ball.GetComponent<Rigidbody>().velocity);
        sensor.AddObservation(ball_velocity.x / 200.0f);
        sensor.AddObservation(ball_velocity.z / 200.0f);

        // Own goal (not sure if needed)
        Vector3 own_goal_pos =
            field_center.InverseTransformDirection(own_goal.transform.position);
        sensor.AddObservation(own_goal_pos.x / 200.0f);
        sensor.AddObservation(own_goal_pos.z / 200.0f);

        // Other goal (not sure if needed)
        Vector3 other_goal_pos =
            field_center.InverseTransformDirection(other_goal.transform.position);
        sensor.AddObservation(other_goal_pos.x / 200.0f);
        sensor.AddObservation(other_goal_pos.z / 200.0f);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        float x_val = Mathf.Clamp(90f*vectorAction[0], -90f, 90f) + field_center.position.x;
        float z_val = Mathf.Clamp(45f*vectorAction[1], -45f, 45f) + field_center.position.z;
        Vector3 objective = new Vector3(x_val, car.transform.position.y, z_val);
        // Debug.DrawLine()
        car.GetComponent<SimpleFollower>().objective_point = objective;
    }

    void Update()
    {
        StepCount += 1;
        add_reward(-1f / maxStep);
        if (StepCount > maxStep - 100) {
            EndEpisode();
        }
    }

    // REWARDS ###################################################### 
    public void touched_ball() {
        add_reward(1f/maxStep);
    }
    
    public void scored_goal() {
        add_reward(1f);
        EndEpisode();
    }

    public void scored_autogoal() {
        add_reward(-1f);
        EndEpisode();
    }


    public void add_reward(float reward)
    { // This is to debug episode reward
        episode_reward += reward;

        // if (this.gameObject.name == "CarSoccerRL_Blue")
        // {
        //     Debug.Log("Adding: " + reward.ToString());
        //     Debug.Log("Summing:" + episode_reward.ToString());
        // }
        AddReward(reward);
    }

}
// }