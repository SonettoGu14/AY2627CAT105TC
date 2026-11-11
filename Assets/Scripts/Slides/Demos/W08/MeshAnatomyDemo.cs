using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slide 4 - a mesh is vertices, edges, and faces. A cube you can take apart: V shows the eight
/// corner positions, E the twelve edges, F the six faces (twelve triangles). The readout also makes
/// the point the slide makes: Unity's cube stores 24 vertices, not 8, because every face needs its
/// own normal and UV.
/// </summary>
public class MeshAnatomyDemo : DemoBase
{
    public Transform spin;
    public MeshFilter cube;
    public GameObject vertexRoot;
    public GameObject edgeRoot;
    public MeshRenderer cubeRenderer;

    private bool vertices, edges = true, faces = true;
    private bool paused;
    private float angle;

    public override string Title { get { return "A mesh is vertices, edges, and faces"; } }

    public override string Keys { get { return "V  vertices     E  edges     F  faces     Space  pause the spin"; } }

    public override void OnActivate()
    {
        vertices = false; edges = true; faces = true; paused = false; angle = -28f;
        Apply();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V)) { vertices = !vertices; Apply(); }
        if (Input.GetKeyDown(KeyCode.E)) { edges = !edges; Apply(); }
        if (Input.GetKeyDown(KeyCode.F)) { faces = !faces; Apply(); }
        if (Input.GetKeyDown(KeyCode.Space)) paused = !paused;

        if (!paused)
        {
            angle += 18f * Time.deltaTime;
            if (spin != null) spin.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    private void Apply()
    {
        if (vertexRoot != null) vertexRoot.SetActive(vertices);
        if (edgeRoot != null) edgeRoot.SetActive(edges);
        if (cubeRenderer != null) cubeRenderer.enabled = faces;
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("corner positions  8");
        lines.Add("mesh vertices     " + (cube != null && cube.sharedMesh != null ? cube.sharedMesh.vertexCount : 24) +
                  "   (each face needs its own normal + UV)");
        lines.Add("edges             12");
        lines.Add("faces             6   (12 triangles)");
        lines.Add("");
        lines.Add("showing: " + (faces ? "faces " : "") + (edges ? "edges " : "") + (vertices ? "vertices" : "") +
                  (faces || edges || vertices ? "" : "(nothing - press V, E or F)"));
        lines.Add("The GPU only ever draws triangles; a polygon is split first.");
    }
}
