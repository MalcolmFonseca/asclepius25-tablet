using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Windows.WebCam;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;

public class VideoManagerHololens : MonoBehaviour
{
    private WebCamTexture hololensCam;
    private Texture2D videoTexture;

    GameObject videoCanvas;

    float refreshRate = .034f; //in seconds (.034s ~ 30fps)
    float timer;

    private string serverIP = "127.0.0.1";
    private int serverPort = 8080;
    private TcpClient client;
    private NetworkStream stream;
    private Thread clientReceiveThread;
    private void Start()
    {
        ConnectToServer();
        hololensCam = new WebCamTexture();
        hololensCam.Play();
        videoTexture = new Texture2D(hololensCam.width, hololensCam.height, TextureFormat.RGB24, false);
    }

    private void Update()
    {

        if (hololensCam.didUpdateThisFrame)
        {
            timer += Time.deltaTime;
            if(timer > refreshRate)
            {
                timer = timer - refreshRate;
                videoTexture.SetPixels(hololensCam.GetPixels());
                videoTexture.Apply();

                byte[] frameData = videoTexture.EncodeToJPG();
                SendMessageToServer(frameData);
            }
        }
    }

    private void ConnectToServer()
    {
        try
        {
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            Debug.Log("Connected to server.");
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e.ToString());
        }
    }

    public void SendMessageToServer(byte[] data)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("Client not connected to server.");
            return;
        }

        stream.Write(data, 0, data.Length);
    }

    void OnApplicationQuit()
    {
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
        if (clientReceiveThread != null)
            clientReceiveThread.Abort();
    }
}
