/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using UnityEngine.UI;
using vrmmo;


public class NetworkController : MonoBehaviour {
	[SerializeField] GameObject player;
	[SerializeField] GameObject uitextobject;
	[SerializeField] const float WORLD_SERVER_SCALE = 25f;
	[SerializeField] const float WORLD_SERVER_MODEL_SCALE = 50f/WORLD_SERVER_SCALE;
	[SerializeField] const float SERVER_SPEED_PERCENT = 0.9f;
	[SerializeField] Vector3 WORLD_SERVER_MODEL_SCALE_VECTOR = new Vector3 (WORLD_SERVER_MODEL_SCALE, WORLD_SERVER_MODEL_SCALE, WORLD_SERVER_MODEL_SCALE);
	[SerializeField] int selectedPrimary = 0;
	//CodeMMO.ClientUserController usercontrol;
	Text uitext;
	string playerID;

	// Use this for initialization
	void Awake () {
		//usercontrol = player.GetComponent<ClientUserController>();
		uitext = uitextobject.GetComponent<Text> ();
		playerID = "";
	}

	void Start () {
		sendStart ("NetworkController Start!");
		uitext.text = "NetworkController initialized...";

		

#if UNITY_EDITOR
		runEditorSimulation();
#endif
	}

	static Plane XZPlane = new Plane(Vector3.up, Vector3.zero);
	
	public static Vector3 GetMousePositionOnXZPlane() {
		float distance;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if(XZPlane.Raycast (ray, out distance)) {
			Vector3 hitPoint = ray.GetPoint(distance);
			//Just double check to ensure the y position is exactly zero
			hitPoint.y = 0;
			return hitPoint;
		}
		return Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonUp (0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
			Vector3 hitpoint = GetMousePositionOnXZPlane();
			float serverx = convertGridFromUnityToServer(hitpoint.x);
			float servery = convertGridFromUnityToServer(hitpoint.z);
			string id = playerID; // TODO: Replace with mouse collided entity?
			sendPrimary(id, serverx, servery, selectedPrimary);
			Debug.Log (hitpoint.ToString());
		}


	}

	void runEditorSimulation(){
		#if UNITY_EDITOR

		runPlayerUpdateSimulation();
		
		Invoke ("runEditorSimulation1", 2);
		Invoke ("runEditorSimulation2", 4);
		#endif
	}

	void runPlayerUpdateSimulation(){
		string testjson = "{\"id\":\"gameserver-5eeb7dbc-6afe-466a-92d4-188decedea6c0ffcd,1\",\"moveAmount\":0.08,\"turnAmount\":0.00314,\"collision\":0,\"x\":75,\"y\":75,\"dir\":0,\"graphic\":10000,\"vx\":0,\"vy\":0,\"layer\":-100}";
		jsUpdatePlayer (testjson);
	}

	void runEditorSimulation1(){
		string testjson = "{\"id\":\"gameserver-5eeb7dbc-6afe-466a-92d4-188decedea6c0ffcd,74\",\"collision\":0,\"x\":75,\"y\":75,\"dir\":0,\"graphic\":10000,\"vx\":0,\"vy\":0,\"layer\":-100}";
		jsNewEntity (testjson);
		testjson = "{\"id\":\"gameserver-5eeb7dbc-6afe-466a-92d4-188decedea6c0ffcd,75\",\"collision\":0,\"x\":125,\"y\":75,\"dir\":0,\"graphic\":10001,\"vx\":0,\"vy\":0,\"layer\":-100}";
		jsNewEntity (testjson);
		testjson = "{\"id\":\"gameserver-5eeb7dbc-6afe-466a-92d4-188decedea6c0ffcd,76\",\"collision\":0,\"x\":175,\"y\":75,\"dir\":0,\"graphic\":10000,\"vx\":0,\"vy\":0,\"layer\":-100}";
		jsNewEntity (testjson);
		testjson = "{\"id\":\"gameserver-5eeb7dbc-6afe-466a-92d4-188decedea6c0ffcd,77\",\"collision\":0,\"x\":175,\"y\":75,\"dir\":0,\"graphic\":3,\"vx\":0,\"vy\":0,\"layer\":-100}";
		jsNewEntity (testjson);
		testjson = "{\"id\":\"gameserver-5eeb7dbc-6afe-466a-92d4-188decedea6c0ffcd,81\",\"collision\":0,\"x\":25,\"y\":125,\"dir\":0,\"graphic\":100,\"vx\":0,\"vy\":0,\"layer\":-100}";
		jsNewEntity (testjson);
		testjson = "{\"id\":\"gameserver-5eeb7dbc-6afe-466a-92d4-188decedea6c0ffcd,82\",\"collision\":0,\"x\":25,\"y\":175,\"dir\":0,\"graphic\":1,\"vx\":0,\"vy\":0,\"layer\":-100}";
		jsNewEntity (testjson);
	}
	void runEditorSimulation2(){
		string testjson = "{\"id\":\"gameserver-5eeb7dbc-6afe-466a-92d4-188decedea6c0ffcd,78\",\"collision\":0,\"x\":125,\"y\":75,\"dir\":0,\"graphic\":1000,\"vx\":0,\"vy\":0,\"layer\":-100}";
		jsNewEntity (testjson);
		testjson = "{\"id\":\"gameserver-5eeb7dbc-6afe-466a-92d4-188decedea6c0ffcd,79\",\"collision\":0,\"x\":75,\"y\":75,\"dir\":0,\"graphic\":1001,\"vx\":0,\"vy\":0,\"layer\":-100}";
		jsNewEntity (testjson);
		testjson = "{\"id\":\"gameserver-5eeb7dbc-6afe-466a-92d4-188decedea6c0ffcd,80\",\"collision\":0,\"x\":25,\"y\":75,\"dir\":3.14,\"graphic\":1002,\"vx\":0.05,\"vy\":0,\"layer\":-100}";
		jsNewEntity (testjson);
	}


