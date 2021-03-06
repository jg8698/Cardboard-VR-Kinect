﻿using UnityEngine;
using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using UnityEngine.UI;
using System.Threading;
using System.Collections;
using System.Diagnostics;


public class clientRokoborba : MonoBehaviour
{
    public UdpClient udpClient1;
    public IPEndPoint ep1;
    
    humanBody[] dvaObjekta = new humanBody[2];
    int write = 0, read = 1, temp;
    public GameObject input, input2;
    public bool reset = false;
    public String ip_adress;
    private static clientRokoborba instance;
    private Matrix4x4 kinectToWorld;
    public float SensorHeight = 1.0f;
    public int SensorAngle = 0;
    public byte[] data;
    public IPEndPoint newIncomingEndPoint;

    public static clientRokoborba Instance
    {
        get
        {
            return instance;
        }
    }

    public void getInput(string ip)
    {
        ip_adress = ip;
        input.SetActive(false);
        input.GetComponent<InputField>().text = "";
        Start_Now();
    }    

    //NOVO
    public void getCooplayerIp(string ip)
    {        
        input2.SetActive(false);
        input2.GetComponent<InputField>().text = "";
        
        try
        {
            IPAddress.Parse(ip);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("Napačen naslov!" + e);
            input2.SetActive(true);
            return;
        }

        udpClient1 = new UdpClient();
        ep1 = new IPEndPoint(IPAddress.Parse(ip), 12000);
        udpClient1.Connect(ep1);
        udpClient1.Send(new byte[] { 1 }, 1);
        udpClient1.Client.ReceiveTimeout = 5000;
        udpClient1.Receive(ref ep1);
    }    

    void Awake()
    {
        DontDestroyOnLoad(this);

        if (FindObjectsOfType(this.GetType()).Length > 1)
        {
            Destroy(gameObject);
        }
        input = GameObject.Find("InputField");
        input2 = GameObject.Find("Cooplayer");   

    }

    void Start_Now()
    {
        instance = this;
        Quaternion quatTiltAngle = new Quaternion();
        quatTiltAngle.eulerAngles = new Vector3(-SensorAngle, 0.0f, 0.0f);
        kinectToWorld.SetTRS(new Vector3(0.0f, SensorHeight, 0.0f), quatTiltAngle, Vector3.one);

        UdpClient udpClient = new UdpClient();
        try
        {
            IPAddress.Parse(ip_adress);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("Napačen naslov!" + e);
            input.SetActive(true);
            return;
        }
        IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip_adress), 11000);
        udpClient.Connect(ep);
        udpClient.Send(new byte[] { 1 }, 1);

        AsyncCallback callback = null;
        callback = ar =>
        {
            newIncomingEndPoint = ep;
            data = udpClient.EndReceive(ar, ref newIncomingEndPoint);
            udpClient.BeginReceive(callback, null);
            String json = Encoding.ASCII.GetString(data, 0, data.Length);
            if (json == String.Empty)
            {
                UnityEngine.Debug.Log("client koneccc");
                reset = true;
                dvaObjekta[read] = null;
            }
            else
            {
                SendForward(data);
                dvaObjekta[write] = JsonUtility.FromJson<humanBody>(json);
                temp = read;
                read = write;
                write = temp;
            }
        };
        udpClient.BeginReceive(callback, null);        
    }

    void SendForward(byte[] data) {
        try
        {          
            udpClient1.Send(data, data.Length);         
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e.ToString() + "JUHUUU");
        }
    }
    
    void OnApplicationQuit()
    {
        if(ip_adress != "")
        {
            UdpClient udpClient = new UdpClient();
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip_adress), 11000);
            udpClient.Connect(ep);
            udpClient.Send(new byte[] { 1 }, 1);
        }
        //UnityEngine.Debug.Log("Application ending after " + Time.time + " seconds");
    }   

    public Quaternion GetJointOrientation(int joint)
    {
        if (dvaObjekta[read] == null)
            return Quaternion.identity;
         switch (joint)
         {
             case 0:
                return dvaObjekta[read].HipCenter;
             case 1:
                return dvaObjekta[read].Spine;
            case 2:
                return dvaObjekta[read].ShoulderCenter;
            case 3:
                return dvaObjekta[read].Head;
            case 4:
                return dvaObjekta[read].ShoulderLeft;
            case 5:
                return dvaObjekta[read].ElbowLeft;
            case 6:
                return dvaObjekta[read].WristLeft;
            case 7:
                return dvaObjekta[read].HandLeft;
            case 8:
                return dvaObjekta[read].ShoulderRight;
            case 9:
                return dvaObjekta[read].ElbowRight;
            case 10:
                return dvaObjekta[read].WristRight;
            case 11:
                return dvaObjekta[read].HandRight;
            case 12:
                return dvaObjekta[read].HipLeft;
            case 13:
                return dvaObjekta[read].KneeLeft;
            case 14:
                return dvaObjekta[read].AnkleLeft;
            case 15:
                return dvaObjekta[read].FootLeft;
            case 16:
                return dvaObjekta[read].HipRight;
            case 17:
                return dvaObjekta[read].KneeRight;
            case 18:
                return dvaObjekta[read].AnkleRight;
            case 19:
                return dvaObjekta[read].FootRight;
            default:
                 return Quaternion.identity;
         }  
    }

    public Vector3 GetUserPosition()
    {
        if (dvaObjekta[read] != null)
                return kinectToWorld.MultiplyPoint3x4((dvaObjekta[read].pos));
        return Vector3.zero;
    }

    public bool IsJointTracked()
    {
        if (dvaObjekta[read] != null)
            //if (dvaObjekta[read].pos != null)
                return true;

        return false;
    }
    
    public enum SkeletonPositionIndex : int
    {
        HipCenter = 0,
        Spine = 1,
        ShoulderCenter = 2,
        Head = 3,
        ShoulderLeft = 4,
        ElbowLeft = 5,
        WristLeft = 6,
        HandLeft = 7,
        ShoulderRight = 8,
        ElbowRight = 9,
        WristRight = 10,
        HandRight = 11,
        HipLeft = 12,
        KneeLeft = 13,
        AnkleLeft = 14,
        FootLeft = 15,
        HipRight = 16,
        KneeRight = 17,
        AnkleRight = 18,
        FootRight = 19
    }
}

public class humanBody
{
    public Vector3 pos;
    public Quaternion HipCenter;
    public Quaternion Spine;
    public Quaternion ShoulderCenter;
    public Quaternion Head;
    public Quaternion ShoulderLeft;
    public Quaternion ElbowLeft;
    public Quaternion WristLeft;
    public Quaternion HandLeft;
    public Quaternion ShoulderRight;
    public Quaternion ElbowRight;
    public Quaternion WristRight;
    public Quaternion HandRight;
    public Quaternion HipLeft;
    public Quaternion KneeLeft;
    public Quaternion AnkleLeft;
    public Quaternion FootLeft;
    public Quaternion HipRight;
    public Quaternion KneeRight;
    public Quaternion AnkleRight;
    public Quaternion FootRight;

    public humanBody()
    {
    }
}
