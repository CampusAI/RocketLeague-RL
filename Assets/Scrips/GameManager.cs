using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {


    public GameObject terrain_manager_game_object;
    TerrainManager terrain_manager;

    public GameObject race_car;

    float start_time;
    public float completion_time;
    public float goal_tolerance = 10.0f;
    public bool finished = false;
    public int no_of_cars = 10;

    public List<GameObject> my_cars;

    // Use this for initialization
    void Awake () {

        terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
       
        start_time = Time.time;

        race_car.transform.position = terrain_manager.myInfo.start_pos + 2f* Vector3.up;
        race_car.transform.rotation = Quaternion.identity;


    }

    void Start()
    {
        start_time = Time.time;

        my_cars = new List<GameObject>();

        race_car.transform.position = terrain_manager.myInfo.start_pos + 2f * Vector3.up;
        race_car.transform.rotation = Quaternion.identity;

        for (int i = 0; i < no_of_cars; i++)
        {
            GameObject new_car;
            UnityStandardAssets.Vehicles.Car.CarAI new_AI;

            new_car = Instantiate(race_car, new Vector3(20.0f + i * 8.0f, 10.0f, 20f), Quaternion.identity);
            Vector3 nominal_pos = CircularConfiguration(i+ (int) Mathf.Floor(no_of_cars/2), no_of_cars, 0.75f);
            new_car.transform.position = GetCollisionFreePosNear(nominal_pos, 50f);
            my_cars.Add(new_car);

            GameObject car_sphere = new_car.transform.Find("Sphere").gameObject;
            Color my_color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            car_sphere.GetComponent<Renderer>().material.SetColor("_Color", my_color);

            GameObject goal_sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            nominal_pos = CircularConfiguration(i, no_of_cars, 0.8f);
            goal_sphere.transform.position = GetCollisionFreePosNear(nominal_pos, 50f);

            goal_sphere.transform.localScale = Vector3.one * 3f;  
            goal_sphere.GetComponent<Renderer>().material.SetColor("_Color", my_color);

            // Assign goal point
            new_AI = new_car.GetComponent<UnityStandardAssets.Vehicles.Car.CarAI>();
            new_AI.my_goal_object = goal_sphere;
            new_AI.terrain_manager_game_object = terrain_manager_game_object;

            //new_AI.my_goal = goal_sphere.transform.position;
            //var cubeRenderer = new_sphere.GetComponent<Renderer>();
            //var cubeRenderer = new_car.GetComponent<Sphere>();


            //Call SetColor using the shader property name "_Color" and setting the color to red
            //cubeRenderer.material.SetColor("_Color", Color.cyan);
        }
    }

    // Update is called once per frame
    void Update () {
        if(!finished) // check is all cars have reached their goals, if so stop clock
        {
            bool all_cars_done = true;
            foreach(GameObject one_car in my_cars)
            {
                Vector3 car_pos = one_car.transform.position;
                Vector3 goal_pos = one_car.GetComponent<UnityStandardAssets.Vehicles.Car.CarAI>().my_goal_object.transform.position;
                if((car_pos-goal_pos).sqrMagnitude > goal_tolerance * goal_tolerance)
                {
                    Debug.Log("distSqr" + (car_pos - goal_pos).sqrMagnitude);
                    Debug.DrawLine(car_pos, goal_pos, Color.magenta);
                    all_cars_done = false;
                    break;
                }
            }
            if(all_cars_done)
            {
                finished = true;
                completion_time = Time.time - start_time;
            }


           
        }

	}

    Vector3 CircularConfiguration(int i, int max_i, float factor_k)
    {
        float center_x = (terrain_manager.myInfo.x_high + terrain_manager.myInfo.x_low) / 2.0f;
        float center_z = (terrain_manager.myInfo.z_high + terrain_manager.myInfo.z_low) / 2.0f;
        Vector3 center = new Vector3(center_x, 1f, center_z);

        float alpha = i * 2.0f * Mathf.PI / max_i;
        float r = (terrain_manager.myInfo.x_high - center_x) * factor_k;
        return center + new Vector3(Mathf.Sin(alpha), 0f, Mathf.Cos(alpha)) * r;
    }

    Vector3 GetCollisionFreePosNear(Vector3 startPos, float max_dist)
    {

        if (terrain_manager.myInfo.is_traverable(startPos))
            return startPos;

        for (int k = 0; k <= 100; k++)
        {
            Vector3 delta_pos = new Vector3(Random.Range(0f, max_dist), 0f, Random.Range(0f, max_dist));
            if (terrain_manager.myInfo.is_traverable(startPos + delta_pos))
                return startPos + delta_pos;
        }

        return Vector3.zero;
    }
}
