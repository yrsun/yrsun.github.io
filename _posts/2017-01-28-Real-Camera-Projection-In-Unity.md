---
layout: post
title:  Real Camera Projection In Unity
category: technical 
description: Combine virtual object and real camera
---
# Introduction
<iframe src="https://drive.google.com/file/d/1wFcmidybzKADvE_LcUWuaLsDIkmkp0pudQ/preview" width="640" height="480"></iframe>

We have recently prototyped an idea that combining virtual object in the Unity3D and images from real camera. As you can see from the video, we are tilting **webcam** with pitch axis, and **Camera** object in the Unity3D is following the real camera motion. At the same time, **webcam** streams the video into Unity3D, combined with the mystery virtual object. Looks like that mystery object is actually on his body. With this idea, we can add virtual objects as we want in to Unity3D and mix them with real scenes, creating mixture reality.

# Components
Following parts were used in this prototype: 
1. Unity3D
2. Webcam
3. Arduino + IMU

#### Unity3D
Unity3D is used to create the whole world, including virtual objects and image background that will be used to project real world scenes.
A rawImage object is placed in the Unity3D world that holds the texture/images captured by webcam.

#### Webcam
Webcam is used to capture the real scenes. We used **WebCamDevice** class to control/read camera. Here is source code that capture images from camera and project images to the rawImage plane mentioned above. 
```cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ui_update : MonoBehaviour {
	public RawImage rawimage;
    public int N_camera = 1;
	// Use this for initialization
	void Start () {
		WebCamDevice[] devices = WebCamTexture.devices;
		WebCamTexture webcamTexture = new WebCamTexture ();
        Debug.Log("Number of Camera: " + devices.Length);
        if (devices.Length != 0)
        {
            webcamTexture.deviceName = devices[N_camera].name;
            rawimage.texture = webcamTexture;
            rawimage.material.mainTexture = webcamTexture;
            webcamTexture.Play();
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
```

#### Arduino/BNO055
IMU is neccessary to capture the motion of camera. BOSCH BNO055 is fusion sensor that directly provide quaternion/euler information which save a lot of time. Since BNO055 only has I2C interface, we used an Arduino here to bridge BNO055 and computer. 
Following Unity3D codes were used to read value from Arduion(BNO055) and update **camera** object in the Unity3D.
```cs
public class camera_control : MonoBehaviour {



	SerialPort stream=new SerialPort("/dev/tty.usbmodem1411",115200);
	float[] quat=new float[4];
	// Use this for initialization
	void Start () {
		while (!stream.IsOpen) {
			stream.Open();
		}

	}
	
	// Update is called once per frame
	void Update () {
		if (stream.IsOpen) {
			string newline;
			newline = stream.ReadLine ();
			//Debug.Log (newline);
			string[] values = newline.Split (' ');

			for (int i=0;i<4;i++) {
				quat[i] = float.Parse(values[i]);

			}
			Debug.Log (quat.ToString());
			//transform.rotation = Quaternion.identity;
			//transform.rotation = quat;
			transform.Rotate(quat[2]-transform.eulerAngles.x,0,0);
		}

	}
}
```
