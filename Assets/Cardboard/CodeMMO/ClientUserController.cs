using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace CodeMMO {

	public class ClientUserController : MonoBehaviour {
        [SerializeField] Text debugText;
		[SerializeField] UDPNetworkController networkController;
		private float networkMoveMultiplier = 1f;
		private float networkTurnMultiplier = 1f;
		private Vector3 m_Move;         // the world-relative desired move direction, calculated from the camForward and user input.
		private float m_CamScale; // The current position of the camera controlled by the scroll wheel.
        private bool isMoving = false;
        [SerializeField]
        GameObject head;
        [SerializeField]
        GameObject neck;
		
		private void Start()
		{

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
            // Correct rotation to head
            Vector3 headTurn = head.transform.eulerAngles;
            Vector3 bodyTurn = this.transform.eulerAngles;
            bodyTurn.y = headTurn.y;
            this.transform.eulerAngles = bodyTurn;

			// read inputs and move
            float v = 0;
            if (isMoving)
            {
                v = 0.5f;
            }
            else
            {
                v = 0f;
            }

            Vector3 worldMoveDir = this.transform.TransformVector(Vector3.forward * v * networkMoveMultiplier * Time.fixedDeltaTime);
            worldMoveDir.y = 0; // Don't move along y
            this.transform.Translate(worldMoveDir, Space.World);
            debugText.text = this.transform.eulerAngles.ToString() + " " + this.transform.position.ToString();

			m_Move = v*this.transform.forward;

			// TODO: add back
			if (m_Move.magnitude > 0) {
				//networkController.sendMovePlayer(this.transform);
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