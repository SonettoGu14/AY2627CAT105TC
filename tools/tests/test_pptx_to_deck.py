import sys, json
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from pptx_to_deck import convert, deck_key, deck_title

DECK = Path("/Users/gyk/Documents/Work/AY26-27/CAT105TC/Slides/W4 - L4 - Animations and Camera.pptx")

def test_key_slug():
    assert deck_key(DECK) == "W04_L4"

def test_deck_title():
    assert deck_title(DECK) == "Animations and Camera"

def test_slide_count_matches_source():
    from pptx import Presentation
    deck = convert(DECK)
    assert len(deck["slides"]) == len(Presentation(DECK).slides)

def test_shape_is_jsonutility_safe():
    deck = convert(DECK)
    assert deck["slides"][0]["layout"] == "title"
    for s in deck["slides"]:
        assert isinstance(s["blocks"], list)
        for b in s["blocks"]:
            assert b["kind"] in ("bullet", "code", "blank")
            assert isinstance(b["level"], int)
        # no dict/None surprises
        json.dumps(deck)

def test_no_empty_trailing_blocks():
    for s in convert(DECK)["slides"]:
        if s["blocks"]:
            assert s["blocks"][-1]["kind"] != "blank"

def test_title_slide_has_subtitle_no_blocks():
    deck = convert(DECK)
    for s in (deck["slides"][0], deck["slides"][-1]):
        assert s["layout"] == "title"
        assert s["subtitle"] == "Lecture 4"
        assert s["blocks"] == []

def test_slide_24_is_two_column():
    deck = convert(DECK)
    assert deck["slides"][23]["layout"] == "twoColumn"
