# Unity-XR-ArticulationBody-Hand

Previously, I used ```ConfiguredJoint``` to implement physical hand but it's not stable. Rcently, I found this physics hand demo made by ultralerp. It uses ```ArticulationBody``` for physics hand implementation and it seems stable. I want to use it on Meta Quest so try replacing current physics component from ```ConfiguredJoint``` to ```ArticulationBody```.

## Requirements
- Unity 2022.3.19f1
- [meta-xr-all-in-one-sdk](https://developers.meta.com/horizon/downloads/package/meta-xr-sdk-all-in-one-upm/)

## Get Started
### Project Settings
- Player
	- Other Settings
		- Color Space: Linear
		- Auto Graphics API: false
		- Graphics APIs: OpenGLES3
		- Scripting Backend: IL2CPP
		- Target Architectures: ARM64

- Physics
	- Bounce Threshold: 0.25
	- Default Max Depenetration Velocity: 0.1
	- Default Solver Iterations: 16
	- Default Solver Velocity Iterations: 32

	- Queries Hit Backfaces: true
	- Queries Hit Triggers: true
	- Enable Adaptive Force: true

	- Contacts Generation: Persistent Contact Manifold
	- Simulation Mode: Fixed Update

	- Friction Type One Directional Friction Type

	- Solver Type: Temporal Gauss Seidel

- Time
	- Fixed Timestep: 0.01
	- Maximum Allowed Timestep: 0.1

### Sample Scene
- Assets/TLab/XR-ArticulationBody-Hand/Scenes/SampleScene.unity
