//=============================================================================
// Project  : nEMO .Net Evolutionary Multiobjective Optimization Framework
// File    : SelectionBase.cs
// Author  : Bernhard Dieber (Bernhard.Dieber@uni-klu.ac.at)
// Copyright 2011 by Bernhard Dieber
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://nemo.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================
using System.Collections.Generic;
using System.Threading;
using nEMO.Algorithm;

namespace nEMO.Selection
{
    public abstract class SelectionBase
    {
        /// <summary>
        /// Select individuals from oldPopulation (within startindex+length) and add to newPopulation
        /// Lock <paramref name="newPopulation"/> since this is meant to be executed by multiple threads
        /// Use <paramref name="are"/> to indicate that work is finished
        /// </summary>
        /// <param name="oldPopulation"></param>
        /// <param name="newPopulation"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <param name="are"></param>
        public abstract void Select(List<IChromosome> oldPopulation, List<IChromosome> newPopulation, int startIndex, int length, AutoResetEvent are);

        #region Helper Functions
        internal bool IsDominated(IChromosome chromosome, IList<IChromosome> population)
        {
            foreach (IChromosome other in population)
            {
                if (other == chromosome)
                    continue;
                if (IsDominated(chromosome, other))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsDominated(IChromosome subject, IChromosome other)
        {
            double[] subjectDV = subject.DecisionVector;
            double[] otherDV = other.DecisionVector;
            //if (subjectDV == null || otherDV == null || subjectDV.Length != otherDV.Length)
            //    throw new ArgumentException("Decision vector must have same length for all chromosomes");
            //if (subject == other)
            //    return false;
            bool isBetter = false;
            bool isWorse = false;
            for (int index = 0; index < subjectDV.Length; index++)
            {
                if (subjectDV[index] >otherDV[index])
                {
                    isBetter = true;
                }
                else if (subjectDV[index] < otherDV[index])
                {
                    isWorse = true;
                }
            }

            return !isBetter && isWorse;
        }
        #endregion
    }
}
