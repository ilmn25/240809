"""Rotate a setpiece JSON around the Y axis by 90-degree steps.

Usage:
    python rotate_setpiece.py DungeonExit.json [--steps 1] [--out DungeonExit90.json]

Index order is x + N*(y + N*z). Entities are rotated with the same mapping.
"""
import argparse
import json


def rotate_coord(x, y, z, n, steps):
    for _ in range(steps % 4):
        x, z = z, n - 1 - x  # 90° clockwise (viewed from above, +Y up)
    return x, y, z


def rotate(data, steps):
    n = data["size"]
    blocks = data["blocks"]
    if len(blocks) != n ** 3:
        raise ValueError(f"blocks length {len(blocks)} != size^3 {n ** 3}")

    out = [0] * (n ** 3)
    for idx, b in enumerate(blocks):
        x = idx % n
        y = (idx // n) % n
        z = idx // (n * n)
        rx, ry, rz = rotate_coord(x, y, z, n, steps)
        out[rx + n * (ry + n * rz)] = b

    data = dict(data)
    data["blocks"] = out
    if data.get("entities"):
        ents = []
        for e in data["entities"]:
            rx, ry, rz = rotate_coord(e["x"], e["y"], e["z"], n, steps)
            ents.append({**e, "x": rx, "y": ry, "z": rz})
        data["entities"] = ents
    return data


def main():
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("input")
    ap.add_argument("--steps", type=int, default=1, help="90-degree steps (0-3)")
    ap.add_argument("--out", default=None, help="output file (default: <input> with suffix)")
    args = ap.parse_args()

    steps = args.steps % 4
    with open(args.input, encoding="utf-8") as f:
        data = json.load(f)

    rotated = rotate(data, steps)
    out = args.out or args.input.replace(".json", f"_{steps * 90}.json")
    with open(out, "w", encoding="utf-8") as f:
        json.dump(rotated, f, indent=4)

    print(f"Rotated {args.input} by {steps * 90} deg -> {out}")


if __name__ == "__main__":
    main()
