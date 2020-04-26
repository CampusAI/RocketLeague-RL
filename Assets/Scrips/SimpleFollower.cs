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
public class SimpleFollower : MonoBehaviour
{

    private string team;
    private Transform parent;
    private List<GameObject> friends, enemies;

    public Vector3 objective_point;

    private Vector3 initial_position, field_center;
    private Quaternion initial_rotation;

    private UnityStandardAssets.Vehicles.Car.CarController car_controller;
    private BehaviorParameters m_BehaviorParameters;
    private Rigidbody self_rBody, ball_rBody;

    // private AgentAgentHelper AgentHelper = new AgentAgentHelper();
    // private float total_steps;

    // [HideInInspector]
    // public FloatPropertiesChannel m_FloatProperties;
    CurriculumManager cv_manager;

    private void Start()
    {
        // total_steps = maxStep / 5;//this.gameObject.GetComponent<DecisionRequester>().DecisionPeriod;
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

    public void Reset(Dictionary<string, float> pars)
    {
        Debug.Log("Reseting follower!");

        // Get variables from curricula
        float car_pos_offset = pars["car_pos_offset"];
        float ball_pos_offset = pars["ball_pos_offset"];
        float car_rot_offset = pars["car_rot_offset"];
        float car_vel_offset = pars["car_vel_offset"];
        float total_random = pars["total_random"];

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

    public string GetTeam() { return team; }


    void Update()
    {
        Debug.DrawLine(this.transform.position, objective_point, Color.cyan, 0.1f);

        Vector3 current_direction = transform.forward.normalized; //vector facing forward from the car
        Vector3 direction = (objective_point - transform.position).normalized;
        float direction_angle = Vector3.Angle(current_direction, direction) * Mathf.Sign(-current_direction.x * direction.z + current_direction.z * direction.x);
        float direction_of_acceleration = Mathf.Clamp(Vector3.Dot(current_direction, direction), -1, 1);
        float acceleration = Mathf.Sign(direction_of_acceleration); //1 if we go forward, -1 if we wanna reverse
        float steering = Mathf.Clamp(direction_angle, -1f, 1f) * Mathf.Sign(direction_of_acceleration);
        car_controller.Move(steering, acceleration, acceleration, 0);
    }


}
// }