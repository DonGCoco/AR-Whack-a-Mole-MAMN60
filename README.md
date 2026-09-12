# AR Whack-a-Mole — MAMN60

A small AR Foundation project for the MAMN60 introductory AR assignment.

## Concept

The user scans a horizontal surface such as a table. When a valid plane is found, a placement reticle appears. Tapping the table places a compact Whack-a-Mole game board in AR. Moles pop up at random positions and can be hit by tapping them.

## Target stack

- Unity 2022.3 LTS
- AR Foundation 5.0.6
- Apple ARKit XR Plugin 5.0.6
- iOS / iPhone

Unity documents AR Foundation 5.0.6 and ARKit XR Plugin 5.0.6 as released packages for Unity 2022.3. Keep the AR Foundation and provider plug-in versions matched.

## Quick start

1. Clone this repository and open it with Unity 2022.3 LTS.
2. Wait for Package Manager to finish resolving packages.
3. In Unity, run **MAMN60 > Setup AR Whack-a-Mole Scene**.
4. Run **MAMN60 > Configure iOS + ARKit**.
5. Open **File > Build Settings**, choose **iOS**, and click **Switch Platform** if iOS is not already active.
6. Confirm **Project Settings > XR Plug-in Management > iOS > ARKit** is enabled.
7. Build the scene to Xcode, choose your signing team, then run it on an ARKit-capable iPhone.

## Current MVP

- Horizontal plane detection
- Screen-space placement reticle
- Tap-to-place AR game board
- Five procedural mole holes
- Random mole spawning
- Tap-to-hit interaction
- Score counter
- Reset button

No external 3D assets are required for the first version; the board and moles are generated from Unity primitives so the AR interaction can be validated first.

## Project structure

```text
Assets/
├── Editor/
│   └── MAMN60Setup.cs
└── Scripts/
    ├── ARPlacementController.cs
    ├── MoleTarget.cs
    └── WhackAMoleGame.cs
```

After the MVP is stable on-device, the procedural objects can be replaced with polished models and animation without changing the core AR flow.
