using UnityEngine;
using System.Collections;

public class SpeedEstimatorAnimator : MonoBehaviour {
    Animator animator;
    float averageMoveSpeed = 0;
    Vector3 lastPosition = new Vector3();
	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        float dt = Time.fixedDeltaTime;
        Vector3 thisPosition = transform.position;
        Vector3 movement = thisPosition - lastPosition;
        float movemag = movement.magnitude;
        float speedmag = movemag / dt;

        averageMoveSpeed -= averageMoveSpeed / (0.5f/dt);
        averageMoveSpeed += speedmag / (0.5f/dt);
        animator.SetFloat("AverageMoveSpeed", averageMoveSpeed);

        lastPosition = thisPosition;
	}
}
