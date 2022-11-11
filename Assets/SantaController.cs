using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class SantaController : Agent
{
    // Start is called before the first frame update
    private float speed = 100;
    public Transform targetKey;
    public Transform targetChest;

    public BoxCollider keySpawningArea;
    public BoxCollider chestSpawningArea;

    private const float MAX_DISTANCE = 48.0f;
    
    private bool keyPickedUp;

    void Start()
    {
    }

    // Update is called once per frame
    public override void OnEpisodeBegin() {
        transform.localPosition = new Vector3(0, 0.5f, 0);
        SpawnTargets();
        keyPickedUp = false;
    }

    private void SpawnTargets(){

        float keyX = Random.Range(keySpawningArea.bounds.min.x, keySpawningArea.bounds.max.x);
        float keyZ = Random.Range(keySpawningArea.bounds.min.z, keySpawningArea.bounds.max.z);

        float chestX = Random.Range(chestSpawningArea.bounds.min.x, chestSpawningArea.bounds.max.x);
        float chestZ = Random.Range(chestSpawningArea.bounds.min.z, chestSpawningArea.bounds.max.z);

        targetKey.position = new Vector3(keyX, 0.5f, keyZ);
        targetChest.position = new Vector3(chestX, 0.5f, chestZ);
    }

    public override void CollectObservations(VectorSensor sensor) {
        // The position of the agent
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);

        // The position of the key prefab
        sensor.AddObservation(targetKey.position.x);
        sensor.AddObservation(targetKey.position.z);

        sensor.AddObservation(targetChest.position.x);
        sensor.AddObservation(targetChest.position.z);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        var actionTaken = actions.ContinuousActions;

        float actionSpeed = (actionTaken[0] + 1) / 2;
        float actionSteering = actionTaken[1]; // [-1, +1]

        transform.Translate(actionSpeed * Vector3.forward * speed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Euler(new Vector3(0, actionSteering * 180, 0));

        float distance_scaled = keyPickedUp ? Vector3.Distance(targetChest.localPosition, transform.localPosition) / MAX_DISTANCE
            : Vector3.Distance(targetKey.localPosition, transform.localPosition) / MAX_DISTANCE;
        //Debug.Log(distance_scaled);

        AddReward(-distance_scaled / 10); // [0, 0.1]
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        ActionSegment<float> actions = actionsOut.ContinuousActions;

        actions[0] = -1;
        actions[1] = 0;

        if (Input.GetKey("w"))
            actions[0] = 1;
        
        if (Input.GetKey("d"))
            actions[1] = +0.5f;

        if (Input.GetKey("a"))
            actions[1] = -0.5f;
        if (Input.GetKey("e"))
            actions[1] = -1;
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.tag == "Lava" || collision.collider.tag == "Wall") {
            AddReward(-1);
            EndEpisode();
        }
        if(collision.collider.tag == "Key"){
            targetKey.localPosition = new Vector3(0, -1, 0);
            keyPickedUp = true;
            AddReward(1);
        }
        if(collision.collider.tag == "Chest" && keyPickedUp){
            AddReward(1);
            EndEpisode();
        }
    }
}
