﻿using System.Collections;
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

        // Start is called before the first frame update
        void Start()
        {
            transform.position = ball_spawn_point.transform.position;
            players = new List<GameObject>();
            players.AddRange(GameObject.FindGameObjectsWithTag("Blue"));
            players.AddRange(GameObject.FindGameObjectsWithTag("Red"));
        }

        void FixedUpdate()
        {
        }

        public void ResetGame() {
            ResetBall();
            blue_score = 0;
            red_score = 0;
        }

        public void ResetBall() {
            transform.position = ball_spawn_point.transform.position;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
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
            if (players.Contains(collision.gameObject)) // if a player touched it
            {
                if (blue_score + red_score == 0)
                    collision.gameObject.GetComponent<CarRLAgent>().TouchedBall();
            }
        }
    }
}