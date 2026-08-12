# Monaco Motors

A high-detail simcade racing game built with Unity 6. Forza-inspired driving experience with AI-generated car models, targeting mobile platforms (iOS/Android) with PC-ready architecture.

## Features

- **Simcade Physics** — Realistic feel, accessible gameplay (Forza Horizon style)
- **AI-Generated Cars** — Unique vehicles created with Shap-E, refined in Blender
- **Multiple Tracks** — 4 distinct environments (city, coastal, desert, mountain)
- **Career Mode** — Bronze to Platinum leagues with progression and unlocks
- **Customization** — F2P cosmetics (paint, wheels, liveries, details)
- **Premium UI** — Mercedes AMG-inspired dark theme with teal accents

## Tech Stack

- **Unity 6 LTS** (6000.x)
- **Universal Render Pipeline (URP)** — Mobile-optimized graphics
- **New Input System** — Touch + controller support
- **Addressables** — On-demand asset loading
- **TextMeshPro** — UI text rendering
- **Shap-E** — AI 3D model generation (self-hosted)
- **Blender** — Model post-processing

---

## For Claude Agents

This section documents the project for future Claude Code sessions.

### Quick Context

Monaco Motors is a mobile racing game with Forza Horizon-style simcade physics. The project uses:
- **Vertical slice approach** — Build one complete experience first, then expand
- **AI model generation** — Cars are generated with Shap-E, not manually modeled
- **Mobile-first** — iOS/Android primary targets, PC secondary

### Key Files

| File | Purpose |
|------|---------|
| `docs/superpowers/specs/2026-08-12-monaco-motors-design.md` | Full game design spec |
| `Tools/README.md` | Model generation pipeline docs |
| `Assets/Scripts/Vehicle/VehicleController.cs` | Core driving physics |
| `Assets/Scripts/Vehicle/VehicleStats.cs` | Car stats ScriptableObject |
| `Assets/Scripts/Game/RaceManager.cs` | Race state machine |
| `Assets/Scripts/Core/SaveData.cs` | Save/load system |

### How to Add a New Car

1. **Create car spec:**
   ```bash
   # Copy template and edit
   cp Tools/ModelGenerator/car_specs/monaco_gls.json Tools/ModelGenerator/car_specs/new_car.json
   # Edit: car_id, display_name, prompt, stats
   ```

2. **Generate 3D model:**
   ```bash
   cd Tools/ModelGenerator
   python generate_car.py car_specs/new_car.json
   ```

3. **Optimize in Blender:**
   ```bash
   blender --background --python Tools/BlenderScripts/optimize_car.py -- \
     Assets/Models/Vehicles/Generated/new_car.glb \
     Assets/Models/Vehicles/new_car.fbx
   ```

4. **Unity setup:**
   - Create `VehicleStats` ScriptableObject (Right-click → Create → Monaco Motors → Vehicle Stats)
   - Create prefab from imported FBX
   - Add components: `VehicleController`, `VehicleInput`, `CarCustomization`
   - Add to `CarDatabase` ScriptableObject

### How to Add a New Track

1. Create new scene in `Assets/Scenes/Tracks/`
2. Build road using spline tool or prefab pieces
3. Add `CheckpointSystem` with checkpoint triggers
4. Set up `SpawnPoints` for start grid
5. Create `AIWaypoints` path for AI drivers
6. Add environment props and lighting
7. Register in track database

### Current Car Roster

| ID | Name | Class | Status |
|----|------|-------|--------|
| `vento_gt` | Vento GT | Sports | Planned (starter car) |
| `monaco_gls` | Monaco GLS | SUV | Spec created |
| `strada_500` | Strada 500 | Muscle | Planned |
| `futura_rs` | Futura RS | Supercar | Planned |
| `classico_1965` | Classico 1965 | Classic | Planned |
| `tempest_x` | Tempest X | Hypercar | Planned |
| `urbano_e` | Urbano E | Electric | Planned |

### Architecture Notes

**Vehicle Physics:**
- Raycast suspension (not WheelCollider) — simpler, more predictable
- Rigidbody-based with arcade grip assist
- Drift via handbrake + reduced lateral grip

**Save System:**
- Local JSON in `Application.persistentDataPath`
- Cloud save interface ready (not implemented)
- See `SaveData.cs` for schema

**UI Color Scheme:**
- Primary: `#0F0F0F` (deep black)
- Accent: `#00D2BE` (Petronas teal)
- Secondary: `#C6C6C6` (silver)
- See `UIColors.cs` for constants

### Common Tasks

**Run model generation pipeline:**
```bash
cd Tools/ModelGenerator
source venv/bin/activate
python generate_car.py car_specs/<car_id>.json
```

**Generate all cars:**
```bash
python batch_generate.py
```

**Test vehicle physics:**
Open `Assets/Scenes/Race.unity`, enter Play mode, use WASD + Shift to drive.

---

## Project Structure

```
monaco-motors/
├── Assets/
│   ├── Scripts/
│   │   ├── Vehicle/      # VehicleController, VehicleInput, AIDriver, etc.
│   │   ├── Track/        # CheckpointSystem, Checkpoint
│   │   ├── Game/         # RaceManager
│   │   ├── UI/           # UIColors
│   │   └── Core/         # GameManager, SaveData
│   ├── Models/
│   │   ├── Vehicles/     # Car FBX files
│   │   └── Tracks/       # Environment assets
│   ├── Prefabs/
│   ├── Scenes/
│   ├── Materials/
│   ├── Textures/
│   └── Audio/
├── Tools/
│   ├── ModelGenerator/   # Shap-E car generation
│   │   ├── generate_car.py
│   │   ├── batch_generate.py
│   │   ├── requirements.txt
│   │   └── car_specs/    # Car JSON specs
│   └── BlenderScripts/   # Post-processing
│       └── optimize_car.py
├── Packages/
├── ProjectSettings/
└── docs/
    └── superpowers/specs/  # Design documents
```

## Getting Started

1. Install **Unity 6** (6000.x LTS)
2. Clone this repository
3. Open the project in Unity Hub
4. Install required packages (URP, Input System, Addressables, TextMeshPro)
5. Open `Assets/Scenes/MainMenu.unity` to start

### Model Generation Setup

```bash
cd Tools/ModelGenerator
python -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate
pip install -r requirements.txt
```

Requires CUDA GPU with 8GB+ VRAM for best results.

## Vertical Slice (Phase 1)

Initial playable version includes:
- 1 car (Vento GT)
- 1 track (Monaco Boulevard) with 4 variants
- 4 AI opponents
- Basic career mode (Bronze League, 1 Series)
- Touch controls
- Local save system

## Color Scheme

Mercedes AMG-inspired premium theme:
- **Primary:** Deep black `#0F0F0F` / Anthracite `#2D2D2D`
- **Accent:** Petronas teal `#00D2BE`
- **Secondary:** Silver `#C6C6C6`
- **Text:** White `#FFFFFF`

## License

All rights reserved. This is a private project.
