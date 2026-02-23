using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using System.Text.Json.Serialization;

namespace Workout_Planner
{
    internal class CurrentExercise
    {
        [JsonInclude]
        public Exercise Exercise { get; set; }
        
        [JsonInclude]
        public int Sets { get; set; }
        
        [JsonInclude]
        public int Reps { get; set; }
        
        [JsonInclude]
        public double? Weight { get; set; }

        /// <summary>
        /// Initializes a new instance of the CurrentExercise class with the specified exercise, number of sets,
        /// repetitions, and optional weight.
        /// </summary>
        /// <remarks>If the specified exercise is not a weight lifting exercise, the weight parameter is
        /// ignored.</remarks>
        /// <param name="exercise">The exercise to associate with this instance. Must be a valid Exercise object.</param>
        /// <param name="sets">The number of sets to perform. Must be greater than zero; if a non-positive value is provided, the value
        /// defaults to 1.</param>
        /// <param name="reps">The number of repetitions per set. Must be greater than zero; if a non-positive value is provided, the value
        /// defaults to 6.</param>
        /// <param name="weight">The weight to use for the exercise, if applicable. This parameter is only used if the exercise is a weight
        /// lifting exercise; otherwise, it is ignored.</param>
        public CurrentExercise(Exercise exercise, int sets, int reps, double? weight = null)
        {
            Exercise = exercise;
            Sets = sets > 0 ? sets : 1;
            Reps = reps > 0 ? reps : 6;

            if (exercise.IsWeightLifting)
            {
                Weight = weight; 
            }
        }

        /// <summary>
        /// Returns a string that describes the exercise, including its name, description, number of sets, number of
        /// repetitions, and weight information if available.
        /// </summary>
        /// <remarks>If the exercise does not specify a weight, the returned string indicates that it is a
        /// bodyweight exercise.</remarks>
        /// <returns>A formatted string representation of the exercise, containing the exercise name, description, sets, reps,
        /// and either the weight in kilograms or an indication that the exercise is bodyweight.</returns>
        public override string ToString()
        {
            string weightInfo = Weight.HasValue ? $"Weight: {Weight.Value} kg" : "Bodyweight exercise";
            return $"{Exercise.Name}: {Exercise.Description} (Sets: {Sets}, Reps: {Reps}, {weightInfo})";
        }
    }
}
