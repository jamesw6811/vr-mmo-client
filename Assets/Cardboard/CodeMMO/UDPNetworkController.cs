using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System;
using System.Net;
using vrmmo;

public class UDPNetworkController : MonoBehaviour
{
    [SerializeField]
    GameObject player;
    private static byte[] protocolBytes = { 0xD8, 0x2A };
    private static byte[] playerUpdateBytes = { 0x10, 0x00 };
    private static byte[] entityUpdateBytes = { 0x10, 0x01 };
    private static ArrayList receivedPackets = new ArrayList();
    private static int port = 33333;
    UdpClient udpServer;
    IPEndPoint e = new IPEndPoint(IPAddress.Any, port);

	// Use this for initialization
	void Start () {
        udpServer = new UdpClient("192.168.86.162", port);
        
        udpServer.BeginReceive(new AsyncCallback(recv), null);
        
        MemoryStream ms = new MemoryStream();
        ms.Write(protocolBytes, 0, protocolBytes.Length);
        writePlayerUpdate(ms);

        byte[] msgBytes = ms.GetBuffer();
        udpServer.Send(msgBytes, msgBytes.Length);


	}
	
	// Update is called once per frame
    private float headtest = 0;
	void Update () {
        try
        {
            lock (receivedPackets)
            {
                foreach (byte[] item in receivedPackets)
                {
                    parseReceived(item);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        receivedPackets.Clear();

        MemoryStream ms = new MemoryStream();
        ms.Write(protocolBytes, 0, protocolBytes.Length);
        writePlayerUpdate(ms);
        byte[] msgBytes = ms.GetBuffer();
        udpServer.Send(msgBytes, msgBytes.Length);

        Entity ent = new Entity();
        ent.x = 0;
        ent.y = 2;
        ent.leftRight = 90;
        ent.upDown = 45 * (float)Math.Sin(headtest);
        headtest += 0.05f;
        ent.id = 111;
        ent.graphic = 0;
        updateEntity(ent);

        ent = new Entity();
        ent.x = 1;
        ent.y = 2;
        ent.leftRight = 90;
        ent.upDown = 45 * (float)Math.Sin(headtest);
        headtest += 0.05f;
        ent.id = 112;
        ent.graphic = 1;
        updateEntity(ent);
	}

    private void recv(IAsyncResult ar)
    {
        lock (receivedPackets)
        {
            Byte[] receiveBytes = udpServer.EndReceive(ar, ref e);
            udpServer.BeginReceive(new AsyncCallback(recv), null);
            receivedPackets.Add(receiveBytes);
        }
    }

    private void parseReceived(byte[] receiveBytes)
    {
        //Debug.Log("Packet received:");
        //Debug.Log(BitConverter.ToString(receiveBytes));
        bool correctProtocol = subByteEqual(receiveBytes, 0, protocolBytes);
        if (!correctProtocol)
        {
            throw new Exception("Protocol not recognized.");
        }

        bool entityUpdate = subByteEqual(receiveBytes, 2, entityUpdateBytes);
        bool playerUpdate = subByteEqual(receiveBytes, 2, playerUpdateBytes);
        //Debug.Log("Entity update: " + entityUpdate);
        if (entityUpdate)
        {
            onReceiveEntityUpdate(receiveBytes, 4);
        }
    }

    private void onReceiveEntityUpdate(byte[] receiveBytes, int offset)
    {
        float x = NetworkFloatToHostOrder(receiveBytes, offset);
        float y = NetworkFloatToHostOrder(receiveBytes, offset + 4);
        float upDown = NetworkFloatToHostOrder(receiveBytes, offset + 8);
        float leftRight = NetworkFloatToHostOrder(receiveBytes, offset + 12);

        UInt16 id = NetworkUInt16ToHostOrder(receiveBytes, offset + 16);
        UInt16 graphic = NetworkUInt16ToHostOrder(receiveBytes, offset + 18);
        /*
        Debug.Log("x: " + x);
        Debug.Log("y: " + y);
        Debug.Log("id: " + id);
        Debug.Log("graphic: " + graphic);
        */
        Entity ent = new Entity();
        ent.x = x;
        ent.y = y;
        ent.upDown = upDown;
        ent.leftRight = leftRight;
        ent.id = id;
        ent.graphic = graphic;
        updateEntity(ent);
    }

    public void writePlayerUpdate(MemoryStream ms)
    {
        ms.Write(playerUpdateBytes, 0, playerUpdateBytes.Length);
        float x = player.transform.position.x;
        float y = player.transform.position.z;
        float upDown = player.transform.rotation.eulerAngles.x;
        float leftRight = player.transform.rotation.eulerAngles.y;
        writeAll(ms, HostToNetworkOrder(x));
        writeAll(ms, HostToNetworkOrder(y));
        writeAll(ms, HostToNetworkOrder(upDown));
        writeAll(ms, HostToNetworkOrder(leftRight));
    }

    public static void writeAll(MemoryStream ms, byte[] arr)
    {
        ms.Write(arr, 0, arr.Length);
    }

    public static byte[] HostToNetworkOrder(byte[] host)
    {
        byte[] bytes = host;

        if (System.BitConverter.IsLittleEndian)
            System.Array.Reverse(bytes);

        return bytes;
    }

    public static byte[] HostToNetworkOrder(float host)
    {
        return HostToNetworkOrder(System.BitConverter.GetBytes(host));
    }

    public static byte[] HostToNetworkOrder(UInt32 host)
    {
        return HostToNetworkOrder(System.BitConverter.GetBytes(host));
    }

    public static float NetworkFloatToHostOrder(byte[] network, int offset)
    {
        byte[] bytes = network;

        if (System.BitConverter.IsLittleEndian)
            System.Array.Reverse(bytes, offset, 4);

        return System.BitConverter.ToSingle(bytes, offset);
    }

    public static UInt16 NetworkUInt16ToHostOrder(byte[] network, int offset)
    {
        byte[] bytes = network;

        if (System.BitConverter.IsLittleEndian)
            System.Array.Reverse(bytes, offset, 2);

        return System.BitConverter.ToUInt16(bytes, offset);
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

    // a1 is big array to compare with offset a1offset
    // b1 is specific array segment to look for
    public bool subByteEqual(byte[] a1, int a1offset, byte[] b1)
    {
        int i;
        i = 0;
        while (i < b1.Length && (a1[i+a1offset] == b1[i])) //Earlier it was a1[i]!=b1[i]
        {
            i++;
        }
        if (i == b1.Length)
        {
            return true;
        }

        return false;
    }

    void instantiateNewEntity(Entity ent)
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
        entity.name = ent.id.ToString();

        //Debug.Log ("vy:" + ent.vy);
        //Debug.Log ("vx:" + ent.vx);
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
        }

        updateEntity(ent);

        // TODO: Add collision
    }

    // server sending updated entity for moving, etc.
    public void updateEntity(Entity ent)
    {
        GameObject current = GameObject.Find(ent.id.ToString());
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
                hn.nod(ent.upDown);
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
