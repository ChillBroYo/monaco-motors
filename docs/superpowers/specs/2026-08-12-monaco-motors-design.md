# Monaco Motors — Game Design Specification

**Date:** 2026-08-12  
**Status:** Approved  
**Version:** 1.0

## Overview

Monaco Motors is a high-detail simcade racing game built with Unity 6, targeting mobile platforms (iOS/Android) with PC-ready architecture. The game features AI-generated car models, Forza Horizon-style driving mechanics, and a free-to-play model with cosmetic monetization.

## Core Decisions

| Aspect | Decision |
|--------|----------|
| Platform | Mobile-first (iOS/Android), PC-ready architecture |
| Driving Style | Simcade (realistic feel, accessible gameplay) |
| Cars | 5-8 AI-generated models (Meshy/Tripo3D + Blender) |
| Tracks | 4 environments (city, coastal, desert, mountain) |
| Game Modes | Career/Championship with progression |
| Monetization | F2P with cosmetic unlocks |
| Development Approach | Vertical Slice First |

## Technical Architecture

### Tech Stack

- **Unity 6 LTS** (6000.x)
- **Universal Render Pipeline (URP)** — Mobile-optimized graphics
- **New Input System** — Touch + controller support
- **Addressables** — On-demand asset loading
- **TextMeshPro** — UI text rendering

### Project Structure

```
monaco-motors/
├── Assets/
│   ├── Scripts/
│   │   ├── Vehicle/          # Physics, input, controls
│   │   ├── Track/            # Track logic, checkpoints, AI paths
│   │   ├── Game/             # Race manager, career, progression
│   │   ├── UI/               # Menus, HUD, garage
│   │   └── Core/             # Save system, audio, utilities
│   ├── Models/
│   │   ├── Vehicles/         # Car FBX files + materials
│   │   └── Tracks/           # Environment assets
│   ├── Prefabs/
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── Garage.unity
│   │   ├── Race.unity
│   │   └── Loading.unity
│   ├── Materials/
│   ├── Textures/
│   └── Audio/
├── Packages/
└── ProjectSettings/
```

---

## Vehicle System

### Physics Model (Simcade)

- Custom `VehicleController` using Unity's `Rigidbody`
- Raycast-based suspension for each wheel (simpler than WheelCollider)
- Arcade-assisted grip with realistic weight transfer
- Drift mechanics: handbrake triggers controlled oversteer

### Vehicle Stats Schema

```csharp
[System.Serializable]
public class VehicleStats
{
    public float topSpeed;        // Max km/h
    public float acceleration;    // 0-100 time feel
    public float handling;        // Turn responsiveness
    public float driftFactor;     // Slide tendency
    public float braking;         // Stopping power
    public float mass;            // Weight transfer, collisions
}
```

### Input System

**Touch Controls (Mobile):**
- Steering: Tilt OR left/right screen buttons (player choice)
- Throttle: Auto-accelerate with brake button, OR manual gas/brake
- Drift/Handbrake: Dedicated button
- Nitro: Tap to activate (earned through drifts/drafting)

**Controller Support:**
- Left stick: Steering
- RT/R2: Throttle
- LT/L2: Brake
- A/X: Nitro
- B/Circle: Handbrake

### Camera System

- Chase cam (default): Follows behind, smooth lerp
- Bumper cam: Hood view for immersion
- Dynamic FOV: Shifts with speed for velocity sensation

---

## Track System

### Architecture

- Each track is a separate Addressable scene
- Spline-based road definition for AI pathing, minimap, checkpoints
- Modular environment pieces

### Track Components

```
Track/
├── RoadSpline          # Bezier path defining the road
├── CheckpointSystem    # Lap counting, position tracking
├── SpawnPoints         # Start grid positions
├── AIWaypoints         # Racing line for AI opponents
├── TrackBoundaries     # Invisible walls, respawn triggers
└── Environment         # Visual scenery
```

### Initial Tracks

| Track | Setting | Characteristics |
|-------|---------|-----------------|
| Monaco Boulevard | City streets | Tight corners, luxury storefronts |
| Coastal Highway | Seaside cliffs | Sweeping curves, ocean views |
| Desert Dunes | Sand/canyon | Wide roads, heat shimmer effects |
| Alpine Pass | Mountain switchbacks | Elevation changes, tunnels |

### Race Configuration

- 3 laps default
- 6 AI opponents (rubber-band difficulty)
- Checkpoint respawn system
- Position tracking (1st - 6th place)

---

## Career Mode & Progression

### Structure

```
Career/
├── Leagues (Bronze → Silver → Gold → Platinum)
│   └── Series (3-4 per league)
│       └── Races (4-5 per series)
```

### Progression Flow

1. Start in Bronze League with starter car
2. Win races → earn Credits + XP
3. XP levels up profile (unlocks new leagues)
4. Credits buy cars and cosmetics
5. Complete Series → unlock next Series + bonus reward

### Rewards Table

| Action | Reward |
|--------|--------|
| Finish race | Credits (scaled by position) |
| Win race | Bonus credits + XP |
| Complete series | New car unlock OR big credit bonus |
| Drift/nitro streaks | Small credit bonuses |

### Save System

- Local save using JSON
- Tracks: owned cars, cosmetics, career progress, settings
- Architecture supports future cloud save integration

---

## Cars & Customization

### Car Roster

