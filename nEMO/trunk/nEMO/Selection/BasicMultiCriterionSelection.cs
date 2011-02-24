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
