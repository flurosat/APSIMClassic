using System;
using System.Collections.Generic;
using System.Text;
using CSGeneral;

public class SIRIUSTuber : SIRIUSGenericOrgan, Reproductive, BelowGround
{
    public override double DMDemand
    {
        get
        {
            Function RelativeGrowthRate = Children["RelativeGrowthRate"] as Function;
            Function TubersPerStem = Children["TubersPerStem"] as Function;
            Leaf L = Plant.Children["Leaf"] as Leaf;
            Phenology P = Plant.Children["Phenology"] as Phenology;
            if (P.Between("FloralInitiation", "TuberInitiation") && (InitialWt == 0))
                InitialWt = 10;                                                      //This is to initiate tuber mass so relative growth rate can kick in
            double CurrentWt = Math.Max(InitialWt, Live.Wt);  
            return L.StemPopulation * TubersPerStem.Value * CurrentWt * RelativeGrowthRate.Value;
        }
    }
}



