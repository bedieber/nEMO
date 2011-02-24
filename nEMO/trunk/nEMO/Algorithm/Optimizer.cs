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
    public class Optimizer
    {
        private readonly int _populationSize;


        private List<IChromosome> _population;
        private Random _rand = new Random((int)DateTime.Now.Ticks);
        private Selection.SelectionBase _selection;

        public bool Parallelize
        {
            get;
            set;
        }

        public float CrossoverRate
        {
            get;
            set;
        }

        public float MutationRate
        {
            get;
            set;
        }

        public IChromosome BestChromosome
        {
            get
            {
                if (_population.Count > 0)
                    return _population[0];
                return null;
            }
        }

        public List<IChromosome> Population
        {
            get { return _population; }
        }


        public IFitnessFunction FitnessFunction { get; private set; }

        public Selection.SelectionBase Selection
        {
            get { return _selection; }
            set { _selection = value; }
        }

        //MC Optimierung selbst bauen
        //Berechnung für Kosten und Coverage separat halten (Bereits im VideoSensor implementiert)
        //Genetische Funktionalität schreiben (Mutate, Crossover, Select)
        //Finde Vektor wo Coverage->max und Cost->min bzw 1/cost->max
        //Definition von Dominierung beachten (eines darf schlechter sein, wenn das andere dafür besser ist)
        //Verwenden von Sesorcoveragechromosome
        //Versch. Selektionsarten vorsehen (elite und nicht-elite)
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

        public void ReInitRandom()
        {
            _rand = new Random();
        }

        #region Genetic Algorithm
        public void RunEpoc()
        {
            RunEpoc(true);
        }

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
            } while (_population.Count < _populationSize && change > 0);
            Select(enforcePopulationSize);
        }

        private void Select(bool enforcePopulationSize)
        {
            List<IChromosome> newPopulation = new List<IChromosome>(_populationSize);


            /**
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
                for (int i = 0; i < _population.Count && (newPopulation.Count < _populationSize); i++)
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

        private void Crossover()
        {
            if (CrossoverRate == 0)
                return;
            if (_populationSize <= 2) return;

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
                int index = _rand.Next(0, Math.Min(_populationSize, _population.Count));
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
