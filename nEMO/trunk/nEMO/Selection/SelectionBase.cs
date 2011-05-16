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
    /// <summary>
    /// Base class for all Selection
    /// </summary>
    public abstract class SelectionBase
    {
        /// <summary>
        /// Select individuals from oldPopulation (within startindex+length) and add to newPopulation
        /// Lock <paramref name="newPopulation"/> since this is meant to be executed by multiple threads
        /// Use <paramref name="are"/> to indicate that work is finished using <c>are.Set()</c>
        /// </summary>
        /// <param name="oldPopulation">The old population</param>
        /// <param name="newPopulation">The new population filled in thís method (the result of the selection)</param>
        /// <param name="startIndex">The startindex where to start selection</param>
        /// <param name="length">The number of elements to perform selection on starting at <paramref name="startIndex"/></param>
        /// <param name="are">The AutoResetEvent to set after finishing</param>
        public abstract void Select(List<IChromosome> oldPopulation, List<IChromosome> newPopulation, int startIndex, int length, AutoResetEvent are);

        #region Helper Functions
        /// <summary>
        /// Determines whether the specified chromosome is dominated.
        /// </summary>
        /// <param name="chromosome">The chromosome.</param>
        /// <param name="population">The population.</param>
        /// <returns>
        ///   <c>true</c> if the specified chromosome is dominated; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Determines whether the specified subject is dominated.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="other">The other.</param>
        /// <returns>
        ///   <c>true</c> if the specified subject is dominated; otherwise, <c>false</c>.
        /// </returns>
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
