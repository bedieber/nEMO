using System.Collections.Generic;
using System.Linq;
using System.Threading;
using nEMO.Algorithm;

namespace nEMO.Selection
{
    public class EliteSelection : SelectionBase
    {
        private readonly SortedList<double, IChromosome> _elite;
        private readonly int _eliteSize;

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

        protected SortedList<double, IChromosome> InternalElite
        {
            get { return _elite; }
        }


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
