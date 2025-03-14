using UnityEngine;
using System.Net.Sockets;
using System;
using System.IO;
using System.Threading;
using UnityEngine.UI;

public class VideoReceiver : MonoBehaviour
{
    public RawImage display;
    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;
    private Texture2D receivedTexture;

    void Start()
    {
        Thread serverThread = new Thread(StartServer);
        serverThread.Start();
    }

    void StartServer()
    {
        server = new TcpListener(System.Net.IPAddress.Any, 5005);
        server.Start();
        Debug.Log("Waiting for connection...");
        client = server.AcceptTcpClient();
        stream = client.GetStream();
        Debug.Log("Connected to HoloLens!");

        while (true)
        {
            try
            {
                // Read frame size
                byte[] sizeBytes = new byte[4];
                stream.Read(sizeBytes, 0, sizeBytes.Length);
                int frameSize = BitConverter.ToInt32(sizeBytes, 0);

                // Read frame data
                byte[] frameData = new byte[frameSize];
                int bytesRead = 0;
                while (bytesRead < frameSize)
                {
                    bytesRead += stream.Read(frameData, bytesRead, frameSize - bytesRead);
                }

                ProcessFrame(frameData);
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving frame: " + e.Message);
                break;
            }
        }
    }

    void ProcessFrame(byte[] data)
    {
        if (receivedTexture == null)
        {
            receivedTexture = new Texture2D(640, 480, TextureFormat.RGB24, false);
        }

        receivedTexture.LoadImage(data);
        receivedTexture.Apply();

        // Update UI on main thread
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            display.texture = receivedTexture;
        });
    }
}