using UnityEngine;
using System.Net.Sockets;
using System;
using System.IO;

public class HoloLensVideoSender : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private WebCamTexture webcam;
    private Texture2D videoTexture;

    void Start()
    {
        // Start camera
        webcam = new WebCamTexture();
        webcam.Play();
        videoTexture = new Texture2D(webcam.width, webcam.height, TextureFormat.RGB24, false);

        // Connect to PC & Tablet (Change IP to match your network)
        client = new TcpClient("192.168.1.100", 5005); /////////// PC
        stream = client.GetStream();

        InvokeRepeating(nameof(CaptureAndSendFrame), 0, 0.033f); // ~30 FPS
    }

    void CaptureAndSendFrame()
    {
        if (webcam.didUpdateThisFrame)
        {
            videoTexture.SetPixels(webcam.GetPixels());
            videoTexture.Apply();

            byte[] frameData = videoTexture.EncodeToJPG();
            SendFrame(frameData);
        }
    }

    void SendFrame(byte[] frameData)
    {
        try
        {
            if (stream != null && client.Connected)
            {
                byte[] sizeBytes = BitConverter.GetBytes(frameData.Length);
                stream.Write(sizeBytes, 0, sizeBytes.Length);
                stream.Write(frameData, 0, frameData.Length);
                stream.Flush();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending frame: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}