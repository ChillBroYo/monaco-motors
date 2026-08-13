#!/usr/bin/env python3
"""
Generic Driving Test Game - Car Model Generator
Uses Shap-E for text-to-3D generation with post-processing pipeline.

Supported platforms:
- Windows with NVIDIA GPU (CUDA) - fastest
- Windows with Intel CPU - uses all cores
- macOS with Apple Silicon (M1/M2/M3/M4/M5) - CPU mode (MPS lacks float64)
- macOS with Intel - uses all cores
- Linux with NVIDIA GPU (CUDA) - fastest
"""

import argparse
import json
import os
import platform
import sys
from pathlib import Path

import torch
import trimesh
import numpy as np
from tqdm import tqdm

# Shap-E imports
from shap_e.diffusion.sample import sample_latents
from shap_e.diffusion.gaussian_diffusion import diffusion_from_config
from shap_e.models.download import load_model, load_config
from shap_e.util.notebooks import decode_latent_mesh


def get_system_info() -> dict:
    """Detect system capabilities for optimal configuration."""
    info = {
        "os": platform.system(),
        "machine": platform.machine(),
        "processor": platform.processor(),
        "cuda_available": torch.cuda.is_available(),
        "mps_available": hasattr(torch.backends, 'mps') and torch.backends.mps.is_available(),
        "cpu_count": os.cpu_count() or 4,
    }

    if info["cuda_available"]:
        info["cuda_device"] = torch.cuda.get_device_name(0)
        info["cuda_memory"] = torch.cuda.get_device_properties(0).total_memory // (1024**3)

    return info


def select_device(requested_device: str = None) -> str:
    """Select optimal compute device based on system capabilities."""
    info = get_system_info()

    # User override
    if requested_device:
        print(f"Using requested device: {requested_device}")
        return requested_device

    # CUDA (Windows/Linux with NVIDIA GPU) - best option
    if info["cuda_available"]:
        print(f"✓ CUDA available: {info.get('cuda_device', 'Unknown GPU')}")
        print(f"  GPU Memory: {info.get('cuda_memory', '?')} GB")
        return "cuda"

    # MPS (Apple Silicon) - Shap-E needs float64 which MPS doesn't support
    if info["mps_available"]:
        print(f"Note: Apple Silicon detected ({info['machine']})")
        print("  MPS available but Shap-E requires float64 (unsupported by MPS)")
        print(f"  Using CPU with {info['cpu_count']} cores (still fast on Apple Silicon)")
        return "cpu"

    # CPU fallback
    print(f"Using CPU with {info['cpu_count']} cores")
    if info["os"] == "Windows":
        print("  Tip: Install CUDA toolkit for faster generation with NVIDIA GPU")
    return "cpu"


class CarModelGenerator:
    def __init__(self, device: str = None):
        print("\n" + "="*60)
        print("Generic Driving Test Game - Car Model Generator")
        print("="*60)

        self.device = select_device(device)

        # Optimize CPU threading
        if self.device == "cpu":
            cpu_count = os.cpu_count() or 4
            torch.set_num_threads(cpu_count)
            print(f"  PyTorch threads: {cpu_count}")

        print("Loading Shap-E models...")
        self.xm = load_model("transmitter", device=self.device)
        self.model = load_model("text300M", device=self.device)
        self.diffusion = diffusion_from_config(load_config("diffusion"))
        print("Models loaded successfully.")

    def generate(self, spec: dict, output_dir: Path) -> Path:
        """Generate a 3D car model from specification."""

        car_id = spec["car_id"]
        gen_config = spec["generation"]

        print(f"\nGenerating: {spec['display_name']}")
        print(f"Prompt: {gen_config['prompt']}")

        # Generate latents
        batch_size = 1
        guidance_scale = gen_config.get("guidance_scale", 15.0)

        latents = sample_latents(
            batch_size=batch_size,
            model=self.model,
            diffusion=self.diffusion,
            guidance_scale=guidance_scale,
            model_kwargs=dict(texts=[gen_config["prompt"]]),
            progress=True,
            clip_denoised=True,
            use_fp16=True,
            use_karras=True,
            karras_steps=gen_config.get("num_inference_steps", 64),
            sigma_min=1e-3,
            sigma_max=160,
            s_churn=0,
        )

        # Decode to mesh
        print("Decoding latent to mesh...")
        mesh = decode_latent_mesh(self.xm, latents[0]).tri_mesh()

        # Convert to trimesh for processing
        vertices = mesh.verts.cpu().numpy()
        faces = mesh.faces.cpu().numpy()
        tri_mesh = trimesh.Trimesh(vertices=vertices, faces=faces)

        # Post-process
        tri_mesh = self._post_process(tri_mesh, spec.get("post_processing", {}))

        # Export
        output_dir.mkdir(parents=True, exist_ok=True)

        # Export as GLB (Unity-compatible)
        glb_path = output_dir / f"{car_id}.glb"
        tri_mesh.export(str(glb_path))
        print(f"Exported: {glb_path}")

        # Also export OBJ for Blender editing
        obj_path = output_dir / f"{car_id}.obj"
        tri_mesh.export(str(obj_path))
        print(f"Exported: {obj_path}")

        return glb_path

    def _post_process(self, mesh: trimesh.Trimesh, config: dict) -> trimesh.Trimesh:
        """Apply post-processing to the generated mesh."""

        print("Post-processing mesh...")

        # Center at origin
        if config.get("center_origin", True):
            mesh.vertices -= mesh.centroid

        # Scale to target size (car length in meters)
        target_size = config.get("scale_to_meters", 4.5)
        current_size = mesh.extents.max()
        if current_size > 0:
            scale_factor = target_size / current_size
            mesh.apply_scale(scale_factor)

        # Apply rotation correction
        rotation = config.get("rotation_correction", [0, 0, 0])
        if any(r != 0 for r in rotation):
            rot_matrix = trimesh.transformations.euler_matrix(
                np.radians(rotation[0]),
                np.radians(rotation[1]),
                np.radians(rotation[2])
            )
            mesh.apply_transform(rot_matrix)

        # Decimate if needed
        target_faces = config.get("target_faces")
        if target_faces and len(mesh.faces) > target_faces:
            try:
                import pymeshlab
                ms = pymeshlab.MeshSet()
                ms.add_mesh(pymeshlab.Mesh(mesh.vertices, mesh.faces))
                ms.meshing_decimation_quadric_edge_collapse(
                    targetfacenum=target_faces,
                    preservenormal=True
                )
                m = ms.current_mesh()
                mesh = trimesh.Trimesh(
                    vertices=m.vertex_matrix(),
                    faces=m.face_matrix()
                )
                print(f"Decimated to {len(mesh.faces)} faces")
            except ImportError:
                print("Warning: pymeshlab not installed, skipping decimation")

        # Smooth
        smooth_iter = config.get("smooth_iterations", 0)
        if smooth_iter > 0:
            trimesh.smoothing.filter_laplacian(mesh, iterations=smooth_iter)

        print(f"Final mesh: {len(mesh.vertices)} vertices, {len(mesh.faces)} faces")
        return mesh


