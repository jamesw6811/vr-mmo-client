using UnityEngine;
using System.Collections;

public class HeadNodder : MonoBehaviour {
    [SerializeField]
    GameObject head;

    float nodAmt;
    float tiltAmt;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
        Vector3 headrot = head.transform.eulerAngles;
        headrot.x = nodAmt;
        headrot.z = tiltAmt;
        head.transform.eulerAngles = headrot;
	}

    public void nod(float nod, float tilt)
    {
        nodAmt = nod;
        tiltAmt = tilt;
    }
}
