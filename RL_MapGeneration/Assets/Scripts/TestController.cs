using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class TestController : Agent
{
    public override void OnEpisodeBegin()
    {
        Debug.Log("TEST ON");
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Debug.Log("TEST");
    }
}
