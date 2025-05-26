using UnityEngine;

public static class Matrix4x4Extensions {
    public static Matrix4x4 MultiplyFloat(this Matrix4x4 m, float value) { 
        Matrix4x4 t = new Matrix4x4();
        t[0,0] = m[0,0] * value;
        t[0,1] = m[0,1] * value;
        t[0,2] = m[0,2] * value;
        t[0,3] = m[0,3] * value;

        t[1,0] = m[1,0] * value;
        t[1,1] = m[1,1] * value;
        t[1,2] = m[1,2] * value;
        t[1,3] = m[1,3] * value;

        t[2,0] = m[2,0] * value;
        t[2,1] = m[2,1] * value;
        t[2,2] = m[2,2] * value;
        t[2,3] = m[2,3] * value;

        t[3,0] = m[3,0] * value;
        t[3,1] = m[3,1] * value;
        t[3,2] = m[3,2] * value;
        t[3,3] = m[3,3] * value;
        return t;
    }

    public static Matrix4x4 ComponentAdd(this Matrix4x4 m, Matrix4x4 o) { 
        Matrix4x4 t = new Matrix4x4();
        t[0,0] = m[0,0] * o[0,0];
        t[0,1] = m[0,1] * o[0,1];
        t[0,2] = m[0,2] * o[0,2];
        t[0,3] = m[0,3] * o[0,3];

        t[1,0] = m[1,0] * o[1,0];
        t[1,1] = m[1,1] * o[1,1];
        t[1,2] = m[1,2] * o[1,2];
        t[1,3] = m[1,3] * o[1,3];

        t[2,0] = m[2,0] * o[2,0];
        t[2,1] = m[2,1] * o[2,1];
        t[2,2] = m[2,2] * o[2,2];
        t[2,3] = m[2,3] * o[2,3];

        t[3,0] = m[3,0] * o[3,0];
        t[3,1] = m[3,1] * o[3,1];
        t[3,2] = m[3,2] * o[3,2];
        t[3,3] = m[3,3] * o[3,3];
        return t;
    }
}