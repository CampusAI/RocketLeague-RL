using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using MLAgents.Policies;


public class AgentHelper
{
    public static float NextGaussian(float mean, float std_dev)
    {
        float v1, v2, s;
        do
        {
            v1 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            v2 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            s = v1 * v1 + v2 * v2;
        } while (s >= 1.0f || s == 0f);
        s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);
        return mean + std_dev * v1 * s;
    }

    public static List<GameObject> FindGameObjectInChildWithTag(Transform parent, string tag)
    {
        List<GameObject> children = new List<GameObject>();
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).gameObject.tag == tag)
            {
                children.Add(parent.GetChild(i).gameObject);
            }
        }
        return children;
    }
}