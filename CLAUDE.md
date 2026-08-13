# Generic Driving Test Game - Claude Code Instructions

This file provides context for Claude Code agents working on this project.

## Project Overview

Generic Driving Test Game is a **mobile racing game** built with Unity 6. Think Forza Horizon but for mobile — simcade physics, beautiful visuals, career progression.

**Current Phase:** Pre-development (architecture and tooling setup)

## Key Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Platform | Mobile-first (iOS/Android) | Primary target market |
| Physics | Simcade (raycast suspension) | Accessible yet satisfying |
| Models | AI-generated (Shap-E) | Faster iteration, unique designs |
| Monetization | F2P + cosmetics | Fair, non-pay-to-win |
| UI Theme | Mercedes AMG (dark + teal) | Premium feel |

## File Locations

**Design & Planning:**
- `docs/superpowers/specs/2026-08-12-monaco-motors-design.md` — Full game design spec

**Core Systems:**
- `Assets/Scripts/Vehicle/` — All vehicle-related code
- `Assets/Scripts/Game/RaceManager.cs` — Race state machine
- `Assets/Scripts/Core/SaveData.cs` — Progression/save system

**Tools:**
- `Tools/ModelGenerator/` — Shap-E 3D generation pipeline
- `Tools/BlenderScripts/` — Post-processing scripts
- `Tools/README.md` — Complete pipeline documentation

**Car Specs:**
- `Tools/ModelGenerator/car_specs/*.json` — Car definitions

## Adding Cars

Cars are generated with AI, not manually modeled. The pipeline:

1. Create JSON spec in `Tools/ModelGenerator/car_specs/`
2. Run `python generate_car.py car_specs/<name>.json`
3. Optimize with Blender script
4. Import FBX to Unity
5. Create prefab with VehicleController components

See `Tools/README.md` for full documentation.

## Code Patterns

**ScriptableObjects for data:**
- `VehicleStats` — Car performance parameters
- `CarDatabase` — Registry of all cars
- Use `[CreateAssetMenu]` for easy creation in Unity

**Singleton managers:**
- `GameManager.Instance` — Save/load, scene transitions
- `RaceManager.Instance` — Race state, positions

**Event-driven:**
- `RaceManager.OnRaceStateChanged`
- `RaceManager.OnCheckpointReached`

## UI Colors

```csharp
UIColors.PrimaryBlack    // #0F0F0F
UIColors.Anthracite      // #2D2D2D
UIColors.PetronasTeal    // #00D2BE (accent)
UIColors.Silver          // #C6C6C6
UIColors.White           // #FFFFFF
```

## Common Commands

```bash
# Generate a car model
cd Tools/ModelGenerator
python generate_car.py car_specs/monaco_gls.json

# Generate all cars
python batch_generate.py

# Optimize generated model
blender --background --python ../BlenderScripts/optimize_car.py -- \
  ../../Assets/Models/Vehicles/Generated/car.glb \
  ../../Assets/Models/Vehicles/car.fbx

# Git workflow
git add -A && git commit -m "message"
git push origin main
```

## What's Next (Vertical Slice)

Priority order for Phase 1:

1. ✅ Project structure and architecture
2. ✅ Vehicle physics system
3. ✅ Model generation pipeline
4. ⬜ First car model (Vento GT)
5. ⬜ First track (Monaco Boulevard)
6. ⬜ Basic race loop (start → race → finish)
7. ⬜ AI opponents
8. ⬜ Race HUD
9. ⬜ Main menu + garage
10. ⬜ Career mode basics
11. ⬜ Mobile build + touch controls

## Don't Forget

- **No real car names** — We use fictional brands (Monaco, Vento, Strada, etc.)
- **Mobile performance** — Keep poly counts low (8-25k per car)
- **Touch-first** — Design for touch, add controller support
- **Cloud save ready** — Interface exists but not implemented
