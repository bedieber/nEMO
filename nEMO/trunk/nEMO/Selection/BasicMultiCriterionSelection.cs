//=============================================================================
// Project  : nEMO .Net Evolutionary Multiobjective Optimization Framework
// File    : BasicMultiCriterionSelection.cs
// Author  : Bernhard Dieber (Bernhard.Dieber@uni-klu.ac.at)
// Copyright 2011 by Bernhard Dieber
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://nemo.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================
using System;
using System.Collections.Generic;
using nEMO.Algorithm;

namespace nEMO.Selection
{
    public class BasicMultiCriterionSelection : SelectionBase
    {


        public override void Select(List<IChromosome> oldPopulation, List<IChromosome> newPopulation, int startIndex, int length, System.Threading.AutoResetEvent are)
        {
            List<IChromosome> tmpList = new List<IChromosome>();
            int endIndex = startIndex + length;
            if (endIndex >= oldPopulation.Count)
                endIndex = oldPopulation.Count - 1;
            //for (int i = startIndex; i <= endIndex && i < oldPopulation.Count; i++)
            //{
            //    //if (oldPopulation[i].DecisionVector == null)
            //    oldPopulation[i].Evaluate(_ff);
            //}

            for (int i = startIndex; i <= endIndex && i < oldPopulation.Count; i++)
            {
                if (/*oldPopulation[i].DecisionVector[i] > 0 &&*/Array.TrueForAll(oldPopulation[i].DecisionVector, (d => d > 0)) && !tmpList.Contains(oldPopulation[i]) && !IsDominated(oldPopulation[i], oldPopulation))
                    tmpList.Add(oldPopulation[i]);

            }
            lock (newPopulation)
            {
                newPopulation.AddRange(tmpList);
            }
            are.Set();
        }

    }
}
