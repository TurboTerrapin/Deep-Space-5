/*
    VoiceChat.cs
    - Handles Voice Chat (VOIP) functionality was mostly copied from user 
        schouffy here: https://github.com/Facepunch/Facepunch.Steamworks/issues/261
        with small edits to fit our use case
    Contributor(s): John Aylward
    Last Updated: 10/2/2026
*/


using Steamworks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class VoiceChat : NetworkBehaviour
{

    public static VoiceChat Instance { get; protected set; }

    public AudioSource AudioSource;
    public bool recording = false;
    private bool voiceActivated = false;
    private KeyCode pushToTalkKey;

    private MemoryStream uncompressedStream;
    private MemoryStream voiceStream;
    private MemoryStream compressedStream;

    private float[] audioclipBuffer;
    private int audioclipBufferSize;
    private int audioPlayerPosition;

    private int playbackBuffer;
    private int dataPosition;
    private int dataReceived;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
    }

    void Start()
    {
        voiceStream = new MemoryStream();
        uncompressedStream = new MemoryStream();
        compressedStream = new MemoryStream();

        int optimalRate = (int)SteamUser.OptimalSampleRate;

        audioclipBufferSize = optimalRate * 5;
        audioclipBuffer = new float[audioclipBufferSize];

        // Here optimalRate * 2 seems to be what fixes the playback issues
        AudioSource.clip = AudioClip.Create("VoiceData", (int)optimalRate * 2, 1, (int)optimalRate, true, OnAudioRead, null);
        AudioSource.loop = true;
        AudioSource.Play();

        // TODO for this example it always records but you should push to talk or have a setting
        SteamUser.VoiceRecord = true;
        recording = true;
    }

    private void OnAudioRead(float[] data)
    {
        for (int i = 0; i < data.Length; ++i)
        {
            // start with silence
            data[i] = 0;

            // do I  have anything to play?
            if (playbackBuffer > 0)
            {
                // current data position playing
                audioPlayerPosition = (audioPlayerPosition + 1) % audioclipBufferSize;
                data[i] = audioclipBuffer[audioPlayerPosition];
                playbackBuffer--;
            }
        }
    }

    internal void Update()
    {
        SteamUser.VoiceRecord = true;
        recording = SteamUser.VoiceRecord;
        //Recording from mic and sending to server
        if (IsClient)
        {
            if ((SteamUser.HasVoiceData && voiceActivated) || (!voiceActivated && Input.GetKey(pushToTalkKey)))
            {
                int compressedRead = SteamUser.ReadVoiceData(voiceStream);
                voiceStream.Position = 0;
                var bytes = new System.ArraySegment<byte>(voiceStream.GetBuffer(), 0, compressedRead);
                SendVoiceServerRpc(bytes.Array, compressedRead, NetworkManager.Singleton.LocalClientId);
            }
        }

        while (true)
        {
            if (_pendingBuffers.Count == 0 || _pendingBuffers.ElementAt(0).ReadTime > Time.time)
                break;

            var pending = _pendingBuffers.Dequeue();
            WriteToClip(pending.Buffer, pending.Size);
        }
    }

    public void WriteToClip(byte[] uncompressed, int iSize)
    {
        for (int i = 0; i < iSize; i += 2)
        {
            // insert converted float to buffer
            audioclipBuffer[dataReceived] = (short)(uncompressed[i] | uncompressed[i + 1] << 8) / 32767.0f;

            // buffer loop
            dataReceived = (dataReceived + 1) % audioclipBufferSize;

            playbackBuffer++;
        }
    }

    internal void Append(float time, byte[] buffer, int size)
    {
        _pendingBuffers.Enqueue(new PendingBuffer { ReadTime = time + .5f /* latency buffer */, Buffer = buffer, Size = size });
    }

    //[Rpc(SendTo.Me)]
    public void PlayVoice(byte[] compressed, int length)
    {
        compressedStream.Position = 0;
        compressedStream.Write(compressed, 0, length);
        compressedStream.Position = 0;

        uncompressedStream.Position = 0;
        int uncompressedWritten = SteamUser.DecompressVoice(compressedStream, length, uncompressedStream);

        // ToArray to copy it, because otherwise all the appends will share the same underlying array. This can maybe be pooled to avoid too many allocations
        byte[] outputBuffer = uncompressedStream.GetBuffer().ToArray();
        Append(Time.time, outputBuffer, uncompressedWritten);
    }

    [Rpc(SendTo.NotMe)]
    public void SendVoiceServerRpc(byte[] compressed, int length, ulong clientId)
    {
        // TODO here you should only call clientrpc on clients other than clientId
        PlayVoice(compressed, length);
    }

    public void setVoiceActivated(bool value)
    {
        voiceActivated = value;
    }
    public void setPushToTalkKey(KeyCode key)
    {
        pushToTalkKey = key;
    }


    public override void OnDestroy()
    {
        SteamUser.VoiceRecord = false;
        base.OnDestroy();
    }

    private Queue<PendingBuffer> _pendingBuffers = new();

    private class PendingBuffer
    {
        public float ReadTime { get; set; }
        public byte[] Buffer { get; set; }
        public int Size { get; set; }
    }


}

