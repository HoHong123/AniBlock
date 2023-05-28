using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineParam {
    List<double> rho, ang;
    public LineParam()
    {
        rho = new List<double>();
        ang = new List<double>();
    }
    public void AddRho(double rho)
    {
        this.rho.Add(rho);
    }
    public void AddAng(double ang)
    {
        this.ang.Add(ang);
    }
    public int RhoLength()
    {
        return this.rho.Count;
    }
    public int AngLength()
    {
        return this.ang.Count;
    }
    public double GetRho(int idx)
    {
        return rho[idx];
    }
    public double GetAng(int idx)
    {
        return ang[idx];
    }
}
