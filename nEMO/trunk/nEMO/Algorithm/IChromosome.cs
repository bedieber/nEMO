namespace nEMO.Algorithm
{
    public interface IChromosome
    {
        void Generate();
        IChromosome Clone();
        IChromosome CreateNew();
        void Mutate();
        void Crossover(IChromosome pair);
        void Evaluate(IFitnessFunction ff);
        double[] DecisionVector { get;}
    }
}