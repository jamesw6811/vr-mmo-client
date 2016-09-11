using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System;
using System.Net;
using VrMMOServer;
using System.Collections.Generic;

public class UDPNetworkController : MonoBehaviour, GamePacketListener
{
    [SerializeField]
    GameObject player;
    GameNetworkingClient gc;
    LinkedList<GamePacket> gamePackets;

	// Use this for initialization
	void Start () {
        gamePackets = new LinkedList<GamePacket>();
        gc = new GameNetworkingClient();
        gc.registerPacketListener(this);
        gc.startClient();
        notifyPlayerUpdated();
	}
	
	// Update is called once per frame
	void Update () {
        handlePackets();
	}

    private void handlePackets()
    {
        lock (gamePackets){
            foreach (GamePacket gp in gamePackets)
            {
                handlePacket(gp);
            }
        }
    }

    private void handlePacket(GamePacket gp)
    {
        if (gp is EntityUpdatePacket)
        {
            onReceiveEntityUpdate((EntityUpdatePacket)gp);
        } 
        else
        {
            throw new NotImplementedException("No code to handle GamePacket of this type:" + gp.ToString());
        }
    }

    public void receiveGamePacket(GamePacket gp)
    {
        lock (gamePackets)
        {
            gamePackets.AddLast(gp);
        }
    }

    public void notifyPlayerUpdated()
    {
        EntityUpdatePacket eup = new EntityUpdatePacket();
        eup.x = player.transform.position.x;
        eup.y = player.transform.position.z;
        eup.upDown = player.transform.FindChild("Neck").FindChild("Head").rotation.eulerAngles.x;
        eup.leftRight = player.transform.rotation.eulerAngles.y;
        eup.tilt = player.transform.FindChild("Neck").FindChild("Head").rotation.eulerAngles.z;
        gc.sendUpdatePlayer(eup);
    }
    
    private void onReceiveEntityUpdate(EntityUpdatePacket eup)
    {
        /*
        Debug.Log("x: " + x);
        Debug.Log("y: " + y);
        Debug.Log("id: " + id);
        Debug.Log("graphic: " + graphic);
        */
        GameEntity ent = new GameEntity();
        ent.x = eup.x;
        ent.y = eup.y;
        ent.upDown = eup.upDown;
        ent.leftRight = eup.leftRight;
        ent.tilt = eup.tilt;
        ent.id = eup.id;
        ent.graphic = eup.graphic;
        updateEntity(ent);
    }

    public static byte[] StringToByteArrayFastest(string hex)
    {
        if (hex.Length % 2 == 1)
            throw new System.Exception("The binary key cannot have an odd number of digits");

        byte[] arr = new byte[hex.Length >> 1];

        for (int i = 0; i < hex.Length >> 1; ++i)
        {
            arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
        }

        return arr;
    }

    public static int GetHexVal(char hex)
    {
        int val = (int)hex;
        //For uppercase A-F letters:
        return val - (val < 58 ? 48 : 55);
        //For lowercase a-f letters:
        //return val - (val < 58 ? 48 : 87);
        //Or the two combined, but a bit slower:
        //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
    }

    void instantiateNewEntity(GameEntity ent)
    {
        GameObject loadedResource = (GameObject)Resources.Load("EntityGraphics/" + ent.graphic, typeof(GameObject));
        if (loadedResource == null)
        {
            Debug.LogError("No resource found matching graphic:" + ent.graphic);
            loadedResource = (GameObject)Resources.Load("EntityGraphics/placeholder", typeof(GameObject));
        }

        Vector3 pos = new Vector3(0, loadedResource.transform.position.y, 0);
        Quaternion rot = Quaternion.Euler(new Vector3(0, 0, loadedResource.transform.rotation.eulerAngles.z));

        GameObject entity = (GameObject)Instantiate(loadedResource, pos, rot);
        //entity.transform.localScale = Vector3.Scale (entity.transform.localScale, WORLD_SERVER_MODEL_SCALE_VECTOR);
        entity.name = serverIdToGameObjectName(ent.id.ToString());

        //Debug.Log ("vy:" + ent.vy);
        //Debug.Log ("vx:" + ent.vx);
        /*
        if (ent.vx != 0 || ent.vy != 0)
        {
            Rigidbody rb = entity.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = new Vector3(ent.vx, 0, ent.vy);
                Debug.Log("Applied velocity to:" + ent.graphic);
            }
            else
            {
                Debug.LogError("No rigidbody on velocity object:" + ent.graphic);
            }
        }*/

        updateEntity(ent);

        // TODO: Add collision
    }

    private String serverIdToGameObjectName(String id)
    {
        return "serverId" + id;
    }

    // server sending updated entity for moving, etc.
    public void updateEntity(GameEntity ent)
    {
        GameObject current = GameObject.Find(serverIdToGameObjectName(ent.id.ToString()));
        if (current == null)
        {
            instantiateNewEntity(ent);
        }
        else
        {
            Vector3 pos = new Vector3(ent.x, 0, ent.y);
            current.transform.position = pos;
            HeadNodder hn = current.GetComponent<HeadNodder>();
            if (hn != null)
            {
                Quaternion rot = Quaternion.Euler(new Vector3(0, ent.leftRight, 0));
                hn.nod(ent.upDown, ent.tilt);
                current.transform.rotation = rot;
            }
            else
            {
                Quaternion rot = Quaternion.Euler(new Vector3(ent.upDown, ent.leftRight, 0));
                current.transform.rotation = rot;
            }
        }
    }
}
