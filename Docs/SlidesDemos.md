# W04 slide demos — design and checklist

**Why this exists.** The first version of `W04Slides` showed the slides with the *W4Lab platformer*
behind them. That is pedagogically empty: if the thing behind the deck is just the W4 lab, there is
no reason to present from inside Unity at all. **Each slide must reveal a live demonstration of the
thing that slide is teaching.**

**Rule (from the instructor):** split by knowledge point. **One demo teaches one thing** — no
composite demos. So "SpriteRenderer: Color / Flip / Sorting Layer" is three demos, not one.

**Presentation (from the instructor):** a fixed demo stage + a live readout panel (current state /
variable values / the API call being made) + a key hint, styled like the slides.

---

## Architecture

| Piece | Responsibility |
|---|---|
| `DemoBase` (abstract MonoBehaviour) | one demo: `Title`, `Keys` hint, `string[] Readout()`; `OnActivate()` / `OnDeactivate()` |
| `DemoStage` | owns every demo's world anchor + camera framing. `Activate(DemoSlot)` deactivates all demo roots, activates one, moves the stage camera. `NextDemo()` / `PreviousDemo()` move within the current slide's list |
| `DemoReadout` (HUD, Chrome canvas) | polls the active `DemoBase` each frame and draws Title / Readout / Keys / "demo 2 / 3" |
| `SlideDemoMap` | per-slide `string[] demoKeys`, loaded from `Assets/Slides/<key>.demos.json` and baked onto `SlidePresenter` by the builder |
| `SlidePresenter` | on hide: `demoStage.Activate(demosForSlide[SlideIndex], 0)`; on show: `demoStage.DeactivateAll()` |

**Layout.** Every demo gets its own world anchor (12 units apart in a grid). Only the active one is
enabled; the stage camera moves to that anchor. Demos are built by `SlideDeckBuilder`.

**Keys while the deck is hidden**

| Key | Action |
|---|---|
| `,` / `.` | previous / next demo **for the current slide** |
| `1`…`9` | jump straight to demo N of the current slide |
| `Tab` | back to the slides (the demo deactivates) |
| the demo's own keys | whatever that demo documents in its `Keys` hint |

---

## The 44 demos

### Slides 2–4 — 2D controller review + Raycast (9)

| key | slide | teaches exactly one thing | keys | readout |
|---|---|---|---|---|
| `ctrl_move` | 2 | move by writing `rb.velocity` | A/D | `rb.velocity.x` |
| `ctrl_jump` | 2 | jump with `AddForce(…, Impulse)` | Space | `rb.velocity.y` |
| `ctrl_ground` | 2 | ground detection by `Physics2D.Raycast` | A/D | `IsGrounded` + the ray drawn |
| `rc_basic` | 3 | what a raycast is: emit, hit, distance | A/D moves the wall | hit? distance |
| `rc_nocollider` | 3 | the emitter needs **no** collider, the target does | T toggles the target's collider | hits / passes through |
| `rc_origin` | 4 | the `origin` parameter | W/S move the origin | origin, hit |
| `rc_direction` | 4 | the `direction` parameter | A/D rotate it | direction, hit |
| `rc_distance` | 4 | the `distance` parameter | W/S change length | distance, hit? |
| `rc_layermask` | 4 | the `layerMask` parameter | M toggles the mask | mask bits, hit / ignored |

### Slide 6 — Field Modifier (3)

| key | teaches | keys | readout |
|---|---|---|---|
| `fld_serialize` | `[SerializeField]` shows a *private* field in the Inspector | — | the field + its value |
| `fld_hide` | `[HideInInspector]` hides a *public* field | — | the field + its value |
| `fld_range` | `[Range(0,10)]` is a slider | A/D | value + a bar |

### Slides 7–10 — 2D rendering (6)

| key | slide | teaches | keys | readout |
|---|---|---|---|---|
| `sr_color` | 7 | `SpriteRenderer.color` tints the sprite | R/G/B | `color` |
| `sr_flip` | 7 | `flipX` / `flipY` mirror the sprite | F / V | `flipX`, `flipY` |
| `sr_sorting` | 7 | Sorting Layer + Order decides who is in front | W/S | both sprites' order |
| `sp_mask` | 8 | `SpriteMask` + `MaskInteraction` | A/D moves the mask | interaction mode |
| `sp_slice` | 9 | slicing turned one sheet into many sprites | A/D | frame n / 45 |
| `sp_swap` | 10 | assigning `spriteRenderer.sprite` from code | Space | the sprite's name |

### Slides 11–14 — Animation (6)