def generate_car_from_spec(spec_path: str, output_dir: str = None):
    """Main entry point for car generation."""

    # Load spec
    with open(spec_path, "r") as f:
        spec = json.load(f)

    # Determine output directory
    if output_dir is None:
        project_root = Path(__file__).parent.parent.parent
        output_dir = project_root / "Assets" / "Models" / "Vehicles" / "Generated"
    else:
        output_dir = Path(output_dir)

    # Generate
    generator = CarModelGenerator()
    model_path = generator.generate(spec, output_dir)

    print(f"\n✓ Generation complete: {model_path}")
    print(f"\nNext steps:")
    print(f"1. Open in Blender for manual refinement")
    print(f"2. Import into Unity: {spec['unity']['model_path']}")
    print(f"3. Set up prefab with VehicleController")

    return model_path


def main():
    parser = argparse.ArgumentParser(
        description="Generate 3D car models for Generic Driving Test Game",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python generate_car.py car_specs/monaco_gls.json           # Auto-detect best device
  python generate_car.py car_specs/vento_gt.json --device cuda  # Force CUDA
  python generate_car.py car_specs/strada_500.json --device cpu  # Force CPU

Platform support:
  - Windows + NVIDIA GPU: Uses CUDA (fastest)
  - Windows + Intel CPU: Uses all CPU cores
  - macOS + Apple Silicon: Uses CPU (MPS lacks float64 support)
  - Linux + NVIDIA GPU: Uses CUDA
        """
    )
    parser.add_argument("spec", help="Path to car spec JSON file")
    parser.add_argument("-o", "--output", help="Output directory", default=None)
    parser.add_argument(
        "--device",
        help="Compute device: 'cuda' (NVIDIA GPU), 'cpu', or 'auto' (default)",
        choices=["cuda", "cpu", "auto"],
        default=None
    )
    parser.add_argument(
        "--info",
        action="store_true",
        help="Print system info and exit"
    )

    args = parser.parse_args()

    if args.info:
        info = get_system_info()
        print("\nSystem Information:")
        print(f"  OS: {info['os']}")
        print(f"  Machine: {info['machine']}")
        print(f"  Processor: {info['processor']}")
        print(f"  CPU Cores: {info['cpu_count']}")
        print(f"  CUDA Available: {info['cuda_available']}")
        if info['cuda_available']:
            print(f"    Device: {info.get('cuda_device', 'Unknown')}")
            print(f"    Memory: {info.get('cuda_memory', '?')} GB")
        print(f"  MPS Available: {info['mps_available']}")
        return

    if not os.path.exists(args.spec):
        print(f"Error: Spec file not found: {args.spec}")
        sys.exit(1)

    generate_car_from_spec(args.spec, args.output)


if __name__ == "__main__":
    main()
