using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;

namespace Workout_Planner
{
    public sealed partial class MainWindow : Window
    {
        private ObservableCollection<Exercise> exercises = new();

        /// <summary>
        /// Initializes a new instance of the MainWindow class and sets up the UI.
        /// </summary>
        /// <remarks>Resizes the window to 800x600 pixels, binds the exercises collection to the ListView,
        /// and loads exercises from the user's application data directory.</remarks>
        public MainWindow()
        {
            InitializeComponent();
            this.AppWindow.ResizeClient(new Windows.Graphics.SizeInt32(1000, 800));
            WorkoutListView.ItemsSource = exercises;

            string romingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string filePath = Path.Combine(romingAppDataPath, "Workout\\Exercises");
            // Load exercises on startup
            LoadExercises(filePath);
        }

        /// <summary>
        /// Handles the Click event of the Add Exercise button to create a new exercise.
        /// </summary>
        /// <remarks>Creates a new exercise with default values and adds it to the exercises collection.</remarks>
        /// <param name="sender">The button that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void AddExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            // Add a new exercise or open a dialog
            exercises.Add(new Exercise("New Exercise", "Description here", false));
        }

        /// <summary>
        /// Handles the Click event of the Remove Exercise button to delete the selected exercise.
        /// </summary>
        /// <remarks>Removes the currently selected exercise from the collection if one is selected.</remarks>
        /// <param name="sender">The button that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void RemoveExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkoutListView.SelectedItem is Exercise selected)
            {
                exercises.Remove(selected);
            }
        }

        /// <summary>
        /// Handles the Click event of the Edit Exercise button to initiate editing of the selected exercise.
        /// </summary>
        /// <remarks>Displays a confirmation dialog before opening the edit exercise dialog. If the user confirms,
        /// the edit dialog is shown, allowing the user to modify the exercise's properties.</remarks>
        /// <param name="sender">The button that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private async void EditExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            // Ensure an exercise is selected before attempting to edit
            if (WorkoutListView.SelectedItem is not Exercise selected)
            {
                return;
            }

            try
            {
                // Show a confirmation dialog before opening the edit dialog
                ContentDialog dialog = new()
                {
                    Title = "Edit Workout",
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal, /* Display the icon and text side by side for better visual appeal */
                        Spacing = 12,
                        Children = /* Combine an important symbol with the confirmation message to emphasize the action's significance */
                        {
                            new SymbolIcon { Symbol = Symbol.Important },
                            new TextBlock 
                            { 
                                Text = $"Are you sure you want to edit '{selected.Name}'?",
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
                    },

                    // Configure the dialog buttons and root, setting the default to "Cancel" to prevent accidental edits and ensure the user consciously chooses to proceed with editing
                    PrimaryButtonText = "OK",
                    SecondaryButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Secondary,
                    XamlRoot = WorkoutListView.XamlRoot // Set the XamlRoot to ensure the dialog is properly associated to the current window
                };

                ContentDialogResult result = await dialog.ShowAsync(); // Await the user's response to the confirmation dialog

                if (result == ContentDialogResult.Primary)
                {
                    await ShowEditExerciseDialog(selected); // Open the edit dialog if the user confirms
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Displays a dialog allowing the user to edit the name, description, and weightlifting flag of an exercise.
        /// </summary>
        /// <remarks>Creates input controls for the exercise's editable properties and saves the changes to both the
        /// in-memory object and the JSON file if the user confirms.</remarks>
        /// <param name="exercise">The exercise to be edited.</param>
        private async Task ShowEditExerciseDialog(Exercise exercise)
        {
            // Create input controls pre-populated with the current exercise values
            // The weight type options will be dynamically updated based on the weightlifting toggle
            TextBox nameTextBox = new() { Text = exercise.Name, PlaceholderText = "Exercise Name" };
            TextBox descriptionTextBox = new() { Text = exercise.Description, PlaceholderText = "Description", AcceptsReturn = true, TextWrapping = TextWrapping.Wrap };
            ToggleSwitch weightLiftingToggle = new() { IsOn = exercise.IsWeightLifting };

            // Initialize the weight type ComboBox with options based on the current weightlifting status
            ComboBox weightTypeComboBox = new()
            {
                ItemsSource = GetWeightTypeOptions(exercise.IsWeightLifting),
                SelectedItem = exercise.WeightType,
                IsEnabled = exercise.IsWeightLifting
            };

            // Update weight type options when the weightlifting toggle is changed
            weightLiftingToggle.Toggled += (s, args) =>
            {
                bool isWeight = weightLiftingToggle.IsOn;
                weightTypeComboBox.IsEnabled = isWeight;
                weightTypeComboBox.ItemsSource = GetWeightTypeOptions(isWeight);
                weightTypeComboBox.SelectedItem = isWeight ? WeightType.Dumbbell : WeightType.Bodyweight;
            };

            // Create and show the edit dialog
            ContentDialog editDialog = new()
            {
                Title = "Edit Exercise Details",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock { Text = "Name:" },
                        nameTextBox,
                        new TextBlock { Text = "Description:" },
                        descriptionTextBox,
                        new TextBlock { Text = "Weightlifting:" },
                        weightLiftingToggle,
                        new TextBlock { Text = "Weight Type:" },
                        weightTypeComboBox
                    }
                },
                PrimaryButtonText = "Save", /* Label the primary button as "Save" */
                SecondaryButtonText = "Cancel", /* label the secondary button as "Cancel" */
                DefaultButton = ContentDialogButton.Primary, /* Set the default button to "Save" to encourage users to save changes */
                XamlRoot = WorkoutListView.XamlRoot /* Set the XamlRoot to ensure the dialog is properly associated to the current window */
            };

            ContentDialogResult editResult = await editDialog.ShowAsync(); // Await the user's response to the edit dialog

            // If the user clicks "Save", update the exercise properties and save the changes
            if (editResult == ContentDialogResult.Primary)
            {
                exercise.Name = nameTextBox.Text;
                exercise.Description = descriptionTextBox.Text;
                exercise.IsWeightLifting = weightLiftingToggle.IsOn;
                exercise.WeightType = weightTypeComboBox.SelectedItem is WeightType selectedWeightType ? selectedWeightType : WeightType.Bodyweight;

                // Save the updated exercise
                SaveExercise(exercise);

                // Refresh the display
                WorkoutListView_SelectionChanged(null, null);
                Console.WriteLine($"Exercise '{exercise.Name}' updated successfully.");
            }
        }

        /// <summary>
        /// Returns the available WeightType options based on whether the exercise involves weightlifting.
        /// </summary>
        /// <remarks>Excludes <see cref="WeightType.Bodyweight"/> when the exercise is a weightlifting exercise,
        /// since bodyweight is not a valid weight type for weighted exercises.</remarks>
        /// <param name="isWeightLifting">Whether the exercise involves weightlifting.</param>
        /// <returns>An array of <see cref="WeightType"/> values appropriate for the given context.</returns>
        private static WeightType[] GetWeightTypeOptions(bool isWeightLifting)
        {
            // Call a Lambda function to filter the WeightType enum values based on the weightlifting status and return the appropriate options for the ComboBox
            return Enum.GetValues<WeightType>()
                .Where(wtype => !isWeightLifting || wtype != WeightType.Bodyweight)
                .ToArray();
        }

        /// <summary>
        /// Loads all exercise JSON files from the specified directory into the exercises collection.
        /// </summary>
        /// <remarks>Clears the current collection and populates it with deserialized Exercise objects from .json files.
        /// If the directory does not exist, an error message is logged to the console.</remarks>
        /// <param name="directoryPath">The path to the directory containing exercise JSON files.</param>
        private void LoadExercises(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                exercises.Clear();
                string[] files = Directory.GetFiles(directoryPath, "*.json");
                foreach (string file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        Exercise? exercise = JsonSerializer.Deserialize<Exercise>(json);
                        if (exercise != null)
                        {
                            exercises.Add(exercise);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading exercise from {file}: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Directory '{directoryPath}' does not exist.");
            }
        }

        /// <summary>
        /// Saves the specified exercise to a JSON file in the user's application data directory.
        /// </summary>
        /// <remarks>The method serializes the exercise object to JSON format and writes it to a file
        /// named after the exercise's name. If the directory does not exist, it will be created. Any exceptions during
        /// the save process will be caught, and false will be returned.</remarks>
        /// <param name="exercise">The exercise object to be saved. Cannot be null.</param>
        /// <returns>true if the exercise was successfully saved; otherwise, false.</returns>
        private bool SaveExercise(Exercise exercise)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                };
                string json = JsonSerializer.Serialize(exercise, options);
                string romingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(romingAppDataPath, "Workout\\Exercises");
                Directory.CreateDirectory(filePath);
                string fullFilePath = Path.Combine(filePath, $"{exercise.Name}.json");
                File.WriteAllText(fullFilePath, json);
                Console.WriteLine("Exercise saved to: " + fullFilePath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the WorkoutListView to update the details panel.
        /// </summary>
        /// <remarks>When an exercise is selected in the ListView, this method populates the right panel with the
        /// exercise's name, description, and weightlifting status. If no exercise is selected, the details are cleared.</remarks>
        /// <param name="sender">The ListView control that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void WorkoutListView_SelectionChanged(object? sender, SelectionChangedEventArgs? e)
        {
            // Check if an exercise is selected and update the details panel accordingly
            if (WorkoutListView.SelectedItem is Exercise selectedExercise)
            {
                ExerciseNameTextBlock.Text = selectedExercise.Name;
                ExerciseDescriptionTextBlock.Text = selectedExercise.Description;
                IsWeightLiftingTextBlock.Text = selectedExercise.IsWeightLifting ? "Yes" : "No";
                WeightTypeTextBlock.Text = selectedExercise.WeightType.ToString().Replace('_', ' ');
                EditExerciseButton.Visibility = Visibility.Visible;
            }
            else // If no exercise is selected, clear the details panel and hide the edit button
            {
                ExerciseNameTextBlock.Text = "No exercise selected";
                ExerciseDescriptionTextBlock.Text = "";
                IsWeightLiftingTextBlock.Text = "";
                WeightTypeTextBlock.Text = "";
                EditExerciseButton.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles the Tapped event on the WorkoutListView to deselect the current item
        /// when the user taps on empty space within the list.
        /// </summary>
        /// <param name="sender">The ListView that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void WorkoutListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DependencyObject? tapped = e.OriginalSource as DependencyObject;
            while (tapped != null && tapped != WorkoutListView)
            {
                if (tapped is ListViewItem)
                {
                    return;
                }
                tapped = VisualTreeHelper.GetParent(tapped);
            }

            WorkoutListView.SelectedItem = null;
        }
    }
}