	public void setSelectedPrimary(int set){
		selectedPrimary = set;
	}

	// Utility

	float convertGridFromServerToUnity(float pos){
		return pos / WORLD_SERVER_SCALE;
	}
	float convertDirectionFromServerToUnity(float dir){
		return -dir * 180 / Mathf.PI+90;
	}
	float convertGridFromUnityToServer(float pos){
		return pos * WORLD_SERVER_SCALE;
	}
	float convertDirectionFromUnityToServer(float dir){
		dir = dir - 90;
		if (dir < 0)
			dir += 360;
		return (-dir * Mathf.PI / 180)+2*Mathf.PI;
	}

	public Entity getEntityFromJson(JsonObject json){
		// TODO: Reconsider world grid scale to match typical Unity scale
		Entity ent = new Entity ();
		//Debug.Log ("new entity:" + json.ToString ());
		string graphicstring = ((int)(double)json["graphic"]).ToString();
		float x =  convertGridFromServerToUnity((float)(double)json["x"]);
		float y = convertGridFromServerToUnity((float)(double)json["y"]);
		float dir = convertDirectionFromServerToUnity((float)(double) json["dir"]);
		string id = (string)json["id"];
		float vx = convertGridFromServerToUnity((float)(double) json["vx"])*1000f;
		float vy = convertGridFromServerToUnity((float)(double) json["vy"])*1000f;
		ent.x = x;
		ent.y = y;
		ent.dir = dir;
		ent.id = id;
		ent.vx = vx;
		ent.vy = vy;
		ent.graphic = graphicstring;
		return ent;
	}

	// TODO: Automatically remove out-of-range entities to save memory

	// Javascript connector sending functions

	// Run unityStart
	public void sendStart(string data){
		//Application.ExternalCall("unityStart", data);
	}

	// Run unityPrimary
	public void sendPrimary(string id, float x, float y, int actionid){
		JsonObject json = new JsonObject ();
		json.Add (new KeyValuePair<string, object> ("id", (string)(id)));
		json.Add (new KeyValuePair<string, object> ("x", (double)(x)));
		json.Add (new KeyValuePair<string, object> ("y", (double)(y)));
		json.Add (new KeyValuePair<string, object> ("actionid", actionid));
		//Application.ExternalCall("unityPrimary", json.ToString());
	}

	// Run unityMovePlayer
	public void sendMovePlayer(Transform transform){
		JsonObject json = new JsonObject ();
		json.Add (new KeyValuePair<string, object> ("x", (double)(convertGridFromUnityToServer(transform.position.x))));
		json.Add (new KeyValuePair<string, object> ("y", (double)(convertGridFromUnityToServer(transform.position.z))));
		json.Add (new KeyValuePair<string, object> ("dir", (double)(convertDirectionFromUnityToServer(transform.rotation.eulerAngles.y))));
		//Application.ExternalCall("unityMovePlayer", json.ToString());
	}


	// Javascript connector listening functions

