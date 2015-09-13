using System.Collections;
using UnityEngine;

// http://gamedev.stackexchange.com/a/96966
public class DebugLineRender : MonoBehaviour
{
    class Entry
    {
        public Vector3 start;
        public Vector3 end;
        public Color color;
    }

    static Entry[] entries = null;

    static Entry[] Entries
    {
        get
        {
            if (entries == null)
            {
                entries = new Entry[4096];
                for (int i = 0; i < entries.Length; i++)
                {
                    entries[i] = new Entry();
                }
            }
            return entries;
        }
    }

    static Material lineMaterial = null;
    static Material LineMaterial
    {
        get
        {
            if (lineMaterial == null)
            {
                lineMaterial = new Material(@"
                    Shader ""Lines/Colored Blended"" {
                        SubShader {
                            Pass {
                                Blend SrcAlpha OneMinusSrcAlpha
                                Lighting Off
                                ZTest Always
                                ZWrite Off
                                Cull Off
                                Fog { Mode Off }
                                BindChannels {
                                    Bind ""vertex"", vertex
                                    Bind ""color"", color
                                }
                            }
                        }
                    }"
                );
            }
            return lineMaterial;
        }
    }

    static bool initalized = false;
    static int count = 0;

    static void Init()
    {
        if (!initalized)
        {
            initalized = true;
            GameObject goCameraMain = GameMain.Instance.MainCamera.gameObject;
            goCameraMain.AddComponent<DebugLineRender>();
        }
    }

    DebugLineRender()
    {
        StartCoroutine(OnEndOfFrame());
    }

    public void OnPostRender()
    {
        Render();
    }

    IEnumerator OnEndOfFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            Clear();
        }
    }

    void Clear()
    {
        count = 0;
    }

    void Render()
    {
        if (count > 0)
        {
            GL.Begin(GL.LINES);
            LineMaterial.SetPass(0);
            for (int i = 0; i < count; i++)
            {
                Entry l = Entries[i];
                GL.Color(l.color);
                GL.Vertex3(l.start.x, l.start.y, l.start.z);
                GL.Vertex3(l.end.x, l.end.y, l.end.z);
            }
            GL.End();
        }
        Clear();
    }


    public static void Line(Vector3 start, Vector3 end, Color color)
    {
        if (!initalized)
        {
            Init();
        }
        if (count >= Entries.Length)
        {
            return;
        }
        if (start == null || end == null)
        {
            return;
        }
        Entry l = Entries[count++];
        l.start = start;
        l.end = end;
        l.color = color;
    }

    public static void Line(Transform transform, float length)
    {
        Line(transform.position, transform.TransformPoint(new Vector3(length, 0f, 0f)), Color.red);
        Line(transform.position, transform.TransformPoint(new Vector3(0f, length, 0f)), Color.green);
        Line(transform.position, transform.TransformPoint(new Vector3(0f, 0f, length)), Color.blue);
    }

    public static void Line(Transform transform)
    {
        Line(transform, 0.1f);
    }
}
