using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts {
    public static class GeneticAlgorithm {
        public static float MutationProbability = 1f;//0.01f;

        public static CreatureBehaviour[] GetParents(List<CreatureBehaviour> candidates) {
            CreatureBehaviour parent1 = CalculateWinner(candidates);
            candidates.Remove(parent1);
            CreatureBehaviour parent2 = CalculateWinner(candidates);

            return new CreatureBehaviour[] {parent1, parent2};
        }

        // Selection
        //  1. Normalize each fitness (normFitness = fitness / sum(allFitnesses)), so that the sum of all fitnesses is 1
        //  2. Sort population desc fitness
        //  3. Accumulate normalized fitnesses (accFitness = normFitness + allPreviousNormFitnesses)
        //      ex: accFitness1 would be (normFitness1), accFitness2 would be (normFitness2 + normFitness1)
        //          accFitness3 would be (normFitness3 + normFitness2 + normFitness1) ...
        //  4. Choose random number R between 0 and 1
        //  5. Winner is the last one whose accFitness >= R
        // https://en.wikipedia.org/wiki/Selection_(genetic_algorithm)
        public static CreatureBehaviour CalculateWinner(List<CreatureBehaviour> candidates) {
            // Calculate total fitness of all candidates summed together
            float totalFitnesses = candidates.Select(x => x.Fitness).Sum();

            // Normalize fitnesses
            candidates.ForEach(x => x.NormalizedFitness = NormalizeFitness(x.Fitness, totalFitnesses));

            // Sort by descending fitness
            candidates = candidates.OrderByDescending(x => x.Fitness).ToList();

            // Accumulate normalized fitnesses
            foreach (CreatureBehaviour candidate in candidates) {
                int index = candidates.IndexOf(candidate);
                float[] previousFitnesses = candidates.GetRange(0, index).Select(x => x.NormalizedFitness).ToArray();
                candidate.AccumulatedNormalizedFitness = AccumulateNormalizedFitness(candidate.NormalizedFitness, previousFitnesses);
            }

            // Choose winner
            CreatureBehaviour winner = ChooseWinner(candidates.ToArray());

            return winner;
        }

        private static float NormalizeFitness(float fitness, float allFitnesses) {
            float normalizedFitness = fitness / allFitnesses;
            return normalizedFitness;
        }

        private static float AccumulateNormalizedFitness(float normFitness, float[] previousNormFitnesses) {
            float accFitness = normFitness + previousNormFitnesses.Sum();
            return accFitness;
        }

        private static CreatureBehaviour ChooseWinner(CreatureBehaviour[] candidates) {
            float random = Random.value;
            CreatureBehaviour winner = null;
            foreach (CreatureBehaviour candidate in candidates.Reverse()) // Wikipedia doesn't mention reversing the list, but if we don't then we will always pick the latest one in the list (with the highest accNormFitness)
                if (candidate.AccumulatedNormalizedFitness >= random)
                    winner = candidate;

            return winner;
        }

        //  Single point crossover
        //  1. Randomly pick a spot in the parents' genome
        //  2. Bits to the right of the point are swapped with the genome of the other parent
        // https://en.wikipedia.org/wiki/Crossover_(genetic_algorithm)
        public static BitArray Crossover(BitArray genomeParent1, BitArray genomeParent2) {
            BitArray newGenome = new BitArray(genomeParent1.Length);
            int randomIndex = Random.Range(0, genomeParent1.Length);
            for (int i = 0; i < newGenome.Length; i++) {
                if (i >= randomIndex)
                    newGenome.Set(i, genomeParent2.Get(i));
                else
                    newGenome.Set(i, genomeParent1.Get(i));
            }

            return newGenome;
        }

        // Mutation
        //  Flip Bit
        //  1. Randomly pick a spot in the child genome
        //  2. Flip the bit on that spot (0 => 1, 1 => 0)
        // https://en.wikipedia.org/wiki/Mutation_(genetic_algorithm)
        public static BitArray Mutation(BitArray genome) {
            BitArray newGenome = genome;
            if (Random.value <= MutationProbability)
                return genome; // No mutation has happened

            int randomIndex = Random.Range(0, genome.Length);
            newGenome.Set(randomIndex, !newGenome.Get(randomIndex));

            return newGenome;
        }
    }
}