using System;
using System.Collections.Generic;
using System.Linq;
using AForge;
using AForge.Genetic;
using AForge.Math;
using FSharp.Data.Runtime.StructuralTypes;

namespace Knapsack_Genetic_Algorithm
{
    class Program
    {
        public bool running = true;
        public decimal maxCarry = -1;
        public int itemCount = -1;
        public int populationSize = 0;
        public int generations = 0;
        decimal bestResult = 0;
        readonly int crossoverFactor = 2;
        readonly int mutateFactor = 9;
        decimal fitnessValue = 0;
        decimal finalWeight = 0;


        public Chromosome bestFit;

        public List<int> defaultList = new();
        public List<item> items = new();
        public List<Chromosome> Population = new();
        List<item> bestItemCombo = new();

        public Random random = new Random();
        


        static void Main(string[] args)
        {
            Program Current = new Program();

            Console.WriteLine("This is a program for solving the Knapsack problem.");

            while (Current.running == true)
            {
                Current.Initialise();
                Current.buildPopulation();
                Current.Evolution(Current.generations);
                Current.Post();
                
            }
        }

        public void Initialise()
        {
            //defines how much the user can carry and how many items there are

            Console.WriteLine("How much weight can you carry in kg?");
            maxCarry = Convert.ToDecimal(Console.ReadLine());
            Console.WriteLine("So you can carry " + maxCarry + "kg. How many items do you have to carry?");
            itemCount = Convert.ToInt32(Console.ReadLine());

            //Gets gets the weight for each item and uniquely identifies the item 
            foreach (int i in Enumerable.Range(1, (int)itemCount))
            {
                int tempId = items.Count + 1;
                Console.WriteLine("Item " + tempId + ": What is this item called?");
                string tempName = Console.ReadLine();
                Console.WriteLine("Item " + tempId + ": How much does this Item weigh?");
                decimal tempWeight = Convert.ToDecimal(Console.ReadLine());

                item newItem = new item(tempId, tempName, tempWeight);

                items.Add(newItem);
            }

            //Confirms the items were added correctly
            Console.WriteLine("Your items include:");
            foreach (int i in Enumerable.Range(0, items.Count))
            {
                Console.WriteLine(items[i].id + ", " + items[i].name + ", " + items[i].weight);
            }

            //Establishes the population size
            Console.WriteLine("What size population would you like to use?");
            populationSize = Convert.ToInt32(Console.ReadLine());

            //establishes how many times to run the program
            Console.WriteLine("How many generations would you like to run?");
            generations = Convert.ToInt32(Console.ReadLine());

        }


        public Chromosome ChromosomeTemplate()
        {
            List<int> tempChromosome = new List<int>();
            int tempId = Population.Count();
            foreach(int i in Enumerable.Range(1, items.Count))
            {
                tempChromosome.Add(0);               
            }
            Chromosome chromosome = new Chromosome(tempId, tempChromosome);
            return (chromosome);
        }

        public void ShuffleChromosome(Chromosome chromosome)
        {
            foreach(int i in Enumerable.Range(1, chromosome.chromosome.Count -1 ))
            {
                if (random.Next(2) == 1)
                {
                    chromosome.chromosome[i ] = 1;
                }
                else
                {
                    chromosome.chromosome[i] = 0;
                }
            }
        }

        public void buildPopulation()
        {
            foreach(var i in Enumerable.Range(1, populationSize))
            {
                Population.Add(ChromosomeTemplate());
            }
            
            foreach(var i in Enumerable.Range(0,Population.Count() -1 ))
            {
                ShuffleChromosome(Population[i]);

            }
        }




