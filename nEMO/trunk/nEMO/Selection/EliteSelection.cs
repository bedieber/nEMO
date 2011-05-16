//=============================================================================
// Project  : nEMO .Net Evolutionary Multiobjective Optimization Framework
// File    : EliteSelection.cs
// Author  : Bernhard Dieber (Bernhard.Dieber@uni-klu.ac.at)
// Copyright 2011 by Bernhard Dieber
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://nemo.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using nEMO.Algorithm;

namespace nEMO.Selection
{
    /// <summary>
    /// A basic elitist selection. The elite selection keeps a dedicated list of non-dominated solutions that are found during the evolutionary optimization. This prevents that good solutions are overwritten during the process
    /// </summary>
    public class EliteSelection : SelectionBase
    {
        private readonly SortedList<double, IChromosome> _elite;
        private readonly int _eliteSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="EliteSelection"/> class.
        /// </summary>
        /// <param name="eliteSize">Size of the elite.</param>
        public EliteSelection(int eliteSize)
        {
            _eliteSize = eliteSize;
            _elite = new SortedList<double, IChromosome>(eliteSize);
        }

        /// <summary>
        /// List of chromosomes sorted descending
        /// </summary>
        public List<IChromosome> Elite
        {
            get { return new List<IChromosome>(InternalElite.Values.OrderByDescending(c => c.DecisionVector[0])); }
        }

        /// <summary>
        /// Gets the internal elite.
        /// </summary>
        protected SortedList<double, IChromosome> InternalElite
        {
            get { return _elite; }
        }


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
        public override void Select(List<IChromosome> oldPopulation, List<IChromosome> newPopulation, int startIndex, int length, AutoResetEvent are)
        {
            List<IChromosome> tmpList = new List<IChromosome>(startIndex + length);

            int endIndex = startIndex + length;
            if (endIndex >= oldPopulation.Count)
                endIndex = oldPopulation.Count - 1;

            for (int i = startIndex; i <= endIndex && i < oldPopulation.Count; i++)
            {
                //if (oldPopulation[i].DecisionVector == null)
                //    oldPopulation[i].Evaluate(_ff);
                if (oldPopulation[i].DecisionVector.All(d => d > 0))
                {
                    tmpList.Add(oldPopulation[i]);
                }
            }
            //tmpList = (from c in oldPopulation.GetRange(startIndex, length) where c.DecisionVector.All(d => d > 0) select c).ToList();


            //tmpList.Sort(delegate(IChromosome one, IChromosome two)
            //                 {
            //                     for (int i = 0; i < one.DecisionVector.Length; i++)
            //                     {
            //                         if (one.DecisionVector[i] < two.DecisionVector[i])
            //                             return 1;
            //                         if (one.DecisionVector[i] > two.DecisionVector[i])
            //                             return -1;
            //                     }
            //                     return 0;
            //                 });

            lock (InternalElite)
            {
                foreach (IChromosome c in tmpList)
                {
                    if (!InternalElite.ContainsKey(c.DecisionVector[1]))
                    {
                        InternalElite.Add(c.DecisionVector[1], c);
                    }

                }
                RemoveDominated();
                while (InternalElite.Count > _eliteSize)
                {
                    InternalElite.RemoveAt(0);
                }

                lock (newPopulation)
                {
                    newPopulation.AddRange(from c in InternalElite.Values where !newPopulation.Contains(c) select c);
                }
            }
            are.Set();
        }

        /// <summary>
        /// Removes dominated chromosomes from the elite.
        /// </summary>
        protected virtual void RemoveDominated()
        {
            IList<IChromosome> eliteChromosomes = InternalElite.Values;
            List<IChromosome> dominated = new List<IChromosome>(InternalElite.Values.Count);

            foreach (IChromosome chromosome in eliteChromosomes)
            {
                if (IsDominated(chromosome, eliteChromosomes))
                    dominated.Add(chromosome);
            }
            // (from ec in eliteChromosomes where IsDominated(ec, eliteChromosomes) select ec).ToList();
            dominated.ForEach(d => InternalElite.Remove(d.DecisionVector[1]));
        }
    }
}
