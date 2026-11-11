using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The two meshes the W8 slides need to show: a low-poly UV sphere, built either with shared
/// (smooth) normals or with split vertices and one normal per face (flat). Both use exactly the
/// same positions and triangles, so a side-by-side comparison shows that the normals - not the
/// geometry - are what make a surface look faceted or smooth.
/// </summary>
public static class W08Mesh
{
    public static Mesh Sphere(int segments, int rings, bool flat)
    {
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        for (int r = 0; r <= rings; r++)
        {
            float v = (float)r / rings;
            float phi = v * Mathf.PI;
            for (int s = 0; s < segments; s++)
            {
                float u = (float)s / segments;
                float theta = u * Mathf.PI * 2f;
                verts.Add(new Vector3(Mathf.Sin(phi) * Mathf.Cos(theta), Mathf.Cos(phi),
                                      Mathf.Sin(phi) * Mathf.Sin(theta)) * 0.5f);
                uvs.Add(new Vector2(u, 1f - v));
            }
        }

        List<int> tris = new List<int>();
        for (int r = 0; r < rings; r++)
            for (int s = 0; s < segments; s++)
            {
                int a = r * segments + s;
                int b = r * segments + (s + 1) % segments;
                int c = (r + 1) * segments + s;
                int d = (r + 1) * segments + (s + 1) % segments;
                tris.Add(a); tris.Add(b); tris.Add(c);
                tris.Add(b); tris.Add(d); tris.Add(c);
            }

        // The winding formula above can point inward depending on the axis convention; check the
        // averaged normal against the position and flip if needed, rather than guessing.
        Vector3 a0 = verts[tris[0]], b0 = verts[tris[1]], c0 = verts[tris[2]];
        Vector3 n0 = Vector3.Cross(b0 - a0, c0 - a0);
        bool inward = Vector3.Dot(n0, (a0 + b0 + c0) / 3f) < 0f;
        if (inward) Flip(tris);

        if (!flat)
        {
            Mesh m = new Mesh { name = "W08 Sphere Smooth" };
            m.SetVertices(verts);
            m.SetUVs(0, uvs);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        List<Vector3> fv = new List<Vector3>();
        List<Vector3> fn = new List<Vector3>();
        List<Vector2> fu = new List<Vector2>();
        List<int> fi = new List<int>();
        for (int i = 0; i < tris.Count; i += 3)
        {
            Vector3 a = verts[tris[i]], b = verts[tris[i + 1]], c = verts[tris[i + 2]];
            Vector3 n = Vector3.Cross(b - a, c - a).normalized;
            if (Vector3.Dot(n, (a + b + c) / 3f) < 0f) n = -n;
            int baseIndex = fv.Count;
            fv.Add(a); fv.Add(b); fv.Add(c);
            fn.Add(n); fn.Add(n); fn.Add(n);
            fu.Add(uvs[tris[i]]); fu.Add(uvs[tris[i + 1]]); fu.Add(uvs[tris[i + 2]]);
            fi.Add(baseIndex); fi.Add(baseIndex + 1); fi.Add(baseIndex + 2);
        }

        Mesh flatMesh = new Mesh { name = "W08 Sphere Flat" };
        flatMesh.SetVertices(fv);
        flatMesh.SetNormals(fn);
        flatMesh.SetUVs(0, fu);
        flatMesh.SetTriangles(fi, 0);
        flatMesh.RecalculateBounds();
        return flatMesh;
    }

    private static void Flip(List<int> tris)
    {
        for (int i = 0; i < tris.Count; i += 3)
        {
            int t = tris[i + 1];
            tris[i + 1] = tris[i + 2];
            tris[i + 2] = t;
        }
    }
}
