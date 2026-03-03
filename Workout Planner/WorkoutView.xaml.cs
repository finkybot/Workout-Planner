using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Workout_Planner
{
    public sealed partial class WorkoutView : UserControl
    {
        private ObservableCollection<Plan> plans = new();
        private ObservableCollection<Workout> workouts = new();
        private ObservableCollection<CurrentExercise> workoutExercises = new();
        private ObservableCollection<Exercise> planExercises = new();
        private bool isSyncingValues;

        /// <summary>
        /// Initializes a new instance of the WorkoutView class, setting up the user interface components and loading
        /// workout plans from the application data directory.
        /// </summary>
        /// <remarks>This constructor initializes the component and binds the PlanListView and
        /// PlanExercisesListView to their respective data sources. It retrieves the roaming application data path and
        /// constructs the path to the workout plans directory, loading the plans from that location.</remarks>
        public WorkoutView()
        {
            InitializeComponent();
            PlanListView.ItemsSource = plans;
            WorkoutListView.ItemsSource = workouts;
            WorkoutExercisesListView.ItemsSource = workoutExercises;
            PlanExercisesListView.ItemsSource = planExercises;

            string romingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); // Get the roaming application data path
            string planPath = Path.Combine(romingAppDataPath, "Workout\\Plan"); // Construct the full path to the plans directory
            string workoutPath = Path.Combine(romingAppDataPath, "Workout"); // Construct the full path to the workouts directory
            LoadPlans(planPath); // Load workout plans from the specified directory
            LoadWorkouts(workoutPath); // Load workouts from the specified directory
        }

        /// <summary>
        /// Handles the event that occurs when the selection changes in the plan list view, updating the displayed
        /// exercises to match the selected plan.
        /// </summary>
        /// <remarks>If no plan is selected, the list of exercises is cleared. When a plan is selected,
        /// its associated exercises are displayed.</remarks>
        /// <param name="sender">The source of the event, typically the plan list view control.</param>
        /// <param name="e">The event data that contains information about the selection change, including the items that were added or
        /// removed.</param>
        private void PlanListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ExerciseEditPanel.Visibility = Visibility.Collapsed;
            RemoveWorkoutButton.Visibility = Visibility.Collapsed;
            DescriptionPanel.Visibility = Visibility.Collapsed;
            planExercises.Clear();
            workoutExercises.Clear();
            WorkoutExercisesListView.Visibility = Visibility.Collapsed;

            if (PlanListView.SelectedItem is Plan selectedPlan)
            {
                CreateWorkoutButton.Visibility = Visibility.Visible;
                DetailListTitle.Text = "Plan Exercises";
                DetailListTitle.Visibility = Visibility.Visible;
                foreach (var exercise in selectedPlan.WorkoutPlan.Values)
                {
                    planExercises.Add(exercise);
                }
                PlanExercisesListView.Visibility = Visibility.Visible;
            }
            else
            {
                CreateWorkoutButton.Visibility = Visibility.Collapsed;
                DetailListTitle.Visibility = Visibility.Collapsed;
                PlanExercisesListView.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles the event that occurs when the selection changes in the workout list view, updating the displayed
        /// list of exercises to match the selected workout.
        /// </summary>
        /// <remarks>If no workout is selected, the list of exercises is cleared. Selecting a workout will
        /// display its associated exercises.</remarks>
        /// <param name="sender">The source of the event, typically the workout list view control.</param>
        /// <param name="e">The event data that contains information about the selection change, including the items that were added or
        /// removed.</param>
        private void WorkoutListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            workoutExercises.Clear();
            planExercises.Clear();
            ExerciseEditPanel.Visibility = Visibility.Collapsed;
            DescriptionPanel.Visibility = Visibility.Collapsed;
            PlanExercisesListView.Visibility = Visibility.Collapsed;

            if (WorkoutListView.SelectedItem is Workout selectedWorkout)
            {
                CreateWorkoutButton.Visibility = Visibility.Collapsed;
                RemoveWorkoutButton.Visibility = Visibility.Visible;
                DetailListTitle.Text = "Workout Exercises";
                DetailListTitle.Visibility = Visibility.Visible;
                foreach (var currentExercise in selectedWorkout.Exercises.Values)
                {
                    workoutExercises.Add(currentExercise);
                }
                WorkoutExercisesListView.Visibility = Visibility.Visible;
            }
            else
            {
                RemoveWorkoutButton.Visibility = Visibility.Collapsed;
                DetailListTitle.Visibility = Visibility.Collapsed;
                WorkoutExercisesListView.Visibility = Visibility.Collapsed;
                if (PlanListView.SelectedItem is Plan selectedPlan)
                {
                    CreateWorkoutButton.Visibility = Visibility.Visible;
                    DetailListTitle.Text = "Plan Exercises";
                    DetailListTitle.Visibility = Visibility.Visible;
                    foreach (var exercise in selectedPlan.WorkoutPlan.Values)
                    {
                        planExercises.Add(exercise);
                    }
                    PlanExercisesListView.Visibility = Visibility.Visible;
                }
            }
        }

        private void WorkoutListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DependencyObject? tapped = e.OriginalSource as DependencyObject;
            bool tappedOnItem = false;
            while (tapped != null && tapped != WorkoutListView)
            {
                if (tapped is ListViewItem)
                {
                    tappedOnItem = true;
                    break;
                }
                tapped = VisualTreeHelper.GetParent(tapped);
            }

            if (!tappedOnItem)
            {
                WorkoutListView.SelectedItem = null;
            }
        }

        private void PlanExercisesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlanExercisesListView.SelectedItem is Exercise selectedExercise)
            {
                DescriptionPanel.Visibility = Visibility.Visible;
                DescriptionTextBlock.Text = selectedExercise.Description;
            }
            else
            {
                DescriptionPanel.Visibility = Visibility.Collapsed;
                DescriptionTextBlock.Text = "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkoutExercisesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkoutExercisesListView.SelectedItem is CurrentExercise selected)
            {
                isSyncingValues = true;
                ExerciseNameTextBlock.Text = selected.Exercise.Name;
                SetsNumberBox.Value = selected.Sets;
                RepsNumberBox.Value = selected.Reps;

                if (selected.Exercise.IsWeightLifting)
                {
                    WeightPanel.Visibility = Visibility.Visible;
                    WeightNumberBox.Value = selected.Weight ?? 0;
                }
                else
                {
                    WeightPanel.Visibility = Visibility.Collapsed;
                }

                isSyncingValues = false;
                ExerciseEditPanel.Visibility = Visibility.Visible;
                CreateWorkoutButton.Visibility = Visibility.Collapsed;
                RemoveWorkoutButton.Visibility = Visibility.Collapsed;
                DescriptionPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                ExerciseEditPanel.Visibility = Visibility.Collapsed;
                if (PlanListView.SelectedItem is Plan)
                {
                    CreateWorkoutButton.Visibility = Visibility.Visible;
                }
                if (WorkoutListView.SelectedItem is Workout)
                {
                    RemoveWorkoutButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void ExerciseValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (!isSyncingValues)
            {
                if (WorkoutExercisesListView.SelectedItem is not CurrentExercise selected)
                    return;
                if (WorkoutListView.SelectedItem is not Workout selectedWorkout)
                    return;

                selected.Sets = (int)SetsNumberBox.Value;
                selected.Reps = (int)RepsNumberBox.Value;
                if (selected.Exercise.IsWeightLifting)
                {
                    selected.Weight = WeightNumberBox.Value;
                }

                SaveWorkout(selectedWorkout, selectedWorkout.FileName);
            }
        }


        private async void RemoveWorkoutButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (WorkoutListView.SelectedItem is not Workout selectedWorkout)
                return;

            ContentDialog dialog = new()
            {
                Title = "Remove Workout",
                Content = $"Are you sure you want to remove '{selectedWorkout.DisplayName}'?",
                PrimaryButtonText = "Remove",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.XamlRoot
            };

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    string roamingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string filePath = Path.Combine(roamingAppDataPath, "Workout", selectedWorkout.FileName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                workouts.Remove(selectedWorkout);
                workoutExercises.Clear();
                RemoveWorkoutButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Loads plan data from JSON files located in the specified directory.
        /// </summary>
        /// <remarks>If the directory does not exist, an error message is printed to the console. Each
        /// JSON file is expected to contain a serialized representation of a Plan object. Any errors encountered while
        /// reading files or deserializing JSON are caught and logged to the console.</remarks>
        /// <param name="directoryPath">The path to the directory containing JSON files representing plans. This directory must exist for the method
        /// to function correctly.</param>
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

        /// <summary>
        /// Loads workout data from JSON files located in the specified directory.
        /// </summary>
        /// <remarks>If the directory does not exist, an error message is printed to the console. Each
        /// JSON file is expected to represent a valid Workout object. Any errors encountered while reading or
        /// deserializing the files are logged to the console.</remarks>
        /// <param name="directoryPath">The path to the directory containing the JSON files representing workouts. This directory must exist for the
        /// method to function correctly.</param>
        private void LoadWorkouts(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                workouts.Clear();
                string[] files = Directory.GetFiles(directoryPath, "*.json");
                foreach (string file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var options = new JsonSerializerOptions
                        {
                            Converters = { new JsonStringEnumConverter() }
                        };
                        Workout? workout = JsonSerializer.Deserialize<Workout>(json, options);
                        if (workout != null)
                        {
                            workout.FileName = Path.GetFileName(file);
                            workouts.Add(workout);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading workout from {file}: {ex.Message}");
                    }
                }

                // Ensure workouts are displayed newest-first (latest date at top)
                SortWorkoutsDescending();
            }
            else
            {
                Console.WriteLine($"Directory '{directoryPath}' does not exist.");
            }
        }

        /// <summary>
        /// Sorts the internal workouts collection by Date descending so the newest workout appears first.
        /// </summary>
        private void SortWorkoutsDescending()
        {
            var sorted = workouts.OrderByDescending(w => w.Date).ToList();
            workouts.Clear();
            foreach (var w in sorted)
            {
                workouts.Add(w);
            }
        }

        /// <summary>
        /// Handles the click event for the Create Workout button, prompting the user to create a new workout plan based
        /// on the currently selected plan.
        /// </summary>
        /// <remarks>If a plan is selected in the PlanListView, a dialog is displayed allowing the user to
        /// confirm creation of a new workout plan using the selected plan's name. The user can choose to proceed or
        /// cancel the operation.</remarks>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private async void CreateWorkoutButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Plan? selectedPlan = PlanListView.SelectedItem as Plan;

            if (selectedPlan != null)
            {
                TextBox fileNameTextBox = new()
                {
                    PlaceholderText = "Enter workout file name"
                };

                CalendarDatePicker datePicker = new()
                {
                    Date = DateTimeOffset.Now,
                    PlaceholderText = "Select a date"
                };

                ContentDialog dialog = new()
                {
                    Title = "New Workout",
                    Content = new StackPanel
                    {
                        Spacing = 10,
                        Children =
                        {
                            new TextBlock { Text = $"Plan: {selectedPlan.PlanName}" },
                            new TextBlock { Text = "File Name:" },
                            fileNameTextBox,
                            new TextBlock { Text = "Workout Date:" },
                            datePicker
                        }
                    },
                    PrimaryButtonText = "Create",
                    SecondaryButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    IsPrimaryButtonEnabled = false,
                    XamlRoot = this.XamlRoot
                };

                fileNameTextBox.TextChanged += (s, args) =>
                {
                    dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(fileNameTextBox.Text);
                };

                ContentDialogResult result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary && datePicker.Date.HasValue && !string.IsNullOrWhiteSpace(fileNameTextBox.Text))
                {
                    string fileName = fileNameTextBox.Text.Trim() + ".json";
                    Workout newWorkout = new(selectedPlan);
                    newWorkout.Date = DateOnly.FromDateTime(datePicker.Date.Value.DateTime);
                    newWorkout.FileName = fileName;

                    // Iterate through each exercise in the selected plan and add it to the workout by checking earlier workout data for the same exercise. If no previous workout data exists for that exercise, if there is set the values to match the latest exercise,
                    // if not add it with default values.
                    foreach (var exercise in newWorkout.Exercises)
                    {
                        DateOnly dateOnly = newWorkout.Date.AddDays(-5); // Look back up to 5 days for previous workout data for this exercise, we will reset this for each exercise in the new workout plan

                        // Iterate through workouts and exercises to find the most recent workout that contains this exercise and use those values as the default for the new workout,
                        // but only if that workout is within the last 5 days. This allows us to pre-populate the new workout with recent values for each exercise, but not use old values that may no longer be relevant.
                        foreach (var workout in workouts)
                        {
                            foreach (var ex in workout.Exercises) // Iterate through each exercise in the workout
                            {
                                // Check if the exercise in the workout matches the current exercise we are adding to the new workout and if the workout date is within the last 5 days. If so, use those values as the default for the new workout.
                                if (workout.Date > dateOnly && ex.Value.Exercise.Name == exercise.Value.Exercise.Name)
                                {
                                    dateOnly = workout.Date; // Update the dateOnly to the date of the most recent workout that contains this exercise, so we only use values from the most recent workout within the last 5 days
                                    exercise.Value.Sets = ex.Value.Sets;
                                    exercise.Value.Reps = ex.Value.Reps;
                                    exercise.Value.Weight = ex.Value.Weight;
                                }
                            }
                        }
                    }

                    SaveWorkout(newWorkout, fileName);
                    workouts.Add(newWorkout);
                    // Keep collection sorted newest-first after adding
                    SortWorkoutsDescending();
                }
            }
        }

        /// <summary>
        /// Saves the specified workout data to a JSON file in the user's roaming application data directory.
        /// </summary>
        /// <remarks>The method creates the target directory if it does not already exist. The workout
        /// data is serialized to JSON format and written to a file named 'workout.json' in a 'Workout' subdirectory of
        /// the roaming application data folder.</remarks>
        /// <param name="workout">The workout object containing the data to be saved. This parameter cannot be null.</param>
        /// <returns>true if the workout was successfully saved; otherwise, false.</returns>
        private bool SaveWorkout(Workout workout, string workoutFileName)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                };
                string json = JsonSerializer.Serialize(workout, options);
                string roamingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(roamingAppDataPath, "Workout");
                Directory.CreateDirectory(filePath);
                string fullFilePath = Path.Combine(filePath, workoutFileName);
                File.WriteAllText(fullFilePath, json);
                Console.WriteLine("Workout saved to: " + fullFilePath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Deletes the workout file located in the user's roaming application data folder if it exists.
        /// </summary>
        /// <remarks>If the workout file does not exist, the method returns false without throwing an
        /// exception. Any exceptions encountered during the deletion process are caught and logged, and the method
        /// returns false in such cases.</remarks>
        /// <returns>true if the workout file was successfully deleted; otherwise, false.</returns>
        private bool DeleteWorkoutFile()
        {
            try
            {
                string roamingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(roamingAppDataPath, "Workout", "workout.json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine("Workout deleted: " + filePath);
                    return true;
                }
                else
                {
                    Console.WriteLine("No workout found to delete at: " + filePath);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}