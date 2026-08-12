#!/usr/bin/env python3
"""
Monaco Motors - Car Model Generator
Uses Shap-E for text-to-3D generation with post-processing pipeline.
"""

import argparse
import json
import os
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


class CarModelGenerator:
    def __init__(self, device: str = None):
        self.device = device or ("cuda" if torch.cuda.is_available() else "cpu")
        print(f"Using device: {self.device}")

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
    parser = argparse.ArgumentParser(description="Generate 3D car models for Monaco Motors")
    parser.add_argument("spec", help="Path to car spec JSON file")
    parser.add_argument("-o", "--output", help="Output directory", default=None)
    parser.add_argument("--device", help="Device (cuda/cpu)", default=None)

    args = parser.parse_args()

    if not os.path.exists(args.spec):
        print(f"Error: Spec file not found: {args.spec}")
        sys.exit(1)

    generate_car_from_spec(args.spec, args.output)


if __name__ == "__main__":
    main()
