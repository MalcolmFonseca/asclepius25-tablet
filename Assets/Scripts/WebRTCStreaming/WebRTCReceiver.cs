using UnityEngine;
using Microsoft.MixedReality.WebRTC;
using UnityEngine.UI;

public class WebRTCReceiver : MonoBehaviour
{
    public RawImage display;
    public PeerConnection peerConnection;

    async void Start()
    {
        await peerConnection.InitializeAsync();
        peerConnection.OnTrackAdded += (PeerConnection pc, RemoteVideoTrack track) =>
        {
            track.OnVideoFrameReady += (frame) =>
            {
                Texture2D tex = new Texture2D(frame.Width, frame.Height, TextureFormat.RGB24, false);
                tex.LoadRawTextureData(frame.Data);
                tex.Apply();
                display.texture = tex;
            };
        };
    }
}