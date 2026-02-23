using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;

namespace Exercise_info_Prototype
{
    /// <summary>
    /// Represents a workout session that manages a collection of exercises being performed, including their sets,
    /// repetitions, and optional weight information.
    /// </summary>
    internal class Workout
    {
        [JsonInclude]
        public Dictionary<string, CurrentExercise> Exercises { get; set; } = new Dictionary<string, CurrentExercise>();

        [JsonInclude]
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        public Workout() { }

        public Workout(Plan plan)
        {
            Dictionary<string, Exercise>? planExercises = plan.GetPlanDictionary();
            if (planExercises != null)
            {
                foreach (var exercise in planExercises.Values)
                {
                    if (exercise != null)
                    {
                        Exercises.Add(exercise.Name, new CurrentExercise(exercise, 3, 10, null));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current collection of exercises in the workout, mapped by exercise name.
        /// </summary>
        /// <remarks>Use this method to access the current state of all exercises in the workout. Check
        /// for <see langword="null"/> before accessing the dictionary to avoid exceptions.</remarks>
        /// <returns>A dictionary that maps exercise names to their corresponding <see cref="CurrentExercise"/> instances.
        /// Returns <see langword="null"/> if no exercises are available.</returns>
        public Dictionary<string, CurrentExercise>? GetWorkoutDictionary()
        {
            return Exercises;
        }

        /// <summary>
        /// Modifies the sets, repetitions, and weight for an existing exercise in the workout.
        /// </summary>
        /// <remarks>If the specified exercise does not exist in the workout, no changes are made and a
        /// message is written to the console.</remarks>
        /// <param name="exerciseName">The name of the exercise to modify. Must correspond to an existing exercise in the workout.</param>
        /// <param name="sets">The number of sets to assign to the exercise. If the value is greater than zero, the sets are updated;
        /// otherwise, the previous value is retained.</param>
        /// <param name="reps">The number of repetitions to assign to each set. If the value is greater than zero, the repetitions are
        /// updated; otherwise, the previous value is retained.</param>
        /// <param name="weight">The weight to assign for weightlifting exercises. If not specified, the existing weight remains unchanged.</param>
        public void ModifyExercise(string exerciseName, int sets, int reps, double? weight = null)
        {
            if (Exercises.ContainsKey(exerciseName))
            {
                CurrentExercise currentExercise = Exercises[exerciseName];
                currentExercise.Sets = sets > 0 ? sets : currentExercise.Sets; // Update sets if a valid value is provided
                currentExercise.Reps = reps > 0 ? reps : currentExercise.Reps; // Update reps if a valid value is provided
                if (currentExercise.Exercise.IsWeightLifting)
                {
                    currentExercise.Weight = weight; // Update weight for weightlifting exercises
                }
            }
            else
            {
                Console.WriteLine($"Exercise '{exerciseName}' not found in the workout.");
            }
        }

        /// <summary>
        /// Adds a new exercise to the workout with the specified number of sets, repetitions, and an optional weight.
        /// </summary>
        /// <remarks>If an exercise with the same name already exists in the workout, the method does not
        /// add it again and displays a message. Providing a valid weight marks the exercise as a weightlifting
        /// exercise.</remarks>
        /// <param name="exercise">The exercise to add to the workout. Must not be null and should have a unique name within the workout.</param>
        /// <param name="sets">The number of sets to perform for the exercise. Must be greater than zero.</param>
        /// <param name="reps">The number of repetitions to perform in each set. Must be greater than zero.</param>
        /// <param name="weight">The weight to use for the exercise, if applicable. Must be greater than zero if specified; otherwise, the
        /// exercise is not marked as weightlifting.</param>
        public void AddExercise(Exercise exercise, int sets, int reps, double? weight = null)
        {
            if (!Exercises.ContainsKey(exercise.Name))
            {
                if(weight != null && weight > 0)
                {
                    exercise.IsWeightLifting = true; // Mark the exercise as weightlifting if a valid weight is provided
                }
                
                Exercises.Add(exercise.Name, new CurrentExercise(exercise, sets, reps, weight));
            }
            else
            {
                Console.WriteLine($"Exercise '{exercise.Name}' already exists in the workout.");
            }
        }

        /// <summary>
        /// Removes the specified exercise from the workout by name, if it exists.
        /// </summary>
        /// <remarks>If the specified exercise is not found in the workout, a message is written to the
        /// console indicating that the exercise was not found.</remarks>
        /// <param name="exerciseName">The name of the exercise to remove from the workout. This parameter cannot be null or empty.</param>
        public void RemoveExercise(string exerciseName)
        {
            if (Exercises.ContainsKey(exerciseName))
            {
                Exercises.Remove(exerciseName);
            }
            else
            {
                Console.WriteLine($"Exercise '{exerciseName}' not found in the workout.");
            }
        }

       /// <summary>
       /// Displays the details of the workout, including all exercises it contains, to the console.
       /// </summary>
       /// <remarks>If the workout does not contain any exercises, a message indicating this is displayed
       /// instead.</remarks>
        public void PrintWorkout()
        {
            Console.WriteLine($"\n=== Workout ===");
            if (Exercises.Count == 0)
            {
                Console.WriteLine("No exercises in this workout.");
                return;
            }
            
            Console.WriteLine($"Date: {Date}");
            foreach (var exercise in Exercises.Values)
            {
                Console.WriteLine(exercise.ToString());
            }
        }
    }
}