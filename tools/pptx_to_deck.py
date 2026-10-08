"""Convert a CAT105TC lecture .pptx into a Unity-consumable deck JSON.

Source of truth stays the .pptx. Output is written to <unity>/Assets/Slides/<key>.json.
JsonUtility constraints: no jagged arrays, no dictionaries, missing field == default.
"""
from __future__ import annotations

import argparse
import json
import re
from datetime import datetime
from pathlib import Path

from pptx import Presentation
from pptx.enum.shapes import MSO_SHAPE_TYPE, PP_PLACEHOLDER

CJK = re.compile(r"[\u4e00-\u9fff\u3040-\u30ff\uac00-\ud7af]")
CODE_HINT = re.compile(r"^\s*[A-Za-z_][\w\.]*\s*\(.*\)\s*;?\s*$|;\s*$")


def deck_key(p: Path) -> str:
    """'W4 - L4 - Animations and Camera.pptx' -> 'W04_L4'"""
    m = re.match(r"W(\d+)\s*-\s*([A-Za-z]\d+)", p.stem)
    if not m:
        raise ValueError(f"cannot derive a week key from {p.name!r}")
    return f"W{int(m.group(1)):02d}_{m.group(2)}"


def deck_title(p: Path) -> str:
    parts = p.stem.split(" - ")
    return parts[-1].strip() if len(parts) >= 3 else p.stem


def _blocks_from_frame(tf) -> list[dict]:
    out = []
    for para in tf.paragraphs:
        text = "".join(r.text for r in para.runs).strip()
        if not text:
            out.append({"level": 0, "text": "", "kind": "blank"})
            continue
        kind = "code" if CODE_HINT.match(text) and len(text) < 80 else "bullet"
        out.append({"level": min(int(para.level or 0), 4), "text": text, "kind": kind})
    # drop leading/trailing blanks so pages don't open with an empty line
    while out and out[0]["kind"] == "blank":
        out.pop(0)
    while out and out[-1]["kind"] == "blank":
        out.pop()
    return out


def _table(shape) -> dict | None:
    if not shape.has_table:
        return None
    rows = [[c.text.strip() for c in r.cells] for r in shape.table.rows]
    if not rows:
        return None
    cols = max(len(r) for r in rows)
    cells = [r[i] if i < len(r) else "" for r in rows for i in range(cols)]
    return {"columns": cols, "cells": cells}


def _is_subtitle(shape) -> bool:
    try:
        return bool(shape.is_placeholder and shape.placeholder_format.type == PP_PLACEHOLDER.SUBTITLE)
    except Exception:
        return False


def _picture(shape, key: str, out_dir: Path) -> dict | None:
    try:
        if shape.shape_type != MSO_SHAPE_TYPE.PICTURE:
            return None
        blob = shape.image.blob
    except Exception:
        return None
    out_dir.mkdir(parents=True, exist_ok=True)
    name = f"img_{shape.shape_id:02d}.png"
    (out_dir / name).write_bytes(blob)
    return {"path": f"{key}/{name}",
            "w": int(shape.width / 9525),      # EMU -> px @96dpi
            "h": int(shape.height / 9525)}


def convert(pptx_path: Path, image_root: Path | None = None) -> dict:
    prs = Presentation(str(pptx_path))
    key = deck_key(pptx_path)
    slides = []
    for i, s in enumerate(prs.slides, 1):
        title_shape = s.shapes.title
        title = subtitle = ""
        blocks: list[dict] = []
        table = image = None
        body_count = 0
        for sh in s.shapes:
            if sh == title_shape and sh.has_text_frame:
                title = sh.text_frame.text.strip()
            elif _is_subtitle(sh) and sh.has_text_frame:
                subtitle = sh.text_frame.text.strip()
            else:
                # a body shape is a text frame or a (possibly empty) body placeholder;
                # pictures/decorative shapes do not count toward the two-column test
                if sh.has_text_frame or sh.is_placeholder:
                    body_count += 1
                if sh.has_text_frame:
                    blocks += _blocks_from_frame(sh.text_frame)
            if sh.has_table:
                table = _table(sh)
            if image_root is not None:
                pic = _picture(sh, key, image_root / key)
                if pic:
                    image = pic
        if not title:
            # no title placeholder: use the first non-empty text shape's first paragraph
            for sh in s.shapes:
                if sh.has_text_frame and sh.text_frame.text.strip():
                    title = sh.text_frame.text.strip().splitlines()[0].strip()
                    break
        if not blocks:
            layout = "title"
        elif body_count >= 2:
            layout = "twoColumn"
        else:
            layout = "content"
        slides.append({"index": i, "layout": layout, "title": title,
                       "subtitle": subtitle, "blocks": blocks,
                       "table": table, "image": image})
    return {"key": key, "title": deck_title(pptx_path), "subtitle": "",
            "source": pptx_path.name, "generated": datetime.now().isoformat(timespec="seconds"),
            "slides": slides}


def main(argv=None) -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("deck", nargs="*", help=".pptx files (default: all under --slides-dir)")
    ap.add_argument("--slides-dir", default="/Users/gyk/Documents/Work/AY26-27/CAT105TC/Slides")
    ap.add_argument("--out-dir", default="Assets/Slides")
    args = ap.parse_args(argv)
    decks = [Path(p) for p in args.deck] or sorted(Path(args.slides_dir).glob("*.pptx"))
    out_dir = Path(args.out_dir)
    out_dir.mkdir(parents=True, exist_ok=True)
    for d in decks:
        data = convert(d, image_root=out_dir)
        (out_dir / f"{data['key']}.json").write_text(json.dumps(data, indent=2), encoding="utf8")
        cjk = sum(1 for s in data["slides"] if CJK.search(json.dumps(s, ensure_ascii=False)))
        print(f"{data['key']:8} slides={len(data['slides']):3} cjk={cjk:2}  <- {d.name}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
