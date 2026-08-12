#!/usr/bin/env python3
"""
Batch generate all car models from specs in car_specs/ directory.
"""

import os
import sys
from pathlib import Path
from generate_car import generate_car_from_spec


def main():
    specs_dir = Path(__file__).parent / "car_specs"

    if not specs_dir.exists():
        print(f"Error: Specs directory not found: {specs_dir}")
        sys.exit(1)

    spec_files = list(specs_dir.glob("*.json"))

    if not spec_files:
        print("No spec files found in car_specs/")
        sys.exit(1)

    print(f"Found {len(spec_files)} car specs to generate:\n")
    for spec in spec_files:
        print(f"  - {spec.stem}")

    print("\n" + "=" * 50)

    for i, spec_file in enumerate(spec_files, 1):
        print(f"\n[{i}/{len(spec_files)}] Generating {spec_file.stem}...")
        try:
            generate_car_from_spec(str(spec_file))
        except Exception as e:
            print(f"Error generating {spec_file.stem}: {e}")
            continue

    print("\n" + "=" * 50)
    print("Batch generation complete!")


if __name__ == "__main__":
    main()
