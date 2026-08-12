"""
Blender script for optimizing generated car models.
Run with: blender --background --python optimize_car.py -- input.glb output.fbx

This script:
1. Imports the generated model
2. Cleans up geometry
3. Creates proper UV maps
4. Separates materials (body, glass, wheels, interior)
5. Creates LOD variants
6. Exports as FBX for Unity
"""

import bpy
import sys
import os
import math


def clear_scene():
    """Remove all objects from scene."""
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)


def import_model(filepath: str):
    """Import GLB/OBJ file."""
    ext = os.path.splitext(filepath)[1].lower()

    if ext == '.glb' or ext == '.gltf':
        bpy.ops.import_scene.gltf(filepath=filepath)
    elif ext == '.obj':
        bpy.ops.import_scene.obj(filepath=filepath)
    elif ext == '.fbx':
        bpy.ops.import_scene.fbx(filepath=filepath)
    else:
        raise ValueError(f"Unsupported format: {ext}")

    # Get imported object
    obj = bpy.context.selected_objects[0]
    bpy.context.view_layer.objects.active = obj
    return obj


def clean_geometry(obj):
    """Clean up mesh geometry."""
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode='EDIT')

    # Remove doubles
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.remove_doubles(threshold=0.001)

    # Recalculate normals
    bpy.ops.mesh.normals_make_consistent(inside=False)

    # Triangulate for game engine
    bpy.ops.mesh.quads_convert_to_tris()

    bpy.ops.object.mode_set(mode='OBJECT')


def create_uvs(obj):
    """Create UV map using smart project."""
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=66, island_margin=0.02)
    bpy.ops.object.mode_set(mode='OBJECT')


def create_materials(obj):
    """Create material slots for car parts."""
    # Clear existing materials
    obj.data.materials.clear()

    materials = {
        'Body': (0.8, 0.8, 0.8, 1.0),      # Silver
        'Glass': (0.2, 0.3, 0.4, 0.5),      # Tinted blue
        'Wheels': (0.1, 0.1, 0.1, 1.0),     # Dark grey
        'Interior': (0.05, 0.05, 0.05, 1.0) # Black
    }

    for name, color in materials.items():
        mat = bpy.data.materials.new(name=name)
        mat.use_nodes = True

        # Set base color
        bsdf = mat.node_tree.nodes["Principled BSDF"]
        bsdf.inputs['Base Color'].default_value = color

        if name == 'Body':
            bsdf.inputs['Metallic'].default_value = 0.9
            bsdf.inputs['Roughness'].default_value = 0.1
        elif name == 'Glass':
            bsdf.inputs['Transmission'].default_value = 0.9
            bsdf.inputs['Roughness'].default_value = 0.0
        elif name == 'Wheels':
            bsdf.inputs['Metallic'].default_value = 0.7
            bsdf.inputs['Roughness'].default_value = 0.3

        obj.data.materials.append(mat)


def create_lods(obj, lod_levels=None):
    """Create LOD variants of the mesh."""
    if lod_levels is None:
        lod_levels = [1.0, 0.5, 0.25]  # LOD0, LOD1, LOD2

    lod_objects = []

    for i, ratio in enumerate(lod_levels):
        # Duplicate object
        new_obj = obj.copy()
        new_obj.data = obj.data.copy()
        new_obj.name = f"{obj.name}_LOD{i}"
        bpy.context.collection.objects.link(new_obj)

        if ratio < 1.0:
            # Apply decimate modifier
            bpy.context.view_layer.objects.active = new_obj
            mod = new_obj.modifiers.new(name='Decimate', type='DECIMATE')
            mod.ratio = ratio
            bpy.ops.object.modifier_apply(modifier=mod.name)

        lod_objects.append(new_obj)

    return lod_objects


def export_fbx(objects, filepath: str):
    """Export objects to FBX."""
    # Select objects to export
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:
        obj.select_set(True)

    bpy.ops.export_scene.fbx(
        filepath=filepath,
        use_selection=True,
        apply_scale_options='FBX_SCALE_ALL',
        axis_forward='-Z',
        axis_up='Y',
        use_mesh_modifiers=True,
        mesh_smooth_type='FACE',
        use_tspace=True
    )


def main():
    # Parse arguments after --
    argv = sys.argv
    if "--" in argv:
        argv = argv[argv.index("--") + 1:]
    else:
        print("Usage: blender --background --python optimize_car.py -- input.glb output.fbx")
        sys.exit(1)

    if len(argv) < 2:
        print("Error: Need input and output paths")
        sys.exit(1)

    input_path = argv[0]
    output_path = argv[1]

    print(f"Input: {input_path}")
    print(f"Output: {output_path}")

    # Process
    clear_scene()
    obj = import_model(input_path)
    clean_geometry(obj)
    create_uvs(obj)
    create_materials(obj)
    lods = create_lods(obj)
    export_fbx(lods, output_path)

    print(f"\n✓ Exported to {output_path}")


if __name__ == "__main__":
    main()