        public decimal FitnessFunction(Chromosome chromosome)
        {
            decimal weightInBag = 0;
            decimal numberOfItems = 0;

            decimal percentageItemsCarried = 0;
            decimal percentageWeightCarried = 0;

            decimal overallEffectiveness = 0;


            foreach(int i in Enumerable.Range(0, chromosome.chromosome.Count -1))
            {
                if(chromosome.chromosome[i] == 1)
                {
                    weightInBag += items[i].weight;
                    numberOfItems += 1;
                }
            }

            Console.WriteLine("Chromosome: "+ string.Join(", ", chromosome.chromosome));
            Console.WriteLine("Number of items: " + numberOfItems);

            //Checks there are items in the bag, and then sets a variable to see how effectively items are stored
            if(numberOfItems != 0)
            {
               percentageItemsCarried = numberOfItems / chromosome.chromosome.Count ;
            }
            else
            {
                return 0;
            }

            //Checks the weight of the bag is not 0, nor is it too heavy. Then sets a variable to check how effectively weight has been chosen
            if(weightInBag > maxCarry)
            {
                return 0;
            }

            if(weightInBag != 0)
            {
                percentageWeightCarried = weightInBag / maxCarry;
            }
            else
            {
                return 0;
            }
            
            overallEffectiveness = percentageWeightCarried * percentageItemsCarried;
            Console.WriteLine(percentageItemsCarried + " x " + percentageWeightCarried + " = " + overallEffectiveness);

            return (overallEffectiveness);


            
        }


        public void CrossOver(Chromosome chromosome)
        {
            foreach(int i in Enumerable.Range(0, items.Count - 1))
            {
                if(random.Next(crossoverFactor) == 0 && bestFit.chromosome[i] == 1)
                {
                    chromosome.chromosome[i] = bestFit.chromosome[i];
                }
            }
        }

        public void Evolution(int generations)
        {
            foreach(int x in Enumerable.Range(1, generations))
            {
                findBest();
                removeUnfit();
            }
            findBest();
        }

        public void removeUnfit()
        {
            foreach (var i in Enumerable.Range(0, Population.Count - 1))
            {
                decimal fitnessValue = FitnessFunction(Population[i]);

                if(fitnessValue < 0.1m)
                {
                    ShuffleChromosome(Population[i]);
                }
                else if (Population[i] != bestFit)
                {
                    switch (random.Next(3))
                    {
                        case 1:
                            Population[i].chromosome = bestFit.chromosome;
                            break;
                        case 2:
                            CrossOver(Population[i]);
                            break;
                        default:
                            ShuffleChromosome(Population[i]);
                            break;
                    }
                    RandomMutate(Population[i]);

                }
            }
        }

        public void findBest()
        {
            foreach (var i in Enumerable.Range(0, Population.Count - 1))
            {
                fitnessValue = FitnessFunction(Population[i]);


                if(fitnessValue > bestResult)
                {
                    bestResult = fitnessValue;
                    bestFit = new(Population[i].id, Population[i].chromosome);
                }
                
            }
        }
        
        public void Post()
        {
            Console.WriteLine("The final Best Chromosome is:" + string.Join(", ", bestFit.chromosome));
            foreach (int i in Enumerable.Range(0,items.Count - 1))
            {
                if(bestFit.chromosome[i] == 1)
                {
                    bestItemCombo.Add(items[i]);
                    finalWeight += items[i].weight;
                }

            }
            if(finalWeight > maxCarry | bestItemCombo.Count == 0 )
            {
                Console.WriteLine("No possible solution could be found");
            }
            else
            {
                Console.WriteLine("After " + generations + " generations, the most accurate result is:");
                foreach (int i in Enumerable.Range(0, (bestItemCombo.Count - 1)))
                {
                    Console.WriteLine("ID: " + bestItemCombo[i].id + ", Name: " + bestItemCombo[i].name + ", Weight: " + items[i].weight);

                }
                Console.WriteLine("Total Weight of: " + finalWeight);
            }



        }

        public void RandomMutate(Chromosome chromosome)
        {
            foreach (int i in Enumerable.Range(0, chromosome.chromosome.Count))
            {
                if(random.Next(mutateFactor) == 0)
                {
                    switch (chromosome.chromosome[i])
                    {
                        case 1:
                            chromosome.chromosome[i] = 0;
                            break;
                        default:
                            chromosome.chromosome[i] = 0;
                            break;
                    }
                }
            }
        }
    }



    class Chromosome
    {
        public int id;
        public List<int> chromosome;

        public Chromosome(int givenID, List<int> givenChromosome)
        {
            this.id = givenID;
            this.chromosome = givenChromosome;
        }
    }

    class item
    {
        public int id;
        public string name;
        public decimal weight;


        public item(int givenID, string givenName, decimal givenWeight)
        {
            this.id = givenID;
            this.name = givenName;
            this.weight = givenWeight;
        }
    } 

    

}
