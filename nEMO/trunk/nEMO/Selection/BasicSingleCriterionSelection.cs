using System.Collections.Generic;
using nEMO.Algorithm;

namespace nEMO.Selection
{
    public class BasicSingleCriterionSelection:SelectionBase
    {

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