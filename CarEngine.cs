using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarEngine : MonoBehaviour {

	public Transform path;
	public float maxStearAngle = 45f;
    public float turnSpeed = 5f;
	public WheelCollider wheelfl;
	public WheelCollider wheelfr;
	public WheelCollider wheelrl;
	public WheelCollider wheelrr;

	public float maxMotorTorque = 80f;
	public float maxBreakeTorque = 150f;
	public float currentSpeed;
	public float maxSpeed = 100f;
	public Vector3 centerOfMass;
	public bool isBraking = false;
	public Texture2D textureNormal;
	public Texture2D textureBraking;
	public Renderer carRenderer;

	[Header("Sensors")]
	public float sensorLenght = 3f;
	public Vector3 frontSensorPosition = new Vector3 (0f, 0.2f, 0.5f);
	public float frontSideSensorPosition = 0.3f;
	public float frontSensorAngle = 30f;

	private List<Transform> nodes;
	public int currentNode = 0;

    private bool avoiding = false;
    private float targetStearAngle = 0;

	private void Start () {

		GetComponent<Rigidbody> ().centerOfMass = centerOfMass;

		Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
		nodes = new List<Transform>();

		for (int i = 0; i < pathTransforms.Length; i++) {
			if(pathTransforms[i] != path.transform){
				nodes.Add (pathTransforms[i]);
			}
		}
	}
	
	private void FixedUpdate () {
		Sensors ();
		ApplyStear ();
		Drive ();
		CheckPointDistance ();
		Braking ();
        LerpToSteerAngle();
	}

	private void ApplyStear(){
        if (avoiding) return;
		Vector3 relativevector = transform.InverseTransformPoint (nodes[currentNode].position);
		//relativevector = relativevector / relativevector.magnitude;

		float newStear = (relativevector.x / relativevector.magnitude) * maxStearAngle;

        targetStearAngle = newStear;
	}

	private void Drive(){
		currentSpeed = 2 * Mathf.PI * wheelfl.radius * wheelfl.rpm * 60 / 1000;

        //if (isBraking && currentSpeed <5){
        //    isBraking = false;
        //}

		if (currentSpeed < maxSpeed && !isBraking) {
			wheelfl.motorTorque = maxMotorTorque;
			wheelfr.motorTorque = maxMotorTorque;
		} else {
			wheelfl.motorTorque = 0;
			wheelfr.motorTorque = 0;
		}
	}

	private void CheckPointDistance(){
		if (Vector3.Distance (transform.position, nodes [currentNode].position) < 0.5f) {
			if (currentNode == nodes.Count - 1) {
				currentNode = 0;
			} else {
				currentNode++;
			}
		}
	}

	public void Braking(){
		if (isBraking) {
			//carRenderer.material.mainTexture = textureBraking;
			wheelrl.brakeTorque = maxBreakeTorque;
			wheelrr.brakeTorque = maxBreakeTorque;
		} else {
			//carRenderer.material.mainTexture = textureNormal;
			wheelrl.brakeTorque = 0;
			wheelrr.brakeTorque = 0;
		}
	}

	public void Sensors(){
		RaycastHit hit;
		Vector3 sensorStartPos = transform.position;
        sensorStartPos += transform.forward * frontSensorPosition.z;
        sensorStartPos += transform.up * frontSensorPosition.y;
        float avoidMultiplier = 0f;
        avoiding = false;

		//front center
        if (avoidMultiplier == 0){
            if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLenght))
            {
                if (!hit.collider.CompareTag("Terrain"))
                {
                    Debug.DrawLine(sensorStartPos, hit.point);
                    isBraking = true;
                }
            } else {
               isBraking = false;               
            }
        }



        if (avoiding){
            targetStearAngle = maxStearAngle * avoidMultiplier;
        }

	}

    private void LerpToSteerAngle(){
        wheelfl.steerAngle = Mathf.Lerp(wheelfl.steerAngle, targetStearAngle, Time.deltaTime * turnSpeed);
        wheelfr.steerAngle = Mathf.Lerp(wheelfr.steerAngle, targetStearAngle, Time.deltaTime * turnSpeed);
    }

}
