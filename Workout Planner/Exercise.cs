using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace Workout_Planner
{
    /// <summary>
    /// Represents a physical exercise, including its name, description, and whether it involves weightlifting.
    /// </summary>
    [Serializable]
    internal class Exercise : INotifyPropertyChanged
    {
        private string name;
        private string description;
        private bool isWeightLifting;
        private WeightType weightType;

        public event PropertyChangedEventHandler? PropertyChanged;

        [JsonInclude]
        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        [JsonInclude]
        public string Description
        {
            get => description;
            set { description = value; OnPropertyChanged(); }
        }

        [JsonInclude]
        public bool IsWeightLifting
        {
            get => isWeightLifting;
            set { isWeightLifting = value; OnPropertyChanged(); }
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WeightType WeightType
        {
            get => weightType;
            set { weightType = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Initializes a new instance of the Exercise class with the specified name, description, and weight lifting
        /// indicator.
        /// </summary>
        /// <param name="name">The name of the exercise. This value must be a non-empty string.</param>
        /// <param name="description">A brief description of the exercise, providing context or details about its execution.</param>
        /// <param name="isWeightLifting">A value indicating whether the exercise involves weight lifting.</param>
        /// <param name="weightType">The type of weight used for the exercise. Defaults to <see cref="WeightType.Bodyweight"/>.</param>
        [JsonConstructor]
        public Exercise(string name, string description, bool isWeightLifting, WeightType weightType = WeightType.Bodyweight)
        {
            this.name = name;
            this.description = description;
            this.isWeightLifting = isWeightLifting;
            this.weightType = isWeightLifting ? weightType : WeightType.Bodyweight;
        }

        public override string ToString() => $"{Name}: {Description} (Weightlifting: {IsWeightLifting})";

        /// <summary>
        /// Invokes the PropertyChanged event to notify subscribers that a property value has changed.
        /// Used by {x:Bind Mode=OneWay} bindings to update the UI when properties are modified in place.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed. This parameter is optional and is automatically
        /// populated by the compiler if not specified.</param>
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
