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
public class FollowerRL : Agent
{
    private string team;
    private GameObject own_goal, other_goal, ball;
    private Transform parent;
    private List<GameObject> friends, enemies;

    public Vector3 objective_point;
    
    private Vector3 initial_position, field_center;
    private Quaternion initial_rotation;

    private UnityStandardAssets.Vehicles.Car.CarController car_controller;
    private BehaviorParameters m_BehaviorParameters;
    private Rigidbody self_rBody, ball_rBody;
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
        total_steps = maxStep / 5;//this.gameObject.GetComponent<DecisionRequester>().DecisionPeriod;
        cv_manager = new CurriculumManager();
        team = this.gameObject.tag;
        initial_position = this.transform.position;
        initial_rotation = this.transform.rotation;
        parent = this.transform.parent;
        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        car_controller = GetComponent<UnityStandardAssets.Vehicles.Car.CarController>();
        self_rBody = GetComponent<Rigidbody>();
        // goalCheck = ball.GetComponent<GoalCheck_1v1>();
        field_center = this.transform.parent.Find("ball_spawn_point").position;
        GameObject car_sphere = this.transform.Find("Sphere").gameObject;
        if (team == "Blue")
            car_sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
        else if (team == "Red")
            car_sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
    }

    public override void OnEpisodeBegin()
    {
        // Debug.Log("Episode begin!");
        cv_manager.LogReward(episode_reward);
        episode_reward = 0;
        StepCount = 0;

        var pars = cv_manager.GetParams();

        // Get variables from curricula
        float car_pos_offset = pars["car_pos_offset"];
        float ball_pos_offset = pars["ball_pos_offset"];
        float car_rot_offset = pars["car_rot_offset"];
        float car_vel_offset = pars["car_vel_offset"];
        float total_random = pars["total_random"];

        // Sample new objective
        float x_objective = field_center.x + 30f + Random.Range(-ball_pos_offset, ball_pos_offset);
        float z_objective = field_center.z + Random.Range(-ball_pos_offset, ball_pos_offset);
        if (total_random == 1)
        {
            x_objective = field_center.x + Random.Range(-80, 80);
            z_objective = field_center.z + Random.Range(-40, 40);
        }
        objective_point.x = x_objective;
        objective_point.z = z_objective;

        // Position
        float x_pos = field_center.x - 30f + Random.Range(-car_pos_offset, car_pos_offset);
        float z_pos = field_center.z + Random.Range(-car_pos_offset, car_pos_offset);
        if (total_random == 1)
        {
            x_pos = field_center.x + Random.Range(-80, 80);
            z_pos = field_center.z + Random.Range(-40, 40);

        }
        Vector3 new_pos = new Vector3(x_pos, initial_position.y, z_pos);
        this.transform.position = new_pos;

        // Velocity
        this.self_rBody.velocity = new Vector3(AgentHelper.NextGaussian(0, car_vel_offset), 0, AgentHelper.NextGaussian(0, car_vel_offset));

        // Initial rotation with noise
        this.transform.rotation = initial_rotation;
        var euler = this.transform.eulerAngles;
        euler.y += Random.Range(-car_rot_offset, car_rot_offset);
        this.transform.eulerAngles = euler;
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
    public string GetTeam() { return team; }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 objective_relative_pos =
            transform.InverseTransformDirection(objective_point - transform.position);
        sensor.AddObservation(objective_relative_pos.x / 200.0f);
        sensor.AddObservation(objective_relative_pos.z / 200.0f);

        Vector3 velocity_relative = transform.InverseTransformDirection(self_rBody.velocity);
        sensor.AddObservation(velocity_relative.x / 200.0f);  // Drift speed
        sensor.AddObservation(velocity_relative.z / 200.0f);

        // Raycasts
        Vector3 ray_pos = transform.position;
        ray_pos.y += 10.0f; // So that it ignores cars and ball
        RaycastHit front_hit, back_hit;
        float lidar_range = 200.0f;
        // Front ray
        Vector3 front_ray_dir = transform.TransformDirection(new Vector3(0.0f, 0.0f, 1.0f));
        Ray front_ray = new Ray(ray_pos, front_ray_dir);
        Physics.Raycast(front_ray, out front_hit);
        float front_distance = (Mathf.Clamp(front_hit.distance - 2f, 0, lidar_range)) / lidar_range;
        // Debug.Log("front_distance: " + front_distance.ToString());
        sensor.AddObservation(front_distance);
        // Back ray
        Vector3 back_ray_dir = transform.TransformDirection(new Vector3(0.0f, 0.0f, -1.0f));
        Ray back_ray = new Ray(ray_pos, back_ray_dir);
        Physics.Raycast(back_ray, out back_hit);
        float back_distance = (Mathf.Clamp(back_hit.distance - 2f, 0, lidar_range)) / lidar_range;
        // Debug.Log("back_distance: " + back_distance.ToString());
        sensor.AddObservation(back_distance);
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
        // if (((this.transform.position - ball.transform.position).magnitude < 2) &&
        //     (team == "Blue" && this.transform.position.x < ball.transform.position.x) ||
        //     (team == "Red" && this.transform.position.x > ball.transform.position.x))
        // {
        //     // Debug.Log("close");
        //     add_reward(0.05f / total_steps);
        // }
        // else
        // {
        //     // Debug.Log("far");
        //     add_reward(-0.05f / total_steps);
        // }

        car_controller.Move(vectorAction[0], vectorAction[1], vectorAction[1], 0.0f);
        //draw_rew_dir(7);
    }

    void Update()
    {
        StepCount += 1;
        add_reward(-1f / maxStep);
        Debug.DrawLine(this.transform.position, objective_point, Color.cyan, 0.1f);
        if ((this.transform.position - objective_point).magnitude < 6) {
            add_reward(1.0f);
            EndEpisode();
        }
        if (StepCount > maxStep - 100) {
            EndEpisode();
        }
    }



    // REWARDS ###################################################### 
    private void OnCollisionStay(Collision collision)
    {
        float reward = -0.5f / total_steps;
        if (collision.gameObject.tag != "Ball")
        {
            add_reward(reward);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        float reward = -0.5f / total_steps;
        if (collision.gameObject.tag != "Ball")
        {
            add_reward(reward);
        }
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