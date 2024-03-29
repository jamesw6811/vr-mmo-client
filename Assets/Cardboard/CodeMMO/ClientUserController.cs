﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace CodeMMO {

	public class ClientUserController : MonoBehaviour {
        [SerializeField] Text debugText;
		[SerializeField] UDPNetworkController networkController;
		private float networkMoveMultiplier = 1f;
		private float networkTurnMultiplier = 1f;
        private bool isMoving = false;
        [SerializeField]
        GameObject head;
        Vector3 lastHeadAngle;
		
		private void Start()
		{
            lastHeadAngle = new Vector3(0, 0, 0);
		}
		
		
		private void Update()
		{
            if (Cardboard.SDK.Triggered)
            {
                isMoving = !isMoving;
            }
		}
		
		
		// Fixed update is called in sync with physics
		private void FixedUpdate()
		{
            bool updated = false;

            // Correct rotation to head
            Vector3 headTurn = head.transform.eulerAngles;
            Vector3 bodyTurn = this.transform.eulerAngles;
            if (lastHeadAngle != headTurn)
            {
                lastHeadAngle = headTurn;
                updated = true;
                Debug.Log("Not equal.");
                Debug.Log(bodyTurn.ToString());
                Debug.Log(headTurn.ToString());
                // Body
                this.transform.eulerAngles = new Vector3(0f, headTurn.y, 0f);
                // Head
                head.transform.eulerAngles = headTurn;
            }

			// read inputs and move
            float v = 0;
            if (isMoving)
            {
                v = 1f;
                updated = true;
            }
            else
            {
                v = 0f;
            }

            Vector3 worldMoveDir = this.transform.TransformVector(Vector3.forward * v * networkMoveMultiplier * Time.fixedDeltaTime);
            worldMoveDir.y = 0; // Don't move along y
            this.transform.Translate(worldMoveDir, Space.World);
            debugText.text = this.transform.eulerAngles.ToString() + " " + this.transform.position.ToString();

            if (updated)
            {
                networkController.notifyPlayerUpdated();
            }
		}

		public void setMoveMultiplier(float multiplier){
			networkMoveMultiplier = multiplier;
		}
		public void setTurnMultiplier(float multiplier){
			networkTurnMultiplier = multiplier;
		}
	}

}