	// js is connecting to server for first time
	public void jsRequestingServer (string data){
		Debug.Log ("Unity received jsRequestingServer");
		uitext.text = "Requesting server...";
	}

	public void jsConnectingTo (string data){
		Debug.Log ("Unity received jsConnectingTo");
		uitext.text = "Connecting to server...";
	}
	
	// js connected to server (not necessarily in game yet)
	public void jsConnected (string data){
		Debug.Log ("Unity received jsConnected");
		uitext.text = "Connected to server.";
	}
	
	// js disconnected from server
	public void jsDisconnected (string data){
		Debug.Log ("Unity received jsDisconnected");
		uitext.text = "Disconnected.";
	}

	// server sending new entity for rendering
	//{"id":"gameserver-5eeb7dbc-6afe-466a-92d4-188decedea6c0ffcd,74","collision":0
    //,"x":575,"y":75,"dir":0,"graphic":10000,"vx":0,"vy":0,"layer":-100}
	public void jsNewEntity (string data){
		jsUpdateEntity (data);
	}

	void InstantiateNewEntity (JsonObject json){
		Entity ent = getEntityFromJson (json);


		GameObject loadedResource = (GameObject) Resources.Load ("EntityGraphics/" + ent.graphic, typeof(GameObject));
		if (loadedResource == null){
			Debug.LogError("No resource found matching graphic:"+ent.graphic);
			loadedResource = (GameObject) Resources.Load ("EntityGraphics/placeholder", typeof(GameObject));
		}
		
		Vector3 pos = new Vector3 (ent.x, loadedResource.transform.position.y, ent.y);
		Quaternion rot = Quaternion.Euler(new Vector3 (loadedResource.transform.rotation.eulerAngles.x, ent.dir, loadedResource.transform.rotation.eulerAngles.z));

		GameObject entity = (GameObject) Instantiate(loadedResource, pos, rot);
		//entity.transform.localScale = Vector3.Scale (entity.transform.localScale, WORLD_SERVER_MODEL_SCALE_VECTOR);
		entity.name = ent.id;
		
		//Debug.Log ("vy:" + ent.vy);
		//Debug.Log ("vx:" + ent.vx);
		if (ent.vx != 0 || ent.vy != 0) {
			Rigidbody rb = entity.GetComponent<Rigidbody>();
			if (rb != null){
				rb.velocity = new Vector3(ent.vx, 0, ent.vy);
				Debug.Log("Applied velocity to:"+ent.graphic);
			} else {
				Debug.LogError("No rigidbody on velocity object:"+ent.graphic);
			}
		}
		
		// TODO: Add collision
	}
	
	// server sending updated entity for moving, etc.
	public void jsUpdateEntity (string data){
		// TODO: Ignore if update matches player id
		JsonObject json = (JsonObject)SimpleJson.SimpleJson.DeserializeObject (data);
		string id = (string)json["id"];
		GameObject current = GameObject.Find (id);
		if (current == null) {
			InstantiateNewEntity (json);
		} else {
			Entity ent = getEntityFromJson (json);
			Vector3 pos = new Vector3 (ent.x, 0, ent.y);
			Quaternion rot = Quaternion.Euler(new Vector3 (0, ent.dir, 0));
			current.transform.position = pos;
			current.transform.rotation = rot;

			// TODO: Update graphics when necessary
		}
	}
	
	// server sending player update
	public void jsUpdatePlayer (string data){
		JsonObject json = (JsonObject)SimpleJson.SimpleJson.DeserializeObject (data);
		Entity ent = getEntityFromJson (json);
		playerID = ent.id;
		Vector3 pos = new Vector3 (ent.x, 0, ent.y);
		Quaternion rot = Quaternion.Euler(new Vector3 (0, ent.dir, 0));
		player.transform.position = pos;
		player.transform.rotation = rot;
		//usercontrol.setMoveMultiplier(SERVER_SPEED_PERCENT*1000f / WORLD_SERVER_SCALE * ((float)(double)json["moveAmount"]));
		//usercontrol.setTurnMultiplier(SERVER_SPEED_PERCENT*1000f * 180f * ((float)(double)json["turnAmount"]) / Mathf.PI);
		// TODO: Update graphics when necessary
	}

	// server sending remove entity
	public void jsRemoveEntity (string data){
		JsonObject json = (JsonObject)SimpleJson.SimpleJson.DeserializeObject (data);
		string id = (string)json["id"];
		GameObject current = GameObject.Find (id);
		if (current == null) {
		} else {
			Destroy(current);
		}
	}


}
*/