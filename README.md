# AR Whack-a-Mole — MAMN60

A small AR Foundation project for the MAMN60 introductory AR assignment.

## Concept

The user scans a horizontal surface such as a table. When a valid plane is found, a placement reticle appears. Tapping the table places a compact Whack-a-Mole game board in AR. Moles pop up at random positions and can be hit by tapping them.

## Target stack

- Unity 6.3 LTS (6000.3.10f1)
- AR Foundation 6.3.1
- Apple ARKit XR Plugin 6.3.1
- XR Plug-in Management 4.5.3
- iOS / iPhone

AR Foundation and the ARKit provider plug-in are kept on matching versions.

## Quick start

1. Clone this repository and open it with **Unity 6.3 LTS (6000.3.10f1)**.
2. Wait for Package Manager to finish resolving packages.
3. In Unity, run **MAMN60 > Setup AR Whack-a-Mole Scene**.
4. Run **MAMN60 > Configure iOS + ARKit**.
5. Open **File > Build Profiles** (or Build Settings, depending on the editor layout), select **iOS**, and switch the active platform if needed.
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
