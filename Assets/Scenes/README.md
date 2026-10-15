# Scenes

Three kinds of scene live here, defined in full under **"Scene types"** in
`Assets/Scripts/README.md`:

| Scene | Kind | Job |
|---|---|---|
| `WnnLab.unity` | **Lab** | the lecture's worked example, small enough for a student to reproduce |
| `WnnPlayground.unity` | **Playground** | the combined result: the week's topics laid out and runnable |
| `WnnSlides.unity` | **Slides** | the lecture deck, with a live demo behind every slide |

Currently: `W02Playground` `W03Playground` `W04Playground` `W05Playground`, `W05Lab`, and `W04Slides` `W05Slides`.

**All three kinds are generated.** Rebuild them from the menu bar —

* `CAT105TC ▸ Wnn … ▸ Build Wnn Playground`
* `CAT105TC ▸ Wnn … ▸ Build Wnn Lab (Starter)`
* `CAT105TC ▸ Slides ▸ Build WnnSlides`

— rather than editing them. The builders recreate a scene from scratch, so hand edits are lost on
the next build. To change a lab, edit its builder in `Assets/Editor/Wnn_Topic/`; to change a deck,
edit the `.pptx` (or the generated JSON in `Assets/Slides/`) and rebuild.

Build Settings order: `W02Playground`, `W03Playground`, `W04Playground`, `W04Slides`, `W05Playground`, `W05Slides`, `W05Lab`.
