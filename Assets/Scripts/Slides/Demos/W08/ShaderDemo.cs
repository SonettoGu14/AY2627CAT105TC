using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slide 7 - a material is a shader plus its parameters. Same sphere, same colour, three shaders:
/// URP/Lit responds to the light, URP/Unlit ignores it, and the Built-in Standard shader in a URP
/// project renders the familiar magenta. The pink is not a colour anyone chose - it is the shader
/// failing to match the pipeline.
/// </summary>
public class ShaderDemo : DemoBase
{
    public Transform spin;
    public Renderer target;
    public Material lit;
    public Material unlit;
    public Material broken;

    private int mode;
    private float angle;

    public override string Title { get { return "The shader decides how the surface responds to light"; } }

    public override string Keys { get { return "S  cycle URP/Lit -> URP/Unlit -> Standard (pink)"; } }

    public override void OnActivate()
    {
        mode = 0; angle = -20f;
        Apply();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) { mode = (mode + 1) % 3; Apply(); }
        angle += 18f * Time.deltaTime;
        if (spin != null) spin.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    private void Apply()
    {
        if (target == null) return;
        target.sharedMaterial = mode == 0 ? lit : mode == 1 ? unlit : broken;
    }

    public override void Readout(List<string> lines)
    {
        string shader = target != null && target.sharedMaterial != null ? target.sharedMaterial.shader.name : "(none)";
        lines.Add("shader: " + shader);
        lines.Add("");
        switch (mode)
        {
            case 0:
                lines.Add("URP/Lit: reads the lights and shades the surface.");
                lines.Add("albedo, metallic, smoothness are parameters inside it.");
                break;
            case 1:
                lines.Add("URP/Unlit: draws the colour and stops.");
                lines.Add("The same parameters are there, but this shader");
                lines.Add("ignores every light in the scene.");
                break;
            default:
                lines.Add("Standard is a Built-in shader; URP cannot draw it.");
                lines.Add("The magenta is the error colour, not a material choice.");
                lines.Add("Fix: recreate the material with URP/Lit.");
                break;
        }
    }
}
