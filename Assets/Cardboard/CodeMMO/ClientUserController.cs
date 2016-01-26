using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace CodeMMO {

	public class ClientUserController : MonoBehaviour {
        [SerializeField] Text debugText;
		[SerializeField] UDPNetworkController networkController;
		private float networkMoveMultiplier = 1f;
		private float networkTurnMultiplier = 1f;
		private Transform m_Cam;                  // A reference to the main camera in the scenes transform
		private Vector3 m_CamForward;             // The current forward direction of the camera
		private Vector3 m_Move;         // the world-relative desired move direction, calculated from the camForward and user input.
		private float m_CamScale; // The current position of the camera controlled by the scroll wheel.
        private bool isMoving = false;
        [SerializeField]
        CardboardHead head;
		
		private void Start()
		{
			m_CamScale = 0;
			// get the transform of the main camera
			if (Camera.main != null)
			{
				m_Cam = Camera.main.transform;
			}
			else
			{
				Debug.LogWarning(
					"Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
				// we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
			}

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
			// read inputs

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

            worldMoveDir.y = 0;

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