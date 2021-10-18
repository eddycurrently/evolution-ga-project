using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithm
{
    class Individual1
    {
        public double x;
        public double result;

        public void mutate()
        {
            Random rng = new Random();

            // Mutate bits
            for(int index = 0; index < 64; index++)
            {
                x = (Int64)x ^ (rng.Next(0, 2) << index);
            }

            // Snap to boundary if outside
            if(x > 1) {
                x = 1;
            }
            if(x < 0) {
                x = 0;
            }
        }
    }
    class Program
    {
        // NOTE: count must be divisible by 10
        private const Int32 INDIVIDUAL_COUNT_1 = 100;
        private const double THRESHOLD_1 = 0.000001;
        // Problem Definition
        // Minimize:
        // f(x) = x
        // g(x, y) = 1 + y^2 - x - 0.1 * Math.Sin(3 * Math.Pi * x)

        // subjected to: 
        // 0 <= x <= 1
        // -2 <= y <= 2

        static double fn1(double individual)
        {
            return individual;
        }

        static void Main(string[] _)
        {
            Random rng = new Random();
            Console.WriteLine("Running for f(x) = x");

            // Initialize starting population
            List<Individual1> individuals1 = new List<Individual1>();
            for(int index = 0; index < INDIVIDUAL_COUNT_1; index++)
            {
                Console.WriteLine(index.ToString());
                Individual1 individual = new Individual1();
                individual.x = rng.NextDouble();
                individuals1.Add(individual);
            }

            double min = Double.MaxValue;
            double change = Double.MaxValue;

            int generations = 0;
            
            while(change > THRESHOLD_1)
            {
                generations++;
                // Get results for generation
                foreach(Individual1 individual in individuals1)
                {
                    individual.result = fn1(individual.x);
                }

                // Use sort as a fitness check
                individuals1.Sort((a, b) => a.x.CompareTo(b.x));
                change = Math.Abs(min - individuals1[0].result);
                min = individuals1[0].result;
                Console.WriteLine(min.ToString());

                // NOTE: no crossover is performed as there is only one variable per individual

                // perform mutation with a 90:10 split, that is, 90% of the new
                // individuals come from the fittest 10% of the old generation,
                // and the other new 10% come from the old 90%
                List<Individual1> last_generation = new List<Individual1>(individuals1);
                individuals1 = new List<Individual1>();
                for(int i = 0; i < INDIVIDUAL_COUNT_1 * 0.9; i++)
                {
                    individuals1.Add(last_generation[rng.Next(0, (int)(INDIVIDUAL_COUNT_1 * 0.1))]);
                }
                for(int i = 0; i < INDIVIDUAL_COUNT_1 * 0.1; i++)
                {
                    individuals1.Add(last_generation[rng.Next((int)(INDIVIDUAL_COUNT_1 * 0.1), INDIVIDUAL_COUNT_1)]);
                }
                foreach(Individual1 individual in individuals1)
                {
                    individual.mutate();
                }
            }
            Console.WriteLine("ran for " + generations.ToString() + " generations, with best result: " + min.ToString());
        }
    }
}
