# Unity-XR-ArticulationBody-Hand

Experimental project implementing ArticulationBody-based physical hands in meta-quest's unity-sdk.

<video src="https://player.vimeo.com/video/1055515340?h=7d1f6248dd&amp;badge=0&amp;autopause=0&amp;player_id=0&amp;app_id=58479" frameborder="0" allow="autoplay; fullscreen; picture-in-picture; clipboard-write; encrypted-media" style="position:absolute;top:0;left:0;width:100%;height:100%;" title="com.TLab.XR.ArticulationHand-20250211-183635-0"></video>

## Backgournd

Previously, I used ```ConfiguredJoint``` to implement physical hand but it's not stable. Rcently, I found this physics hand demo made by ultralerp. It uses ```ArticulationBody``` for physics hand implementation and it seems stable. I want to use it on Meta Quest so try replacing current physics component from ```ConfiguredJoint``` to ```ArticulationBody```.

## Requirements
- Unity 2021.3.37f1
- [meta-xr-all-in-one-sdk](https://developers.meta.com/horizon/downloads/package/meta-xr-sdk-all-in-one-upm/)

## Get Started
### Install
Clone this repository with the following command.

```bat
git clone https://github.com/TLabAltoh/Unity-XR-ArticulationBody-Hand
cd Unity-XR-ArticulationBody-Hand
```

### Project Settings
#### Common
- Player
	- Other Settings
    	- Auto Graphics API: false
    	- Graphics APIs: OpenGLES3 only
		- Scripting Backend: IL2CPP
		- Target Architectures: ARM64

- Physics
	- Friction Type One Directional Friction Type
	- Solver Type: Temporal Gauss Seidel

- Time
	- Fixed Timestep: 0.01
	- Maximum Allowed Timestep: 0.1

#### Run on Oculus Link
Select ```Multi View``` in ```ProjectSettings/XR Plug-in Management/Oculus/Stereo Rendering Mode``` and select ```Vulkan``` for Symmetric Projection.

#### Run on Oculus Quest as apk
Select ```Multi Pass``` in ```ProjectSettings/XR Plug-in Management/Oculus/Stereo Rendering Mode``` for UI canvas and hand tracking rendering.

### Sample Scene
```Assets/TLab/XR-ArticulationBody-Hand/Scenes/SampleScene.unity```

## Issues
- Is there a difference in collision behaviour for ArticulationBody between Unity versions 2021 and 2022?
  - In Unity version 2021, by increasing the weight of the target object's Rigidbody, it was possible to avoid the behaviour of the ArticulationBody's collider being buried inside the target object. However, when upgrading to Unity 2022, I noticed that some collider penetration occurred in cases where the ArticulationBody's collider did not penetrate in Unity 2021.
- I noticed that the ArticulationBody's joint would disintegrat if it collided with something at high speed and penetrated its collider.