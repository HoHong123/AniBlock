using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using OpenCvSharp;

public class BlockColor
{
    //////////////////////////////////// 
    /// INDEX INFORMATION
    /// RED : 0
    /// ORANGE : 1
    /// YELLOW : 2
    /// GREEN : 3
    /// BLUE : 4
    /// BROWN : 5
    //////////////////////////////////// 
    
    /*Lookup Table*/
    readonly float[] h = { 2.020833f, 10.5625f, 21.625f, 52.16667f, 95.77083f, 14.0313f };
    readonly float[] s = { 228.3542f, 239.75f, 228.7708f, 160.6458f, 221.3958f, 178.135f };
    readonly float[] v = { 111.875f, 166.3958f, 161.9375f, 104.3333f, 125.75f, 43.1875f };
    readonly float[] r = { 111.875f, 166.3958f, 161.9375f, 56.29167f, 16.91667f, 47.95833f };
    readonly float[] g = { 18.41667f, 65.41667f, 121.1667f, 104.3333f, 104.9375f, 28.10417f };
    readonly float[] b = { 11.29167f, 9.5625f, 16.54167f, 39.14583f, 125.75f, 16.41667f };
    /*end - Lookup Table*/

    /*Array - for voting*/
    int[] count = { 0, 0, 0, 0, 0, 0 };
    /*end - Array*/

    public BlockColor() { }
    public int Get_ArraySize()
    {
        return h.Length;
    }
    public float Get_H(int index)
    {
        return h[index];
    }
    public float Get_S(int index)
    {
        return s[index];
    }
    public float Get_V(int index)
    {
        return v[index];
    }
    public float Get_R(int index)
    {
        return r[index];
    }
    public float Get_G(int index)
    {
        return g[index];
    }
    public float Get_B(int index)
    {
        return b[index];
    }
    public void Counting(int index)
    {
        count[index]++;
    }
    public int Get_Max_Count()
    {
        int max_idx = 0;
        int max = count[max_idx];
        for (int i = 1; i < count.Length; i++)
        {
            if (max < count[i])
            {
                max = count[i];
                max_idx = i;
            }
        }
        return max_idx;
    }
    public void Reset_Count()
    {
        for (int i = 0; i < count.Length; i++)
        {
            count[i] = 0;
        }
    }
}