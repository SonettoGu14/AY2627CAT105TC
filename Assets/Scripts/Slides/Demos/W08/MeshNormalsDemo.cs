using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Slide 5 - one mesh, two normal sets. The left sphere shares averaged vertex normals (smooth
/// shading); the right sphere has split vertices and one normal per face (flat shading). The
/// positions and triangles are identical, so any difference on screen comes from the normals alone.
/// N shows the normals themselves.
/// </summary>
public class MeshNormalsDemo : DemoBase
{
    public Transform[] spinners;
    public MeshFilter smoothFilter;
    public MeshFilter flatFilter;
    public Transform smoothNormalsRoot;
    public Transform flatNormalsRoot;

    private bool showNormals, paused;
    private float angle;
    private int smoothVerts, flatVerts, triangles;

    public override string Title { get { return "Normals decide flat or smooth shading"; } }

    public override string Keys { get { return "N  show the normals     Space  pause the spin"; } }

    private void Awake()
    {
        if (smoothFilter == null || flatFilter == null) return;
        Mesh smooth = W08Mesh.Sphere(10, 7, false);
        Mesh flat = W08Mesh.Sphere(10, 7, true);
        smoothFilter.sharedMesh = smooth;
        flatFilter.sharedMesh = flat;
        smoothVerts = smooth.vertexCount;
        flatVerts = flat.vertexCount;
        triangles = smooth.triangles.Length / 3;

        BuildNormals(smooth, smoothNormalsRoot, 14, false);
        BuildNormals(flat, flatNormalsRoot, 14, true);
        if (smoothNormalsRoot != null) smoothNormalsRoot.gameObject.SetActive(false);
        if (flatNormalsRoot != null) flatNormalsRoot.gameObject.SetActive(false);
    }

    public override void OnActivate()
    {
        showNormals = false; paused = false; angle = 0f;
        if (smoothNormalsRoot != null) smoothNormalsRoot.gameObject.SetActive(false);
        if (flatNormalsRoot != null) flatNormalsRoot.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            showNormals = !showNormals;
            if (smoothNormalsRoot != null) smoothNormalsRoot.gameObject.SetActive(showNormals);
            if (flatNormalsRoot != null) flatNormalsRoot.gameObject.SetActive(showNormals);
        }
        if (Input.GetKeyDown(KeyCode.Space)) paused = !paused;
        if (!paused)
        {
            angle += 16f * Time.deltaTime;
            if (spinners != null)
                foreach (Transform t in spinners) if (t != null) t.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("same sphere: 10 x 7 segments, " + triangles + " triangles");
        lines.Add("smooth  " + smoothVerts + " vertices, averaged normals");
        lines.Add("flat    " + flatVerts + " vertices, one normal per face");
        lines.Add("");
        lines.Add(showNormals ? "normals drawn: blue = the direction each surface points"
                              : "press N to draw the normals");
        lines.Add("Geometry and normals are separate data: only the normals changed here.");
    }

    private void BuildNormals(Mesh mesh, Transform root, int every, bool perFace)
    {
        if (root == null) return;
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.SetColor("_BaseColor", new Color(0.35f, 0.75f, 1f, 1f));

        Vector3[] v = mesh.vertices;
        Vector3[] n = mesh.normals;
        int[] t = mesh.triangles;
        int count = perFace ? t.Length / 3 : v.Length;
        for (int i = 0; i < count; i += every)
        {
            Vector3 pos, dir;
            if (perFace)
            {
                int a = t[i * 3], b = t[i * 3 + 1], c = t[i * 3 + 2];
                pos = (v[a] + v[b] + v[c]) / 3f;
                dir = n[a];
            }
            else
            {
                pos = v[i];
                dir = n[i];
            }
            GameObject go = new GameObject("Normal");
            go.transform.SetParent(root, false);
            go.transform.localPosition = Vector3.zero;
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.positionCount = 2;
            lr.SetPosition(0, pos);
            lr.SetPosition(1, pos + dir * 0.22f);
            lr.widthMultiplier = 0.012f;
            lr.startColor = lr.endColor = new Color(0.35f, 0.75f, 1f, 1f);
            lr.material = mat;
            lr.shadowCastingMode = ShadowCastingMode.Off;
            lr.receiveShadows = false;
        }
    }
}
