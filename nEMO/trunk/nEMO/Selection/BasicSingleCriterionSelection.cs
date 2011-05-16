//=============================================================================
// Project  : nEMO .Net Evolutionary Multiobjective Optimization Framework
// File    : BasicSingleCriterionSelection.cs
// Author  : Bernhard Dieber (Bernhard.Dieber@uni-klu.ac.at)
// Copyright 2011 by Bernhard Dieber
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://nemo.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================
using System.Collections.Generic;
using nEMO.Algorithm;

namespace nEMO.Selection
{
    /// <summary>
    /// A base class for a selection in a single-criterion optimization context (i.e. optimization problems that have one optimization dimension).
    /// </summary>
    public class BasicSingleCriterionSelection:SelectionBase
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
        public override void Select(System.Collections.Generic.List<IChromosome> oldPopulation, System.Collections.Generic.List<IChromosome> newPopulation, int startIndex, int length, System.Threading.AutoResetEvent are)
        {
            List<IChromosome> tmpList = new List<IChromosome>(startIndex + length);

            int endIndex = startIndex + length;
            if (endIndex >= oldPopulation.Count)
                endIndex = oldPopulation.Count - 1;

            for (int i = startIndex; i <= endIndex && i < oldPopulation.Count; i++)
            {
                tmpList.Add(oldPopulation[i]);
            }

            tmpList.Sort(delegate(IChromosome one, IChromosome two)
            {
                if (one.DecisionVector[0] < two.DecisionVector[0])
                    return 1;
                if (one.DecisionVector[0] > two.DecisionVector[0])
                    return -1;
                return 0;
            });

            for(int i=0; i<tmpList.Count; i++)
            {
                lock (newPopulation)
                {
                    if(tmpList[i].DecisionVector[0]==0)
                        break;
                    if(!newPopulation.Contains(tmpList[i]))
                        newPopulation.Add(tmpList[i]);
                }
            }
            are.Set();
        }
    }
}