using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class OpenIGTLinkConnect : MonoBehaviour
{
    public string ipString = "192.168.1.100"; ///////// Change to the tablet's IP
    public int port = 18944;
    private Socket socket;
    private IPEndPoint remoteEP;
    private bool connectionStarted = false;
    private WebCamTexture webcam;
    private Texture2D videoTexture;

    void Start()
    {
        // Start OpenIGTLink connection
        ConnectToServer();

        // Start HoloLens Camera
        webcam = new WebCamTexture();
        webcam.Play();
        videoTexture = new Texture2D(webcam.width, webcam.height, TextureFormat.RGB24, false);

        InvokeRepeating(nameof(CaptureAndSendFrame), 0, 0.033f); // Send ~30 FPS
    }

    void ConnectToServer()
    {
        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            remoteEP = new IPEndPoint(System.Net.IPAddress.Parse(ipString), port);
            socket.Connect(remoteEP);
            connectionStarted = true;
            Debug.Log("Connected to OpenIGTLink server.");
        }
        catch (Exception e)
        {
            Debug.LogError("OpenIGTLink Connection Error: " + e.Message);
        }
    }

    void CaptureAndSendFrame()
    {
        if (webcam.didUpdateThisFrame)
        {
            videoTexture.SetPixels(webcam.GetPixels());
            videoTexture.Apply();

            byte[] frameData = videoTexture.EncodeToJPG(); // Convert to JPG
            SendVideoFrame(frameData);
        }
    }

    void SendVideoFrame(byte[] frameData)
    {
        if (!connectionStarted) return;

        try
        {
            byte[] header = Encoding.ASCII.GetBytes("OPENIGTLINK_IMAGE");
            byte[] sizeBytes = BitConverter.GetBytes(frameData.Length);
            byte[] packet = new byte[header.Length + sizeBytes.Length + frameData.Length];

            Buffer.BlockCopy(header, 0, packet, 0, header.Length);
            Buffer.BlockCopy(sizeBytes, 0, packet, header.Length, sizeBytes.Length);
            Buffer.BlockCopy(frameData, 0, packet, header.Length + sizeBytes.Length, frameData.Length);

            socket.Send(packet);
            Debug.Log($"Sent video frame ({frameData.Length} bytes)");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to send video frame: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (socket != null)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}