| Car | Class | Characteristics | Stats Focus |
|-----|-------|-----------------|-------------|
| Vento GT | Sports | Entry-level sports car | Balanced |
| Strada 500 | Muscle | American muscle style | Top speed, low handling |
| Futura RS | Supercar | Mid-engine exotic | Acceleration, handling |
| Classico 1965 | Classic | Vintage grand tourer | Drift-friendly |
| Tempest X | Hypercar | Ultimate machine | All maxed, hard to unlock |
| Urbano E | Electric | Modern EV sports | Instant torque |

### Customization Categories

```
Customization/
├── Paint
│   ├── Colors (solid, metallic, matte)
│   ├── Wraps/Liveries
│   └── Finish (gloss, satin, matte)
├── Wheels
│   ├── Rim styles (10+ designs)
│   └── Rim colors
├── Details
│   ├── Window tint
│   ├── Brake caliper color
│   └── Underglow (unlockable)
└── License plates
```

### Asset Pipeline

1. Generate base mesh in Meshy/Tripo3D
2. Import to Blender → clean topology, optimize
3. UV unwrap, create material zones
4. Export FBX with LODs
5. Set up Unity prefab with VehicleController

### Poly Count Targets (Mobile)

- LOD0 (close): 15-25k triangles
- LOD1 (medium): 8-12k triangles
- LOD2 (far): 3-5k triangles

---

## UI/UX Design

### Screen Flow

```
Splash → Main Menu → Garage ←→ Career → Race → Results
              ↓
          Settings
              ↓
           Shop
```

### Color Scheme (Mercedes AMG-inspired)

| Element | Color | Hex |
|---------|-------|-----|
| Primary | Deep black | #0F0F0F |
| Secondary | Anthracite grey | #2D2D2D |
| Accent | Petronas teal | #00D2BE |
| Chrome | Silver | #C6C6C6 |
| Text | White | #FFFFFF |

### Race HUD Layout

```
┌─────────────────────────────────────┐
│ [Position] 1st        LAP 2/3 [Map]│
│                                     │
│          (gameplay area)            │
│                                     │
│ [Speed]          [Nitro] [Brake]    │
│ 187 km/h         ████░░  [DRIFT]    │
└─────────────────────────────────────┘
```

### Touch Control Layout

- Left side: Steering (tilt or buttons)
- Right side: Brake, Drift/Handbrake
- Center bottom: Speed, nitro bar
- Top: Position, lap counter, minimap

---

## AI Opponents

### AI System Components

```
AIDriver/
├── PathFollower      # Follows racing line spline
├── SpeedController   # Brakes for corners, accelerates straights
├── RubberBand        # Adjusts speed based on player distance
├── AvoidanceSystem   # Steers around obstacles/cars
└── MistakeGenerator  # Occasional errors based on difficulty
```

### Configuration

- 6 AI opponents (7 total racers)
- Difficulty levels: Easy/Medium/Hard
- Random car selection from roster
- Generated names ("Marco V.", "Elena K.", etc.)

---

## Audio & Visual Polish

### Graphics (URP Mobile)

- Baked lighting for tracks
- Real-time shadows for cars only (1 cascade)
- Reflection probes for paint shine
- Post-processing: Bloom, vignette, motion blur (optional)
- Weather effects: Heat shimmer, rain droplets

### Audio Categories

- Engine: Per-car samples, RPM-mapped pitch
- Tires: Screech on drift, surface variations
- Ambient: Track-specific (crowds, waves, wind)
- Music: Electronic/synthwave
- UI: Subtle clicks, whooshes

### Juice/Feel

- Camera shake on collisions/nitro
- Speed lines at high velocity
- Particle trails: Tire smoke, sparks
- Nitro flame effect
- Screen flash on position change

---

## Vertical Slice Scope (Phase 1)

### Deliverables

| Component | Vertical Slice | Future |
|-----------|---------------|--------|
| Cars | 1 (Vento GT) | 5-8 |
| Tracks | 1 (Monaco Boulevard) | 4 |
| Track variants | 4 (day, night, reverse, rain) | Weather system |
| AI opponents | 4 | 6 |
| Career | Bronze League, 1 Series | 4 Leagues |
| Customization | Paint colors only | Full system |
| UI | Menu, Garage, HUD, Results | Shop, cloud save |
| Audio | Engine, tires, placeholder music | Full sound design |

### Success Criteria

1. ✅ Playable race from start to finish
2. ✅ Simcade physics that feels satisfying
3. ✅ 1 AI-generated car model
4. ✅ 1 track with 4 variants
5. ✅ Basic career progression
6. ✅ Touch controls on mobile
7. ✅ Mercedes-style UI theme
8. ✅ Local save system
9. ✅ Builds to iOS/Android

### Estimated Timeline

| Week | Focus |
|------|-------|
| 1-2 | Project setup, vehicle physics, controls |
| 3-4 | Track system, AI opponents |
| 5 | Career mode, progression, save system |
| 6 | UI/UX implementation |
| 7 | First car model (AI + Blender) |
| 8 | Polish, audio, mobile builds, testing |

---

## Future Considerations

- **Cloud Save:** Architecture supports easy integration (PlayFab, Unity Gaming Services, Firebase)
- **Multiplayer:** Real-time racing infrastructure (Phase 2+)
- **Additional Content:** More cars, tracks, leagues
- **Live Ops:** Seasonal events, limited-time challenges
