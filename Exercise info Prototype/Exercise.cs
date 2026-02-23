using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Exercise_info_Prototype
{
    /// <summary>
    /// Represents a physical exercise, including its name, description, and whether it involves weightlifting.
    /// </summary>
    [Serializable]
    internal class Exercise
    {
        [JsonInclude]
        public string Name { get; set; }
        
        [JsonInclude]
        public string Description { get; set; }
        
        [JsonInclude]
        public bool IsWeightLifting { get; set; } = false;

        [JsonInclude]
        public int Sets { get; set; }
        
        [JsonInclude]
        public int Reps { get; set; }

        /// <summary>
        /// Initializes a new instance of the Exercise class with the specified name, description, and weight lifting
        /// indicator.
        /// </summary>
        /// <remarks>Ensure that the provided parameters are valid before creating an instance. Supplying
        /// invalid or empty values may result in an improperly initialized object.</remarks>
        /// <param name="name">The name of the exercise. This value must be a non-empty string.</param>
        /// <param name="description">A brief description of the exercise, providing context or details about its execution.</param>
        /// <param name="isWeight">A value indicating whether the exercise involves weight lifting. Set to <see langword="true"/> if the
        /// exercise is a weight lifting exercise; otherwise, <see langword="false"/>.</param>
        [JsonConstructor]
        public Exercise(string name, string description, bool isWeightLifting)
        {
            Name = name;
            Description = description;
            IsWeightLifting = isWeightLifting;
        }

        /// <summary>
        /// Returns a string that represents the current exercise, including its name, description, and whether it is
        /// associated with weightlifting.
        /// </summary>
        /// <returns>A formatted string containing the exercise name, description, and a value indicating if the exercise is
        /// related to weightlifting.</returns>
        public override string ToString() => $"{Name}: {Description} (Weightlifting: {IsWeightLifting})";
    }
}
