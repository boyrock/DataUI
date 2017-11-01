using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataUI
{
    [Serializable]
    public class TestData
    {
        [Slider(0.0f,1.0f)]
        public float fv_slider;
        public float fv;
        public int iv;
        public string s;
        public Vector3 vec3v;
        public Vector2 vec2v;
        public Color colv;
        public TestEnum ev;
    }
    public enum TestEnum
    {
        A,
        B,
        C,
    }
}
