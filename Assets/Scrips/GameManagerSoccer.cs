using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerSoccer : MonoBehaviour
{


    public GameObject terrain_manager_game_object;
    TerrainManager terrain_manager;

    public GameObject race_car;

    public GameObject blue_goal;
    public GameObject red_goal;

    public int blue_score;
    public int red_score;

    public GameObject ball;
    //public GameObject spawn_point_ball;

    float start_time;
    public float match_time;
    public float match_length = 100f;
    public float goal_tolerance = 10.0f;
    public bool finished = false;
    public int no_of_cars = 10;

    // public List<GameObject> my_cars;

    // Use this for initialization
    void Awake()
    {
        // terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
        // start_time = Time.time;
    }

    void Start()
    {
        // start_time = Time.time;
    //     my_cars = new List<GameObject>();

    //     blue_goal.transform.position = terrain_manager.myInfo.start_pos;
    //     red_goal.transform.position = terrain_manager.myInfo.goal_pos;

    //     for (int i = 0; i < no_of_cars; i++)
    //     {
    //         string team_tag;
    //         Color team_color;
    //         if (i < no_of_cars / 2f)
    //         {
    //             team_tag = "Blue";
    //             team_color = Color.blue;
    //         }
    //         else
    //         {
    //             team_tag = "Red";
    //             team_color = Color.red;
    //         }

    //         GameObject new_car;
    //         UnityStandardAssets.Vehicles.Car.CarAISoccer_gr1 new_AI_gr1;
    //         UnityStandardAssets.Vehicles.Car.CarAISoccer_gr2 new_AI_gr2;

    //         new_car = Instantiate(race_car, new Vector3(20.0f + i * 8.0f, 10.0f, 20f), Quaternion.identity);
    //         //new_car = Instantiate(race_car, Vector3.zero, Quaternion.identity);
    //         new_car.tag = team_tag;

    //         Vector3 nominal_pos = CircularConfiguration(i + (int)Mathf.Floor(no_of_cars / 2), no_of_cars, 0.2f);
    //         new_car.transform.position = GetCollisionFreePosNear(nominal_pos, 10f);
    //         my_cars.Add(new_car);

    //         GameObject car_sphere = new_car.transform.Find("Sphere").gameObject;
    //         //Color my_color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    //         car_sphere.GetComponent<Renderer>().material.SetColor("_Color", team_color);

    //         //GameObject goal_sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

    //         //nominal_pos = CircularConfiguration(i, no_of_cars, 0.8f);
    //         //goal_sphere.transform.position = GetCollisionFreePosNear(nominal_pos, 50f);

    //         //goal_sphere.transform.localScale = Vector3.one * 3f;
    //         //goal_sphere.GetComponent<Renderer>().material.SetColor("_Color", team_color);

    //         // Assign goal point
    //         new_AI_gr1 = new_car.GetComponent<UnityStandardAssets.Vehicles.Car.CarAISoccer_gr1>();
    //         new_AI_gr2 = new_car.GetComponent<UnityStandardAssets.Vehicles.Car.CarAISoccer_gr2>();

    //         if (team_tag == "Blue")
    //         {
    //             //new_AI.my_goal_object = red_goal;
    //             new_AI_gr1.own_goal = blue_goal;
    //             new_AI_gr1.other_goal = red_goal;
    //             new_AI_gr2.own_goal = blue_goal;
    //             new_AI_gr2.other_goal = red_goal;
    //             new_AI_gr1.enabled = true;
    //             new_AI_gr2.enabled = false;
    //         }
    //         else
    //         {
    //             //new_AI.my_goal_object = blue_goal;
    //             new_AI_gr1.own_goal = red_goal;
    //             new_AI_gr1.other_goal = blue_goal;
    //             new_AI_gr2.own_goal = red_goal;
    //             new_AI_gr2.other_goal = blue_goal;
    //             new_AI_gr1.enabled = false;
    //             new_AI_gr2.enabled = true;
    //         }

    //         new_AI_gr1.terrain_manager_game_object = terrain_manager_game_object;
    //         new_AI_gr2.terrain_manager_game_object = terrain_manager_game_object;

    //         //new_AI.my_goal = goal_sphere.transform.position;
    //         //var cubeRenderer = new_sphere.GetComponent<Renderer>();
    //         //var cubeRenderer = new_car.GetComponent<Sphere>();


    //         //Call SetColor using the shader property name "_Color" and setting the color to red
    //         //cubeRenderer.material.SetColor("_Color", Color.cyan);
    //     }
    }

    // Update is called once per frame
    void Update()
    {
        // if (!finished)
        // {
        //     match_time = Time.time - start_time;
        //     //Debug.Log(ball.GetComponent<GoalCheck>().blue_score);
        //     blue_score = ball.GetComponent<GoalCheck>().blue_score;
        //     red_score = ball.GetComponent<GoalCheck>().red_score;
        //     if(match_time > match_length)
        //     {
        //         finished = true;
        //     }
            
        // }
        

    }

    // Vector3 CircularConfiguration(int i, int max_i, float factor_k)
    // {
    //     float center_x = (terrain_manager.myInfo.x_high + terrain_manager.myInfo.x_low) / 2.0f;
    //     float center_z = (terrain_manager.myInfo.z_high + terrain_manager.myInfo.z_low) / 2.0f;
    //     Vector3 center = new Vector3(center_x, 1f, center_z);

    //     float alpha = i * 2.0f * Mathf.PI / max_i;
    //     float r = (terrain_manager.myInfo.x_high - center_x) * factor_k;
    //     return center + new Vector3(Mathf.Sin(alpha), 0f, Mathf.Cos(alpha)) * r;
    // }

    // Vector3 GetCollisionFreePosNear(Vector3 startPos, float max_dist)
    // {

    //     if (terrain_manager.myInfo.is_traverable(startPos))
    //         return startPos;

    //     for (int k = 0; k <= 100; k++)
    //     {
    //         Vector3 delta_pos = new Vector3(Random.Range(0f, max_dist), 0f, Random.Range(0f, max_dist));
    //         if (terrain_manager.myInfo.is_traverable(startPos + delta_pos))
    //             return startPos + delta_pos;
    //     }

    //     return Vector3.zero;
    // }
}
