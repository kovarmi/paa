﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KnapsackProblem.Common;
using static KnapsackProblem.Common.ArgumentHelpers;
using static KnapsackProblem.Common.Helpers;

namespace KnapsackProblem.SimulatedEvolution
{
    class Program
    {
        /* args:
        [0] - a path to a test file
        [1] - a path to a solution file
        [2] - how many times repeat a test
        [3] - whether to save results        
        [4 ...] - GA arguments
        */
        static void Main(string[] args)
        {
            var genAlgArgs = ParseArguments(args);
            if (genAlgArgs == null)
            {
                Console.ReadLine();
                return;
            }

            var ga = new GeneticAlgorithm(genAlgArgs);
            var results = new List<KnapsackSolution>();
            foreach (var knapsack in KnapsackLoader.LoadKnapsacks(args[0], KnapsackLoader.KnapsackPerFile))
            {
                Console.WriteLine($"---Starting knapsack {knapsack.Id}---");
                results.Add(ga.Solve(knapsack));
            }

            if (bool.Parse(args[3]))
                SaveSolutions(results);

            var bestPrices = LoadBestPricesFromSolutionFile(args[1]);
            var relativeErrors = results.Select((result, index) => ComputeRelativeError(bestPrices[index], result.BestPrice)).ToList();
            Console.WriteLine($"Average relative error is {relativeErrors.Average()}.");
            Console.WriteLine($"Max relative error is {relativeErrors.Max()}.");

            Console.ReadLine();
        }

        static GeneticAlgorithmArgs ParseArguments(string[] args)
        {
            if (args.Length == 0 || args[0] == "-?" || args[0] == "/?")
            {
                PrintHelp();
                return null;
            }

            try
            {
                int populationSize = ParseInt32Option(args, "p", true, 0, int.MaxValue);
                int maxGenerations = ParseInt32Option(args, "g", true, int.MinValue, int.MaxValue);
                var parentSelection = (ParentSelection) ParseInt32Option(args, "ps", true, 0, 1);
                int tournamentSize = ParseInt32Option(args, "t", parentSelection == ParentSelection.Tournament, 0, populationSize);
                var popMngmnt = (PopulationManagement)ParseInt32Option(args, "pm", true, 0, 2);
                int elitesCount = ParseInt32Option(args, "e", popMngmnt == PopulationManagement.ReplaceAllButElites, 0, populationSize / 2);
                var mutateProb = ParseDoubleOption(args, "m", true, 0, 1);
                var printStatus = ParseInt32Option(args, "s", false, 0, 1);

                return new GeneticAlgorithmArgs(populationSize, maxGenerations, parentSelection, tournamentSize, popMngmnt, elitesCount, mutateProb, printStatus == 1);
            }
            catch { return null; }
        }

        static void PrintHelp()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Usage: -p size -g count -ps method -t size -pm method -e count -m probability [-s <0/1>]");
            sb.AppendLine();
            sb.AppendLine("Options:");
            sb.AppendOption("-p size", "Population size.");
            sb.AppendOption("-g count", "Total # of generations if possitive, stop when converged after # of generations if negative.");
            sb.AppendOption("-ps method", "Parent selection method. 0 - tournament, 1 - roulette wheel.");
            sb.AppendOption("-t size", "Tournament size.");
            sb.AppendOption("-pm method", "Population management method. 0 - replace all, 1 - replace all but elites, 2 - replace weakest");
            sb.AppendOption("-e count", "# of a population's fittest pass to the next generation.");
            sb.AppendOption("-m probability", "Probability that a single random offspring's gen is mutated.");
            sb.AppendOption("[-s <0/1>]", "Whether to print status with each new generation, 0 - no (default), 1 - yes.");
            Console.WriteLine(sb.ToString());
        }
    }
}
