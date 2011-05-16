//=============================================================================
// Project  : nEMO .Net Evolutionary Multiobjective Optimization Framework
// File    : Optimizer.cs
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
using System.Threading;

namespace nEMO.Algorithm
{
    /// <summary>
    /// The basic class for single and multi-criterion optimization.<br />
    /// The <see cref="Optimizer"/> lets you configure and run a multicriterion optimization with exchangeable fitness function and selection.
    /// </summary>
    public class Optimizer
    {
        private readonly int _populationSize;


        private List<IChromosome> _population;
        private Random _rand = new Random((int)DateTime.Now.Ticks);
        private Selection.SelectionBase _selection;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Optimizer"/> should try to parallelize  selection and mutation.
        /// </summary>
        /// <value>
        ///   <c>true</c> if parallelization is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Parallelize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the crossover rate.
        /// </summary>
        /// <value>
        /// The crossover rate.
        /// </value>
        public float CrossoverRate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the mutation rate.
        /// </summary>
        /// <value>
        /// The mutation rate.
        /// </value>
        public float MutationRate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the best chromosome (the first chromosome in the population)
        /// </summary>
        public IChromosome BestChromosome
        {
            get
            {
                if (_population.Count > 0)
                    return _population[0];
                return null;
            }
        }

        /// <summary>
        /// Gets the population.
        /// </summary>
        public List<IChromosome> Population
        {
            get { return _population; }
        }


        /// <summary>
        /// Gets the fitness function.
        /// </summary>
        public IFitnessFunction FitnessFunction { get; private set; }

        /// <summary>
        /// Gets or sets the selection.
        /// </summary>
        /// <value>
        /// The selection.
        /// </value>
        public Selection.SelectionBase Selection
        {
            get { return _selection; }
            set { _selection = value; }
        }

