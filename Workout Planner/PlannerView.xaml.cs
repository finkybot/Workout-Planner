using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace Workout_Planner
{
    public sealed partial class PlannerView : UserControl
    {
        private ObservableCollection<Plan> plans = new();
        private ObservableCollection<Exercise> exercises = new();
        public event Action<bool>? SelectionChanged;

        public PlannerView()
        {
            InitializeComponent();
            PlanListView.ItemsSource = plans;
            ExerciseListView.ItemsSource = exercises;

            string romingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string filePath = Path.Combine(romingAppDataPath, "Workout\\Plan");
            LoadPlans(filePath);
        }

        private void PlanListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            exercises.Clear();
            if (PlanListView.SelectedItem is Plan selectedPlan)
            {
                PlanNameTextBlock.Text = selectedPlan.PlanName;
                foreach (var exercise in selectedPlan.WorkoutPlan.Values)
                {
                    exercises.Add(exercise);
                }
                SelectionChanged?.Invoke(true);
            }
            else
            {
                PlanNameTextBlock.Text = "No plan selected";
                SelectionChanged?.Invoke(false);
            }
        }


        private void LoadPlans(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                plans.Clear();
                string[] files = Directory.GetFiles(directoryPath, "*.json");
                foreach (string file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        Plan? plan = JsonSerializer.Deserialize<Plan>(json);
                        if (plan != null)
                        {
                            plans.Add(plan);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading plans from {file}: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Directory '{directoryPath}' does not exist.");
            }
        }

    }
}   