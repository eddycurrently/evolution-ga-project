using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithm
{
    // Problem Definition
    // Minimize:
    // f(x) = x
    // g(x, y) = 1 + y^2 - x - 0.1 * Math.Sin(3 * Math.Pi * x)

    // subjected to: 
    // 0 <= x <= 1
    // -2 <= y <= 2


    // gene containing value of type double
    class geneDouble
    {
        public double value;
        private double min;
        private double max;

        public geneDouble(double min, double max)
        {
            this.min = min;
            this.max = max;

            Random rng = new Random();
            this.value = rng.NextDouble() * (max - min) + min;
        }

        // Mutate value by adding a random value from 10% of the range around
        // the value. There is a 50 % chance of the mutation happening. If the
        // mutation causes the value to be too large or too small, clip the
        // value to the min/max.
        public void mutate()
        {
            Random rng = new Random();

            // Mutate with small value
            if(rng.Next(0, 2) == 0)
            {
                double difference = rng.NextDouble() * Math.Abs(max - min) / 1000;
                if(rng.Next(0, 2) == 0)
                {
                    value += difference;
                } else
                {
                    value -= difference;
                }
            }

            // Clip to min/max
            if(value > max) {
                value = max;
            }
            if(value < min) {
                value = min;
            }
        }
    
        // Swap the value of this gene with the value from another gene.
        // WARNING: does not check that min and max are the same.
        public void swapWith(ref geneDouble gene)
        {
            double temp = this.value;
            this.value = gene.value;
            gene.value = temp;
        }
    }

    // Class representing an individual for use with minimizing f(x)
    class IndividualF
    {
        public geneDouble x;
        public double result;

        public IndividualF()
        {
            x = new geneDouble(0, 1);
        }

        // Mutate genes
        public void mutate()
        {
            x.mutate();
        }
    
        // Recalculate result based on current genes
        public void updateResult()
        {
            result = x.value;
        }
    }

    // Class representing an individual for use with minimizing g(x, y)
    class IndividualG
    {
        public geneDouble x;
        public geneDouble y;
        public double result;

        public IndividualG()
        {
            x = new geneDouble(0, 1);
            y = new geneDouble(-2, 2);
        }

        // Mutate genes
        public void mutate()
        {
            x.mutate();
            y.mutate();
        }

        public List<IndividualG> crossover(IndividualG individual)
        {
            IndividualG a = this;
            Random rng = new Random();
            if(rng.Next(0, 2) == 0)
            {
                a.x.swapWith(ref individual.x);
            }
            if(rng.Next(0, 2) == 0)
            {
                a.y.swapWith(ref individual.y);
            }
            List<IndividualG> output = new List<IndividualG>();
            output.Add(a);
            output.Add(individual);
            return output;
        }

        // Recalculate result based on current genes
        public void updateResult()
        {
            result = 1 + (y.value * y.value) - x.value - 0.1 * Math.Sin(3 * Math.PI * x.value);
        }
    }

    class Program
    {
        // Number of individuals to use for each problem
        // NOTE: counts must be divisible by 10
        private const Int32 INDIVIDUAL_COUNT_F = 100;
        private const Int32 INDIVIDUAL_COUNT_G = 1000;
        private const double THRESHOLD = 0.0001;

        static void runForf()
        {
            Random rng = new Random();

            // Initialize starting population
            List<IndividualF> individuals = new List<IndividualF>();
            for(int index = 0; index < INDIVIDUAL_COUNT_F; index++)
            {
                individuals.Add(new IndividualF());
            }

            double min = Double.MaxValue;
            IndividualF minIndividual = new IndividualF();
            double change = Double.MaxValue;

            int generations = 0;
            
            // Run until change from last generation is less than the threshold.
            // NOTE: this is quite a simple cutoff and can lead to
            // sub-optimal/early completion
            while(change > THRESHOLD)
            {
                generations++;
                // Get results for current generation
                foreach(IndividualF individual in individuals)
                {
                    individual.updateResult();
                }

                // Use sort to order by fitness (that is, the lower the value,
                // the better the fitness)
                individuals.Sort((a, b) => a.result.CompareTo(b.result));

                // Store minimums and change in minimum from last generation
                change = Math.Abs(min - individuals[0].result);
                min = individuals[0].result;
                minIndividual = individuals[0];
                Console.WriteLine("generation: " + generations.ToString() + ", best result is: " + min.ToString());

                // NOTE: no crossover is performed as there is only one variable per individual

                // copy from last generation with a 90:10 split, that is, 90%
                // of the new individuals come from the fittest 10% of the old
                // generation, and the other new 10% come from the old 90%
                List<IndividualF> last_generation = new List<IndividualF>(individuals);
                individuals = new List<IndividualF>();
                for(int i = 0; i < INDIVIDUAL_COUNT_F * 0.9; i++)
                {
                    individuals.Add(last_generation[rng.Next(0, (int)(INDIVIDUAL_COUNT_F * 0.1))]);
                }
                for(int i = 0; i < INDIVIDUAL_COUNT_F * 0.1; i++)
                {
                    individuals.Add(last_generation[rng.Next((int)(INDIVIDUAL_COUNT_F * 0.1), INDIVIDUAL_COUNT_F)]);
                }

                // Mutate individuals
                foreach(IndividualF individual in individuals)
                {
                    individual.mutate();
                }
            }
            Console.WriteLine("ran for " + generations.ToString() + " generations, with best result: " + min.ToString() + ", when x = " + minIndividual.x.value.ToString());
        }


        static void runForg()
        {
            Random rng = new Random();

            // Initialize starting population
            List<IndividualG> individuals = new List<IndividualG>();
            for(int index = 0; index < INDIVIDUAL_COUNT_G; index++)
            {
                individuals.Add(new IndividualG());
            }

            double best = Double.MaxValue;
            double min = Double.MaxValue;
            IndividualG minIndividual = new IndividualG();
            double change = Double.MaxValue;

            int generations = 0;

            // Run until change from last generation is less than the threshold.
            // NOTE: This can lead to false positives where the slope is close
            // to 0.
            while (change > THRESHOLD)
            {
                generations++;
                // Get results for current generation
                foreach (IndividualG individual in individuals)
                {
                    individual.updateResult();
                }

                // Use sort to order by fitness (that is, the lower the value,
                // the better the fitness)
                individuals.Sort((a, b) => a.result.CompareTo(b.result));

                // Store minimums and change in minimum from last generation
                change = Math.Abs(min - individuals[0].result);
                min = individuals[0].result;
                if(min < best)
                {
                    best = min;
                }
                minIndividual = individuals[0];
                Console.WriteLine("generation: " + generations.ToString() + ", best result is: " + min.ToString());

                // copy from last generation with a 90:10 split, that is, 90%
                // of the new individuals come from the fittest 10% of the old
                // generation, and the other new 10% come from the old 90%
                List<IndividualG> last_generation = new List<IndividualG>(individuals);
                individuals = new List<IndividualG>();
                for(int i = 0; i < INDIVIDUAL_COUNT_G * 0.9; i++)
                {
                    individuals.Add(last_generation[rng.Next(0, (int)(INDIVIDUAL_COUNT_G * 0.1))]);
                }
                for(int i = 0; i < INDIVIDUAL_COUNT_G * 0.1; i++)
                {
                    individuals.Add(last_generation[rng.Next((int)(INDIVIDUAL_COUNT_G * 0.1), INDIVIDUAL_COUNT_G)]);
                }

                // Perform crossover on 1/5 of the population
                IEnumerable<int> randIndex = Enumerable.Range(0, INDIVIDUAL_COUNT_G).OrderBy(r => rng.Next());
                for (int i = 0; i < INDIVIDUAL_COUNT_G / 10; i++)
                {
                    int indexA = randIndex.ElementAt(2 * i);
                    int indexB = randIndex.ElementAt(2 * (i + 1));
                    List<IndividualG> newIndividuals = individuals[indexA].crossover(individuals[indexB]);
                    individuals[indexA] = newIndividuals[0];
                    individuals[indexB] = newIndividuals[1];
                }

                // Mutate individuals
                foreach(IndividualG individual in individuals)
                {
                    individual.mutate();
                }
            }
            Console.WriteLine("ran for " + generations.ToString() + " generations, with best result: " + min.ToString() + ", with x = " + minIndividual.x.value.ToString() + ", and y = " + minIndividual.y.value.ToString());
        }

        static void Main(string[] _)
        {
            Console.WriteLine("Running for f(x) = x");
            runForf();
            Console.WriteLine("Running for g(x, y) = 1 + y^2 - x - 0.1 * Math.Sin(3 * Math.Pi * x)");
            runForg();
        }
    }
}
