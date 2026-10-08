import sys, json, os
from pathlib import Path
import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from pptx_to_deck import convert, deck_key, deck_title, block_kind

# The course .pptx files are NOT part of this repository (they are the lecturer's source
# material), so the tests that read one skip unless you point CAT105TC_DECK at it.
DECK = Path(os.environ.get(
    "CAT105TC_DECK",
    Path(__file__).resolve().parents[2] / "Slides" / "W4 - L4 - Animations and Camera.pptx"))


def deck_path():
    """The real course deck, or skip - the .pptx files do not ship with the repo."""
    if not DECK.exists():
        pytest.skip(f"course deck not present ({DECK}); set CAT105TC_DECK to run this test")
    return DECK

def test_key_slug():
    assert deck_key(DECK) == "W04_L4"

def test_deck_title():
    assert deck_title(DECK) == "Animations and Camera"

def test_slide_count_matches_source():
    from pptx import Presentation
    deck = convert(deck_path())
    assert len(deck["slides"]) == len(Presentation(DECK).slides)

def test_shape_is_jsonutility_safe():
    deck = convert(deck_path())
    assert deck["slides"][0]["layout"] == "title"
    for s in deck["slides"]:
        assert isinstance(s["blocks"], list)
        for b in s["blocks"]:
            assert b["kind"] in ("bullet", "code", "blank")
            assert isinstance(b["level"], int)
        # no dict/None surprises
        json.dumps(deck)

def test_no_empty_trailing_blocks():
    for s in convert(deck_path())["slides"]:
        if s["blocks"]:
            assert s["blocks"][-1]["kind"] != "blank"

def test_title_slide_has_subtitle_no_blocks():
    deck = convert(deck_path())
    for s in (deck["slides"][0], deck["slides"][-1]):
        assert s["layout"] == "title"
        assert s["subtitle"] == "Lecture 4"
        assert s["blocks"] == []

def test_slide_24_is_two_column():
    deck = convert(deck_path())
    assert deck["slides"][23]["layout"] == "twoColumn"


def test_deck_subtitle_from_title_slide():
    deck = convert(deck_path())
    assert deck["subtitle"] == "Lecture 4"


def test_prose_ending_in_semicolon_stays_bullet():
    assert block_kind("The code is written here for A;") == "bullet"


def test_statement_ending_in_semicolon_is_code():
    assert block_kind("rb.velocity = new Vector2(1, 2);") == "code"
