---
layout: post
title:  ECS 193 Senior Design Proposal
category: report 
description: ECS 193 Senior Design Proposal AR/VR Game
---
#### **Instructor:** Professor Xin Liu 
#### **Team Members So Far:** Chen Chen (SID: 998820114), Yiru Sun (SID: 999235229) 
#### **Potential Sponsor:** Professor Xiaoguang “Leo” Liu
### **Proposal**: 
The exponential growth in cloud computing power and data storage capacity has rendered moreextensive use of AR technology possible, and drones combined with VR devices are great platforms toimplement AR contents. Our team set out to integrate VR headsets such as Oculus Rift with drones viaLTE/4G network in order to create an immersive viewing experience for drone driver. We then try tobring in AR functionalities so that user could interact with real scenes with virtual drawing andstructures. This capability would come in really handy in many situations such as large scale eventplanning, architecture/civil engineering and military applications. 

This project would be divided into two major parts. The first part is to integrate a VR headset with aflying drone. We could use streaming services such as AWS CloudFront to stream real time compressedvideo captured with a stereo camera on the drone. The two streams of video could be adjusted anddisplayed in the VR headset with APIs such as those offered by the Unity Engine. The camera ismounted on a movable mechanical structure like Gimbal so that it moves together with the VR headsetin a stable manner. 

The second part of our project is to interact with the VR scene we created in the previous part usingdevices such as Oculus touch. We could do things such as measuring the distance between theobserving spot (aka the drone) and a point in the scene. We could also perform more complicated taskssuch as creating new structures or putting virtual objects in the virtual space. The key here is to obtainaccurate localization of the virtual object and the precise movement of the drone and the VR headset,so that the new AR information would not be lost once the user changes the drone position or VR viewing angle. 

This proposal will be cced to Professor Leo Liu who is kind enough to consider accommodating ourteam. We believe that his expertise in drones and RF in general will be great help for us along the way.Demo:The user could measure the distance between the flying drone and a point in his or her range of view. Itwould be great if a virtual object could be placed at a selected location and the information would beobtained even after the user turned away and turn back, or after the drone’s position changed.

#### **Technologies and APIs:**
- Oculus Rift
- Oculus Touch
- Unity
- Drones with a movable camera
- AWS