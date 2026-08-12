# Monaco Motors

A high-detail simcade racing game built with Unity 6. Forza-inspired driving experience with AI-generated car models, targeting mobile platforms (iOS/Android) with PC-ready architecture.

## Features

- **Simcade Physics** — Realistic feel, accessible gameplay (Forza Horizon style)
- **AI-Generated Cars** — 5-8 unique vehicles created with Meshy/Tripo3D, refined in Blender
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

## Project Structure

```
Assets/
├── Scripts/
│   ├── Vehicle/      # Physics, input, controls
│   ├── Track/        # Track logic, checkpoints, AI paths
│   ├── Game/         # Race manager, career, progression
│   ├── UI/           # Menus, HUD, garage
│   └── Core/         # Save system, audio, utilities
├── Models/
│   ├── Vehicles/     # Car FBX files + materials
│   └── Tracks/       # Environment assets
├── Prefabs/
├── Scenes/
│   ├── MainMenu.unity
│   ├── Garage.unity
│   ├── Race.unity
│   └── Loading.unity
├── Materials/
├── Textures/
└── Audio/
```

## Getting Started

1. Install **Unity 6** (6000.x LTS)
2. Clone this repository
3. Open the project in Unity Hub
4. Install required packages (URP, Input System, Addressables, TextMeshPro)
5. Open `Assets/Scenes/MainMenu.unity` to start

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

## Car Roster (Planned)

| Car | Class | Focus |
|-----|-------|-------|
| Vento GT | Sports | Balanced |
| Strada 500 | Muscle | Top speed |
| Futura RS | Supercar | Acceleration |
| Classico 1965 | Classic | Drift-friendly |
| Tempest X | Hypercar | Ultimate |
| Urbano E | Electric | Instant torque |

## License

All rights reserved. This is a private project.

## Contributing

Internal development only at this time.
