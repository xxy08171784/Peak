"""Pre-bake the game's Rare HSV transform into custom portrait borders.

The vanilla uncommon banner material is the identity transform. Peak's original
custom borders were authored from the red card-frame source, so they rendered
red for uncommon and blue after the rare transform. Applying the rare transform
once to the source establishes the same blue baseline as the vanilla portrait
borders: uncommon stays blue, rare rotates to orange, and common desaturates to
gray. Pixel alpha is preserved byte-for-byte.
"""

from __future__ import annotations

import argparse
from pathlib import Path

import numpy as np
from PIL import Image


RGB_TO_YIQ = np.array(
    [
        [0.2989, 0.5870, 0.1140],
        [0.5959, -0.2774, -0.3216],
        [0.2115, -0.5229, 0.3114],
    ],
    dtype=np.float64,
)


def apply_hsv_shader(rgb: np.ndarray, hue_parameter: float, saturation: float, value: float) -> np.ndarray:
    """Match res://shaders/hsv.gdshader for an array of RGB row vectors."""
    yiq = rgb @ RGB_TO_YIQ.T
    hue = (1.0 - hue_parameter) * 2.0 * np.pi
    sin_hue = np.sin(hue)
    cos_hue = np.cos(hue)
    hue_shift = np.array(
        [
            [1.0, 0.0, 0.0],
            [0.0, cos_hue, sin_hue],
            [0.0, -sin_hue, cos_hue],
        ],
        dtype=np.float64,
    )
    yiq = yiq @ hue_shift
    yiq[..., 1:] *= saturation
    yiq *= value
    return yiq @ np.linalg.inv(RGB_TO_YIQ).T


def transform(path: Path) -> None:
    image = Image.open(path).convert("RGBA")
    pixels = np.asarray(image, dtype=np.uint8)
    original_alpha = pixels[..., 3].copy()
    rgb = pixels[..., :3].astype(np.float64) / 255.0

    # Values read directly from the game's card_banner_rare_mat.tres.
    corrected_rgb = apply_hsv_shader(rgb, hue_parameter=0.563, saturation=1.198, value=1.14)
    output = pixels.copy()
    output[..., :3] = np.rint(np.clip(corrected_rgb, 0.0, 1.0) * 255.0).astype(np.uint8)

    if not np.array_equal(output[..., 3], original_alpha):
        raise RuntimeError(f"Alpha changed while transforming {path}")

    Image.fromarray(output, mode="RGBA").save(path)
    print(f"Pre-baked rarity base color: {path}")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("images", nargs="+", type=Path)
    args = parser.parse_args()
    for image_path in args.images:
        if not image_path.is_file():
            raise FileNotFoundError(image_path)
        transform(image_path)


if __name__ == "__main__":
    main()
