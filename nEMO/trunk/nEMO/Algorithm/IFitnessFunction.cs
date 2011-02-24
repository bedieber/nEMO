namespace nEMO.Algorithm
{
    public interface IFitnessFunction
    {
        double[] Evaluate(IChromosome chromosome);
    }
}