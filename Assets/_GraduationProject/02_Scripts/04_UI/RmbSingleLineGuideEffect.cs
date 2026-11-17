using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reflows the existing 128px RMB keycap's two baked rows into one centered row.
/// The original Image, texture, material and dissolve animation remain in use.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public sealed class RmbSingleLineGuideEffect : BaseMeshEffect
{
    private Image _image;

    // Pixel rectangles include a transparent pixel around each authored glyph.
    // Texture coordinates start at the bottom; the PNG's top row is RM.
    private static readonly Rect[] Glyphs =
    {
        new Rect(27f, 78f, 31f, 36f), // R: PNG ink x28..56, y15..48
        new Rect(58f, 78f, 42f, 36f), // M: PNG ink x59..98, y15..48
        new Rect(50f, 14f, 29f, 36f)  // B: PNG ink x51..77, y79..112
    };

    public override void ModifyMesh(VertexHelper vertices)
    {
        if (!IsActive() || vertices.currentVertCount != 4) return;
        if (!_image) _image = GetComponent<Image>();
        Sprite sprite = _image ? _image.overrideSprite : null;
        if (!sprite || sprite.name != "rmb" || _image.type != Image.Type.Simple ||
            sprite.rect.width != 128f || sprite.rect.height != 128f) return;

        UIVertex bottomLeft = default, topLeft = default, topRight = default, bottomRight = default;
        vertices.PopulateUIVertex(ref bottomLeft, 0);
        vertices.PopulateUIVertex(ref topLeft, 1);
        vertices.PopulateUIVertex(ref topRight, 2);
        vertices.PopulateUIVertex(ref bottomRight, 3);
        Vector4 padding = UnityEngine.Sprites.DataUtility.GetPadding(sprite);
        var mapping = new Vector4(padding.x, padding.y,
            1f / Mathf.Max(1f, 128f - padding.x - padding.z),
            1f / Mathf.Max(1f, 128f - padding.y - padding.w));
        vertices.Clear();

        // Match LMB's 34px ink height and vertical center without scaling letters.
        float x = (128f - 102f) * .5f;
        const float y = (128f - 36f) * .5f;
        foreach (Rect source in Glyphs)
        {
            int start = vertices.currentVertCount;
            AddVertex(vertices, bottomLeft, topLeft, topRight, bottomRight, mapping, x, y, source.xMin, source.yMin);
            AddVertex(vertices, bottomLeft, topLeft, topRight, bottomRight, mapping, x, y + source.height, source.xMin, source.yMax);
            AddVertex(vertices, bottomLeft, topLeft, topRight, bottomRight, mapping, x + source.width, y + source.height, source.xMax, source.yMax);
            AddVertex(vertices, bottomLeft, topLeft, topRight, bottomRight, mapping, x + source.width, y, source.xMax, source.yMin);
            vertices.AddTriangle(start, start + 1, start + 2);
            vertices.AddTriangle(start + 2, start + 3, start);
            x += source.width;
        }
    }

    private static void AddVertex(VertexHelper vertices, UIVertex bl, UIVertex tl, UIVertex tr, UIVertex br,
        Vector4 mapping, float x, float y, float sourceX, float sourceY)
    {
        // Interpolate the Image's actual quad, including preserveAspect and atlas UVs.
        // Extra channels follow destination coordinates, while UV0 samples each glyph.
        UIVertex vertex = bl;
        float px = (x - mapping.x) * mapping.z, py = (y - mapping.y) * mapping.w;
        float ux = (sourceX - mapping.x) * mapping.z, uy = (sourceY - mapping.y) * mapping.w;
        vertex.position = Vector3.LerpUnclamped(Vector3.LerpUnclamped(bl.position, br.position, px),
            Vector3.LerpUnclamped(tl.position, tr.position, px), py);
        vertex.uv0 = Interpolate(bl.uv0, tl.uv0, tr.uv0, br.uv0, ux, uy);
        vertex.uv1 = Interpolate(bl.uv1, tl.uv1, tr.uv1, br.uv1, px, py);
        vertex.uv2 = Interpolate(bl.uv2, tl.uv2, tr.uv2, br.uv2, px, py);
        vertex.uv3 = Interpolate(bl.uv3, tl.uv3, tr.uv3, br.uv3, px, py);
        vertex.color = Color.LerpUnclamped(Color.LerpUnclamped(bl.color, br.color, px),
            Color.LerpUnclamped(tl.color, tr.color, px), py);
        vertex.normal = Vector3.LerpUnclamped(Vector3.LerpUnclamped(bl.normal, br.normal, px),
            Vector3.LerpUnclamped(tl.normal, tr.normal, px), py);
        vertex.tangent = Interpolate(bl.tangent, tl.tangent, tr.tangent, br.tangent, px, py);
        vertices.AddVert(vertex);
    }

    private static Vector4 Interpolate(Vector4 bl, Vector4 tl, Vector4 tr, Vector4 br, float x, float y)
    {
        return Vector4.LerpUnclamped(Vector4.LerpUnclamped(bl, br, x),
            Vector4.LerpUnclamped(tl, tr, x), y);
    }
}
