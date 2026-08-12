# Monaco Motors - Tools

This directory contains tools for automated asset generation and processing.

## Directory Structure

```
Tools/
├── ModelGenerator/          # AI-powered 3D car model generation
│   ├── generate_car.py      # Main generation script (Shap-E)
│   ├── batch_generate.py    # Generate all cars from specs
│   ├── requirements.txt     # Python dependencies
│   └── car_specs/           # Car specification JSON files
│       └── monaco_gls.json  # Example: Monaco GLS SUV
├── BlenderScripts/          # Blender post-processing
│   └── optimize_car.py      # Clean, UV, LOD, export to FBX
└── README.md                # This file
```

## Model Generator

### Overview

The Model Generator uses **Shap-E** (OpenAI's open-source text-to-3D model) to generate car meshes from text descriptions. It's completely self-hosted and free to use.

### Requirements

- Python 3.8+
- PyTorch 2.0+
- CUDA-capable GPU (recommended, 8GB+ VRAM)
- Blender 3.0+ (for post-processing)

### Setup

```bash
cd Tools/ModelGenerator
python -m venv venv
source venv/bin/activate  # or `venv\Scripts\activate` on Windows
pip install -r requirements.txt
```

### Usage

**Generate a single car:**
```bash
python generate_car.py car_specs/monaco_gls.json
```

**Generate all cars:**
```bash
python batch_generate.py
```

**Output:** Models are saved to `Assets/Models/Vehicles/Generated/`

### Car Specification Format

Create a JSON file in `car_specs/` with this structure:

```json
{
  "car_id": "unique_identifier",
  "display_name": "Human Readable Name",
  "class": "SUV|Sports|Muscle|Supercar|Hypercar|Electric|Classic",
  "generation": {
    "prompt": "detailed text description for AI generation",
    "guidance_scale": 15.0,
    "num_inference_steps": 64,
    "target_faces": 8000
  },
  "post_processing": {
    "decimate_ratio": 0.5,
    "smooth_iterations": 2,
    "center_origin": true,
    "scale_to_meters": 4.5,
    "rotation_correction": [0, 90, 0]
  },
  "unity": {
    "prefab_path": "Assets/Prefabs/Vehicles/CarName.prefab",
    "model_path": "Assets/Models/Vehicles/car_name.fbx"
  },
  "stats": {
    "top_speed": 220,
    "acceleration_time": 4.5,
    "handling": 1.0,
    "drift_factor": 0.5,
    "braking": 1.0,
    "mass": 1400
  }
}
```

### Prompt Writing Tips

Good prompts for car generation:
- Be specific about body style: "compact SUV", "two-door coupe", "sedan"
- Include key features: "large front grille", "LED headlights", "roof rails"
- Specify view: "side view", "three-quarter view"
- Add style hints: "modern", "futuristic", "classic", "aggressive"
- Include "low poly" or "game asset" for optimized topology

**Example prompts:**

```
# Sports Car
"a sleek sports car, low profile, aerodynamic design, large rear spoiler, 
dual exhaust, wide body kit, 20-inch wheels, aggressive front bumper, 
side view, game asset"

# Luxury SUV
"a compact luxury SUV, boxy rectangular shape, modern crossover design, 
chrome accents, panoramic sunroof, LED light bar, 19-inch alloy wheels, 
clean surfaces, three-quarter view"

# Muscle Car
"an american muscle car, long hood, short deck, dual racing stripes, 
hood scoop, wide rear fenders, chrome bumpers, vintage 1970s style, 
side view, low poly"
```

## Blender Post-Processing

### Usage

After generating a model, run through Blender for optimization:

```bash
blender --background --python BlenderScripts/optimize_car.py -- \
  Assets/Models/Vehicles/Generated/monaco_gls.glb \
  Assets/Models/Vehicles/monaco_gls.fbx
```

### What It Does

1. **Imports** the generated GLB/OBJ
2. **Cleans geometry** - removes doubles, fixes normals, triangulates
3. **Creates UVs** - smart UV projection for texturing
4. **Sets up materials** - Body, Glass, Wheels, Interior slots
5. **Creates LODs** - LOD0 (100%), LOD1 (50%), LOD2 (25%)
6. **Exports FBX** - Unity-compatible format

## Full Pipeline

Complete workflow from spec to Unity:

```bash
# 1. Create car spec
cat > Tools/ModelGenerator/car_specs/new_car.json << 'EOF'
{
  "car_id": "new_car",
  "display_name": "New Car",
  ...
}
EOF

# 2. Generate with Shap-E
cd Tools/ModelGenerator
python generate_car.py car_specs/new_car.json

# 3. Optimize in Blender
cd ../..
blender --background --python Tools/BlenderScripts/optimize_car.py -- \
  Assets/Models/Vehicles/Generated/new_car.glb \
  Assets/Models/Vehicles/new_car.fbx

# 4. Import in Unity
# - Open Unity project
# - FBX auto-imports with LODs
# - Create prefab with VehicleController
# - Assign VehicleStats ScriptableObject
```

## Adding a New Car (Quick Reference)

1. **Create spec:** `Tools/ModelGenerator/car_specs/{car_id}.json`
2. **Generate:** `python generate_car.py car_specs/{car_id}.json`
3. **Optimize:** `blender --background --python optimize_car.py -- input.glb output.fbx`
4. **Unity setup:**
   - Create `VehicleStats` ScriptableObject with car stats
   - Create prefab from FBX
   - Add `VehicleController`, `VehicleInput`, `CarCustomization` components
   - Add to `CarDatabase`

## Troubleshooting

**Out of VRAM:**
- Reduce `num_inference_steps` (minimum ~32)
- Use CPU mode (slower): `python generate_car.py --device cpu spec.json`

**Poor quality output:**
- Increase `guidance_scale` (15-20 range)
- Improve prompt specificity
- Try different seed values

**Blender errors:**
- Ensure Blender 3.0+ is installed
- Check file paths use forward slashes

## Future Improvements

- [ ] ControlNet integration for reference image guidance
- [ ] Automatic material segmentation (body vs glass vs wheels)
- [ ] Texture generation pipeline
- [ ] Direct Unity import without Blender step
