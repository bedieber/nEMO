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
    /// <summary>
    /// Base interface for all chromosomes.<br />
    /// A chromosome in evolutionary optimization represents the whole problem space of the optimization problem, thus all properties of the problem that may be altered in a mutation.
    ///  /// <remarks>
    /// A chromosome never contains any rating of the solutions fitness. It also has no logic included other than that needed for altering the problem space (i.e. mutating and performing crossover). The rating of a certain chromosome is done only by the <see cref="IFitnessFunction"/>.
    /// </remarks>
    /// </summary>
    public interface IChromosome
    {
        /// <summary>
        /// Generates this chromosome, i.e. it fills its properties with (possibly randomized) initial values
        /// </summary>
        void Generate();
        /// <summary>
        /// Performs a deep clone of the current chromosome.
        /// <remarks>When cloning a chromosome it is important to perform a deep copy, i.e. cloning also all reference objects within the chromosome if they are altered by mutation or crossover</remarks>
        /// </summary>
        /// <returns>The newly generated chromosome that is a deep copy of the current chromosome</returns>
        IChromosome Clone();
        /// <summary>
        /// Creates a new randomly initialized chromosome.
        /// </summary>
        /// <returns></returns>
        IChromosome CreateNew();
        /// <summary>
        /// Mutates this chromosome. 
        /// <remarks>Note, that the changes in a mutation should alsways be as small as possible to be able to explore the search space of an evolutionary optimization as deep as possible (if mutation is coarse, solutions might be skipped and not found)
        /// </remarks>
        /// </summary>
        void Mutate();
        /// <summary>
        /// Performs a crossover with another chromosome. This should modify the local parameters
        /// </summary>
        /// <param name="pair">The pair.</param>
        void Crossover(IChromosome pair);
        /// <summary>
        /// Evaluate the current chromosome using the provided FitnessFunction <paramref name="ff"/>. <see cref="IChromosome.DecisionVector"/> should be updated by this call.
        /// </summary>
        /// <param name="ff">The ff.</param>
        void Evaluate(IFitnessFunction ff);
        /// <summary>
        /// Gets the decision vector. This represents the fitness of the chromosome and is generated using a fitness function.
        /// </summary>
        double[] DecisionVector { get;}
    }
}