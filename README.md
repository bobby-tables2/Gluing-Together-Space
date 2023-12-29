# Gluing Together Space
Portals in Unity (primarily for non-character controlled physics objects)

![A gif of gameplay footage](README/gameplay.gif)

Controls (for the Labyrinth example):
- WASD/arrow keys tilts the board.
- Drag with mouse to rotate camera around the board.
- Spacebar spawns a ball.

## How does it work?
### Teleportation Algorithm
- All GameObjects with a RigidBody component are tracked by the game. 
- When a GameObject reaches portal 1, a body double of the GameObject is created at portal 2.
> (For the sake of simplicity, portals only track objects with the RigidBody component, and GameObjects are assumed to not have many components)
- The body double has the same position relative to portal 2, as the GameObject's position relative to portal 1.
```C#
bodydouble.transform.position = plane2.transform.TransformPoint(plane1.transform.InverseTransformPoint(obj.transform.position));
```
- When the GameObject leaves portal 1, it takes the body double's position and the body double is destroyed.

### Portal textures
- Each portal has a portal camera.
- Move and rotate portal 1's camera relative to the main camera.
- Capture portal 1's camera's view onto a RenderTexture.
- Place RenderTexture onto portal 2.

## Usage
### GluingTogetherSpace.cs
Add to any GameObject. On adding, it generates a pair of portals as children of the GameObject. Add the Main Camera to the Main Camera property of the script.

The portals can be moved, rotated and scaled directly in the editor. You can use the Unlit and FlipCulling (changes culling mode to front, is also unlit) materials on the portals.

You can make the portals not do anything when an object enters from one side. To do this, deselect the Enable Front/Back Side Plane 1/2 option in the inspector.

### BoardTilterScript.cs
Add to flat GameObject. Allows it to tilt in 4 directions with the arrow keys. Add the Main Camera to the Main Camera property of the script.

### CameraRotateAboutPivotScript.cs
Add to the Main Camera. Allows the Main Camera to orbit another GameObject with mouse dragging. Add the desired GameObject to focus on to the Target property of the script.

### BallGenScript.cs
Add to any GameObject. Allows it to spawn a desired prefab at a specific coordinate with the press of the space button. Add the desired prefab to the Prefab_spawned property of the script, and set the position and spawning limit (number of prefabs that can be spawned in total) in the inspector.