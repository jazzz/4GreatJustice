using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

public class DepthToTexture : MonoBehaviour {
    public DeviceOrEmulator device;

    Thread thread;

    Color32[] buffer;

    public Texture2D texture;

    Kinect.KinectInterface kinect;

    int size;
    void Start() {
        
        kinect = device.getKinect();
        var resolution = kinect.DepthResolution;

        var width = resolution == Kinect.NuiImageResolution.resolution320x240 ? 320 : 640;
        var height = resolution == Kinect.NuiImageResolution.resolution320x240 ? 240 : 480;

        texture = new Texture2D(width, height, TextureFormat.ARGB32, false, true);

        size = width * height;
        buffer = new Color32[size];

        thread = new Thread(() => {
            while (true) {
                if (kinect.pollDepth()) {
                    var depths = kinect.getDepth();

                    Color32[] writeTo = new Color32[size];
                    for (int i = 0; i < size; ++i) {
                        var s = depths[i];
                        writeTo[i].r = (byte)((s >> 0) & 0xffff);
                        writeTo[i].g = (byte)((s >> 8) & 0xffff);
                    }

                    buffer = writeTo;
                }
                Thread.Sleep(1);
            }
        });
        thread.Start();

        renderer.material.mainTexture = texture;
    }

    void Update() {
        //grab latest buffer
        var b = Interlocked.Exchange(ref buffer, null);
        if (b == null) return;
        texture.SetPixels32(b);
        texture.Apply(false);
    }

    void OnDestroy() {
        if (thread != null) thread.Abort();
    }
}

