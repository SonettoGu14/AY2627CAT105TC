# Scenes

Two kinds of scene live here, defined in full under **"Scene types"** in
`Assets/Scripts/README.md`:

| Scene | Kind | Job |
|---|---|---|
| `WnnLab.unity` | **Lab Playground** | the week's hands-on scene: the students build and run it |
| `WnnSlides.unity` | **Slides** | the lecture deck, with a live demo behind every slide |

Currently: `W2Lab`, `W3Lab`, `W4Lab` (labs) and `W04Slides` (slides).

**Both kinds are generated.** Rebuild them from the menu bar —

* `CAT105TC ▸ Wnn … ▸ Build Wnn Lab`
* `CAT105TC ▸ Slides ▸ Build WnnSlides`

— rather than editing them. The builders recreate a scene from scratch, so hand edits are lost on
the next build. To change a lab, edit its builder in `Assets/Editor/Wnn_Topic/`; to change a deck,
edit the `.pptx` (or the generated JSON in `Assets/Slides/`) and rebuild.

Build Settings order: `W2Lab`, `W3Lab`, `W4Lab`, `W04Slides`.
