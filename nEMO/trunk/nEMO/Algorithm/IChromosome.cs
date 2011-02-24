//=============================================================================
// Project  : nEMO .Net Evolutionary Multiobjective Optimization Framework
// File    : IChromosome.cs
// Author  : Bernhard Dieber (Bernhard.Dieber@uni-klu.ac.at)
// Copyright 2011 by Bernhard Dieber
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://nemo.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================
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