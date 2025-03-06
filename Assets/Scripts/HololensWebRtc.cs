/*using UnityEngine;
using Microsoft.MixedReality.WebRTC;
using System.Threading.Tasks;

public class HololensWebRtc : MonoBehaviour
{
    public PeerConnection peerConnection;
    public VideoRenderer videoRenderer;

    async void Start()
    {
        // Initialize WebRTC
        await peerConnection.InitializeAsync();

        // Start video capture from the HoloLens camera
        var videoTrack = await peerConnection.AddLocalVideoTrackAsync();
        videoRenderer.Source = videoTrack;
        
        // Start WebRTC connection
        await peerConnection.StartConnection();
    }
}*/