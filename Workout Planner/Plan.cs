using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Workout_Planner
{
    /// <summary>
    /// Represents a workout plan that manages a collection of unique exercises.
    /// </summary>
    /// <remarks>The Plan class provides methods to add and remove exercises by name, ensuring that each
    /// exercise is only included once in the workout plan. Exercises are stored in a dictionary keyed by their name,
    /// allowing for efficient lookup and management. This class is intended for internal use within the application and
    /// is not thread-safe.</remarks>
    [Serializable]
    internal class Plan 
    {
        [JsonInclude]
        public Dictionary<string, Exercise> WorkoutPlan { get; set; } = new Dictionary<string, Exercise>();

        [JsonInclude]
        public string PlanName { get; set; }

        /// <summary>
        /// Initializes a new instance of the Plan class.
        /// </summary>
        [JsonConstructor]       
        public Plan() 
        {
            PlanName = "Plan"; // change this later to be user defined
        }

        /// <summary>
        /// Adds a new exercise to the workout plan if an exercise with the same name does not already exist.
        /// </summary>
        /// <remarks>If an exercise with the same name is already present in the workout plan, the method
        /// does not add the exercise and writes a message to the console indicating the duplication.</remarks>
        /// <param name="exercise">The exercise to add to the workout plan. Must not be null and must have a unique name within the plan.</param>
        public void AddExercise(Exercise exercise)
        {
            if (!WorkoutPlan.ContainsKey(exercise.Name))
            {
                WorkoutPlan.Add(exercise.Name, exercise);
            }
            else
            {
                Console.WriteLine($"Exercise '{exercise.Name}' already exists in the workout plan.");
            }
        }

        /// <summary>
        /// Removes the specified exercise from the workout plan if it exists.
        /// </summary>
        /// <remarks>If the specified exercise is not found in the workout plan, a message is written to
        /// the console indicating that the exercise was not found.</remarks>
        /// <param name="exerciseName">The name of the exercise to remove from the workout plan. This parameter cannot be null or empty.</param>
        public void RemoveExercise(string exerciseName)
        {
            if (WorkoutPlan.ContainsKey(exerciseName))
            {
                WorkoutPlan.Remove(exerciseName);
            }
            else
            {
                Console.WriteLine($"Exercise '{exerciseName}' not found in the workout plan.");
            }
        }

        /// <summary>
        /// Gets the current workout plan as a dictionary that maps exercise names to their corresponding <see
        /// cref="Exercise"/> objects.
        /// </summary>
        /// <remarks>Callers should ensure that the workout plan is initialized before invoking this
        /// method to avoid receiving a <see langword="null"/> result.</remarks>
        /// <returns>A dictionary where each key is the name of an exercise and each value is the associated <see
        /// cref="Exercise"/> object. Returns <see langword="null"/> if the workout plan is not set.</returns>
        public Dictionary<string, Exercise>? GetPlanDictionary()
        {
            return WorkoutPlan;
        }
    }
}
