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
    /// <summary>
    /// A basic selection for multi-criterion optimization problems, i.e. optimization problems with more than one dimensions. Those are the so called pareto optimization problems since typically not one single but multiple soltions must be considered optimal.
    /// </summary>
    public class BasicMultiCriterionSelection : SelectionBase
    {


        /// <summary>
        /// Select individuals from oldPopulation (within startindex+length) and add to newPopulation
        /// Lock <paramref name="newPopulation"/> since this is meant to be executed by multiple threads
        /// Use <paramref name="are"/> to indicate that work is finished
        /// </summary>
        /// <param name="oldPopulation">The old population</param>
        /// <param name="newPopulation">The new population filled in thís method (the result of the selection)</param>
        /// <param name="startIndex">The startindex where to start selection</param>
        /// <param name="length">The number of elements to perform selection on starting at <paramref name="startIndex"/></param>
        /// <param name="are">The AutoResetEvent to set after finishing</param>
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
