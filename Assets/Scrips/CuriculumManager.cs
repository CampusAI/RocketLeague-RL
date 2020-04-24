using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CurriculumManager
{
    public int window_size = 300;
    private System.Collections.Generic.List<float> reward_window = new System.Collections.Generic.List<float>();

    private int level = 0;
    public float[] average_to_pass = new float[] { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
    public Dictionary<string, float[]> parameters = new Dictionary<String, float[]>()
    {
        {"car_pos_offset",  new float[]{0, 10, 20, 10, 20, 30}},
        {"car_rot_offset",  new float[]{0, 5,  10, 20, 45, 90}},
        {"car_vel_offset",  new float[]{0, 0,  0,   2,  5,  7}},
        {"ball_pos_offset", new float[]{0, 5,  10, 10, 20, 30}},
        {"ball_rot_offset", new float[]{0, 0,   0,  2,   3, 7}},
        {"ball_vel_offset", new float[]{0, 0,   0,  2,   5, 7}}
    };

    public void LogReward(float reward)
    {
        // Debug.LogWarning(reward);
        reward_window.Add(reward);
        if (reward_window.Count > window_size)
            reward_window.RemoveAt(0);
        // Debug.Log(reward_window);
        UpdateStatus();
    }

    public Dictionary<string, float> GetParams()
    {
        var p = new Dictionary<string, float>();
        foreach (KeyValuePair<string, float[]> entry in parameters)
        {
            p.Add(entry.Key, entry.Value[level]);
        }
        return p;
    }

    private void UpdateStatus()
    {
        // Debug.Log("#########");
        // Debug.Log(reward_window.Count);
        // Debug.Log(Average(reward_window));
        if (reward_window.Count < window_size)
            return;
        float average = Average(reward_window);
        if (average > average_to_pass[level])
        {
            level = Mathf.Min(level + 1, average_to_pass.Length);
            reward_window = new System.Collections.Generic.List<float>();
            Debug.Log("Passed to level " + level.ToString() + ", average score: " + average.ToString());
        }
    }

    private float Average(List<float> list)
    {
        if (list.Count == 0) return 0;
        float sum = 0;
        foreach (float elem in list)
        {
            sum += elem;
        }
        return sum / ((float)list.Count);
    }
}