| key | slide | teaches | keys | readout |
|---|---|---|---|---|
| `an_clip` | 11 | an AnimationClip plays on an Animator | Space / A/D | clip time / length |
| `an_animator` | 11 | the Animator component is what plays the clip | — | controller name, state |
| `an_keyframes` | 12 | an animation is values at keyframes | Space | time, value, nearest key |
| `an_channels` | 12 | one clip can animate position, colour **and your own variables** | 1/2/3 | which channel |
| `an_oneatatime` | 12 | only one animation plays on an object at a time | A/D | which clip is playing |
| `an_priority` | 14 | the animation overrides code (the classic "bug") | A/D writes position from code | code's value vs the animated value |

### Slides 15–21 — Animator (10)

| key | slide | teaches | keys | readout |
|---|---|---|---|---|
| `am_basics` | 15 | the Animator manages order and logic | — | controller, current state |
| `am_states` | 16 | a State is a clip; one is playing | A/D | current state name |
| `am_default` | 16 | exactly one State is the default | D | which state is default |
| `am_transitions` | 17 | a Transition switches States | W/S ramps `Speed` | `Speed`, current state |
| `am_exittime` | 17 | Exit Time decides when a transition may fire | W/S | exit time, progress |
| `am_duration` | 17 | Transition Duration blends the two clips | W/S | duration, blend % |
| `am_vars` | 18 | float / int / bool / trigger | 1/2/3/4 | all four values |
| `am_booltrigger` | 19 | a bool stays set; a trigger clears itself | B / T | bool, trigger |
| `am_scripting` | 20 | `SetBool` / `SetTrigger` from code | B / T | the exact line of code |
| `am_play` | 21 | `animator.Play(name, layer, time)` cuts; a transition blends | A/D | API call vs state |

### Slides 22–27 — Camera (9)

| key | slide | teaches | keys | readout |
|---|---|---|---|---|
| `cam_projection` | 23 | Perspective vs Orthographic | P | mode, fov / size |
| `cam_fov` | 23 | Field of View is the cone width | W/S | fov |
| `cam_ortho` | 24 | orthographic `Size` is the view height | W/S | size |
| `cam_clearflags` | 25 | Clear Flags: Skybox / SolidColor / DepthOnly / Don't Clear | Space | flags + the result |
| `cam_background` | 26 | Background Color | R/G/B | colour |
| `cam_culling` | 26 | Culling Mask picks which layers a camera draws | M | mask, layers shown |
| `cam_depth` | 26 | Depth orders cameras; higher draws on top | D | both depths |
| `cam_follow` | 27 | follow by keeping an offset | A/D moves the target | offset |
| `cam_follow_smooth` | 27 | the "better way": smoothing the follow | A/D | target vs camera x |

### Slide 22 — practice (1)

| key | teaches | notes |
|---|---|---|
| `platformer` | putting animation + camera together | the existing W4 platformer, moved here from being the backdrop |

---

## Slide → demo mapping

```
1  →  (none)
2  →  ctrl_move, ctrl_jump, ctrl_ground
3  →  rc_basic, rc_nocollider
4  →  rc_origin, rc_direction, rc_distance, rc_layermask
5  →  (none)
6  →  fld_serialize, fld_hide, fld_range
7  →  sr_color, sr_flip, sr_sorting
8  →  sp_mask
9  →  sp_slice
10 →  sp_swap
11 →  an_clip, an_animator
12 →  an_keyframes, an_channels, an_oneatatime
13 →  (none — it walks through the Animation window UI)
14 →  an_priority
15 →  am_basics
16 →  am_states, am_default
17 →  am_transitions, am_exittime, am_duration
18 →  am_vars
19 →  am_booltrigger
20 →  am_scripting
21 →  am_play
22 →  platformer
23 →  cam_projection, cam_fov
24 →  cam_ortho
25 →  cam_clearflags
26 →  cam_background, cam_culling, cam_depth
27 →  cam_follow, cam_follow_smooth
28 →  (none)

44 demos across 22 slides.
```

## Build order

1. Infrastructure: `DemoBase`, `DemoStage`, `DemoReadout`, `SlideDemoMap`, presenter wiring, stage backdrop, the layout helper.
2. `field` (3) — smallest, proves the pipeline end to end.
3. `sprite` (6).
4. `raycast` + `ctrl` (9).
5. `animation` (6).
6. `animator` (10).
7. `camera` (9).
8. `platformer` (1) — move the existing gameplay to slide 22's slot.
9. End-to-end: every slide, `,`/`.` cycling, screenshots per demo.