        /// <summary>
        /// Gets the target size of the population.<br />
        /// The actual size of the population may be retrieved via the <see cref="Population"/> property.
        /// </summary>
        /// <value>
        /// The target size of the population.
        /// </value>
        public int PopulationSize
        {
            get { return _populationSize; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Optimizer"/> class.
        /// </summary>
        /// <param name="ancestor">The ancestor chromosome.</param>
        /// <param name="populationSize">Size of the population.</param>
        /// <param name="ff">The fitness function to use for chromosome fitness calculation.</param>
        /// <param name="selection">The selection to be used to select the best chromosomes.</param>
        public Optimizer(IChromosome ancestor, int populationSize, IFitnessFunction ff, Selection.SelectionBase selection)
        {
            if (ancestor == null) throw new ArgumentNullException("ancestor");
            if (ff == null) throw new ArgumentNullException("ff");
            if (selection == null) throw new ArgumentNullException("selection");

            _populationSize = populationSize;
            FitnessFunction = ff;
            Selection = selection;
            _population = new List<IChromosome>();
            ancestor.Evaluate(ff);
            _population.Add(ancestor);
            while (_population.Count < populationSize)
            {
                IChromosome newC = ancestor.Clone();
                newC.Generate();
                newC.Evaluate(ff);
                _population.Add(newC);
            }
            MutationRate = 0.08f;
            CrossoverRate = 0f;
        }

        /// <summary>
        /// Resets the random. May be used to reinitialize the <see cref="Optimizer"/>
        /// </summary>
        public void ReInitRandom()
        {
            _rand = new Random();
        }

        #region Genetic Algorithm
        /// <summary>
        /// Runs single epoch (mutate, crossover, select).
        /// </summary>
        public void RunEpoc()
        {
            RunEpoc(true);
        }

        /// <summary>
        /// Runs Runs single epoch (mutate, crossover, select).<br />
        /// Depending on <paramref name="enforcePopulationSize"/>, the selection phase will reduce the population to the given size
        /// </summary>
        /// <param name="enforcePopulationSize">if set to <c>true</c>the population will be inflated or reduced to the predefined <see cref="PopulationSize"/>.</param>
        public void RunEpoc(bool enforcePopulationSize)
        {
            int change = 0;
            do
            {
                int oldPopulationSize = _population.Count;
                Mutate();
                Crossover();
                change += Population.Count - oldPopulationSize;
                change--;
            } while (_population.Count < PopulationSize && change > 0);
            Select(enforcePopulationSize);
        }

        /// <summary>
        /// Performs selection
        /// </summary>
        /// <param name="enforcePopulationSize">if set to <c>true</c>the population will be inflated or reduced to the predefined <see cref="PopulationSize"/>.</param>
        private void Select(bool enforcePopulationSize)
        {
            List<IChromosome> newPopulation = new List<IChromosome>(PopulationSize);


            /*
             ** Parallelize selection
             */
            int numThreads = Environment.ProcessorCount;
            if (Parallelize && numThreads <= _population.Count)
            {
                int chunkSize = _population.Count / numThreads;
                AutoResetEvent[] ares = new AutoResetEvent[numThreads];
                for (int i = 0; i < numThreads; i++)
                {
                    ares[i] = new AutoResetEvent(false);
                    ThreadPool.QueueUserWorkItem(delegate(object index)
                                                     {
                                                         int j = (int)index;
                                                         Selection.Select(_population, newPopulation, j * chunkSize, chunkSize,
                                                                          ares[j]);
                                                     }, i);
                }
                AutoResetEvent.WaitAll(ares);
            }
            else
            {
                Selection.Select(_population, newPopulation, 0, _population.Count, new AutoResetEvent(false));
            }
            //newPopulation.Sort(
            //    delegate(IChromosome one, IChromosome two)
            //    {
            //        double[] oneDV = one.DecisionVector;
            //        double[] twoDV = two.DecisionVector;

            //        for (int i = 0; i < oneDV.Length; i++)
            //        {
            //            if (oneDV[i] < twoDV[i])
            //                return 1;
            //            if (oneDV[i] > twoDV[i])
            //                return -1;
            //        }
            //        return 0;
            //    });
            if (enforcePopulationSize)
            {
                //while (newPopulation.Count > _populationSize)
                //{
                //    newPopulation.RemoveAt(0);
                //}
                for (int i = 0; i < _population.Count && (newPopulation.Count < PopulationSize); i++)
                {
                    int index = _rand.Next(0, _population.Count);
                    //TODO possible endless loop
                    //if (/*_population[index].DecisionVector[0] == 0 ||*/ !newPopulation.Contains(_population[index]))
                        newPopulation.Add(_population[index]);
                }
            }
            _population = newPopulation;
            //_population.Sort(delegate(IChromosome one, IChromosome two)
            //{
            //    if (one.DecisionVector[0] < two.DecisionVector[0])
            //        return 1;
            //    if (one.DecisionVector[0] > two.DecisionVector[0])
            //        return -1;
            //    return 0;
            //});
        }

        /// <summary>
        /// Performs the crossover operation
        /// </summary>
        private void Crossover()
        {
            if (CrossoverRate == 0)
                return;
            if (PopulationSize <= 2) return;

            int numThreads = Environment.ProcessorCount;
            if (Parallelize && numThreads <= _population.Count * CrossoverRate)
            {
                int crossovers = (int)(_population.Count * CrossoverRate) / numThreads;
                AutoResetEvent[] ares = new AutoResetEvent[numThreads];
                for (int i = 0; i < numThreads; i++)
                {
                    ares[i] = new AutoResetEvent(false);
                    ThreadPool.QueueUserWorkItem(delegate(object index)
                                                     {
                                                         int idx = (int)index;
                                                         DoCrossover(crossovers, ares[idx]);
                                                     }, i);
                }
                AutoResetEvent.WaitAll(ares);
            }
            else
                DoCrossover((int)(_population.Count * CrossoverRate), null);
        }

        /// <summary>
        /// Performs mutation
        /// </summary>
        private void Mutate()
        {
            int numThreads = Environment.ProcessorCount;
            if (Parallelize && numThreads <= _population.Count * MutationRate)
            {
                int mutations = (int)(_population.Count * MutationRate) / numThreads;
                AutoResetEvent[] ares = new AutoResetEvent[numThreads];
                for (int i = 0; i < numThreads; i++)
                {
                    ares[i] = new AutoResetEvent(false);
                    ThreadPool.QueueUserWorkItem(delegate(object index)
                                                     {
                                                         int idx = (int)index;
                                                         DoMutate(mutations, ares[idx]);
                                                     }, i);
                }
                AutoResetEvent.WaitAll(ares);
            }
            else
                DoMutate((int)(_population.Count * MutationRate), null);
        }

        #endregion

        #region Utility methods
        private void DoMutate(int mutations, AutoResetEvent are)
        {
            if (mutations <= 1)
                mutations = 1;
            for (int i = 0; i < mutations; i++)
            {
                int index = _rand.Next(0, Math.Min(PopulationSize, _population.Count));
                IChromosome chromosome = _population[index].Clone();
                chromosome.Mutate();
                chromosome.Evaluate(FitnessFunction);
                lock (_population)
                {
                    if (!_population.Contains(chromosome))
                        _population.Add(chromosome);
                }
            }
            if (are != null)
                are.Set();
        }

        private void DoCrossover(int crossovers, AutoResetEvent are)
        {
            for (int i = 0; i < crossovers; i++)
            {
                int index = _rand.Next(0, _population.Count);
                int index2 = _rand.Next(0, _population.Count);
                while (index2 == index)
                    index2 = _rand.Next(0, _population.Count);
                IChromosome chromosome1 = _population[index].Clone();
                IChromosome chromosome2 = _population[index2];
                chromosome1.Crossover(chromosome2);
                //chromosome2.Crossover(chromosome1);
                chromosome1.Evaluate(FitnessFunction);
                //chromosome2.Evaluate(FitnessFunction);
                lock (_population)
                {
                    if (!_population.Contains(chromosome1))
                        _population.Add(chromosome1);
                }
                //if (!_population.Contains(chromosome2))
                //_population.Add(chromosome2);
            }
            if (are != null)
                are.Set();
        }
        #endregion
    }
}
