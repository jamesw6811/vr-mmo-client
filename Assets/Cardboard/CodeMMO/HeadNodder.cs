using UnityEngine;
using System.Collections;

public class HeadNodder : MonoBehaviour {
    [SerializeField]
    GameObject head;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {

	}

    public void nod(float nodAmt)
    {
        Vector3 headrot = head.transform.eulerAngles;
        headrot.x = nodAmt;
        head.transform.eulerAngles = headrot;
    }
}
