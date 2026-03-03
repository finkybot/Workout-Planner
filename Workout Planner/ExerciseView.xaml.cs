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
using System.Threading.Tasks;

namespace Workout_Planner
{
    public sealed partial class ExerciseView : UserControl
    {
        private ObservableCollection<Exercise> exercises = new();

        public event Action<bool>? SelectionChanged;


        /// <summary>
        /// Initializes a new instance of the ExerciseView class and loads the list of exercises from the user's
        /// application data directory.
        /// </summary>
        /// <remarks>This constructor sets the ItemsSource of the WorkoutListView to the exercises
        /// collection and attempts to load exercise data from a file located in the user's application data directory.
        /// Ensure that the application data directory exists and contains valid exercise data to avoid loading
        /// issues.</remarks>
        public ExerciseView()
        {
            InitializeComponent();

            // Set the ItemsSource of the ExerciseListView to the exercises collection
            ExerciseListView.ItemsSource = exercises;

            string romingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); // Get the path to the user's application data directory
            string filePath = Path.Combine(romingAppDataPath, "Workout\\Exercises"); // Construct the full path to the exercises directory
            LoadExercises(filePath); // Load exercises from the specified directory
        }

        /// <summary>
        /// Handles the Click event of the Add Exercise button by adding a new exercise with default values to the
        /// exercises collection.
        /// </summary>
        /// <remarks>Ensure that the exercises collection is initialized before invoking this method. This
        /// method is intended to be used as an event handler for the Add Exercise button in the user
        /// interface.</remarks>
        /// <param name="sender">The source of the event, typically the Add Exercise button that was clicked.</param>
        /// <param name="e">The event data associated with the button click.</param>
        private async void AddExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            Exercise newExercise = new("New Exercise", "Description here", false);
            exercises.Add(newExercise);
            ExerciseListView.SelectedItem = newExercise;
            await ShowEditExerciseDialog(newExercise);
        }

        /// <summary>
        /// Handles the Click event of the Remove Exercise button by removing the selected exercise from the workout
        /// list.
        /// </summary>
        /// <remarks>No action is taken if no exercise is selected in the workout list.</remarks>
        /// <param name="sender">The source of the event, typically the Remove Exercise button.</param>
        /// <param name="e">The event data associated with the Click event.</param>
        private async void RemoveExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExerciseListView.SelectedItem is Exercise selected)
            {
                ContentDialog dialog = new()
                {
                    Title = "Remove Exercise",
                    Content = $"Are you sure you want to remove '{selected.Name}'?",
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
                        // Match the save path and include the .json extension
                        string filePath = Path.Combine(roamingAppDataPath, "Workout", "Exercises", $"{selected.Name}.json");
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                            Console.WriteLine("Exercise file deleted: " + filePath);
                        }
                        else
                        {
                            Console.WriteLine("Exercise file not found: " + filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    exercises.Remove(selected); // Remove the selected exercise from the collection
                    ExerciseListView_SelectionChanged(null, null); // refresh UI state
                    //exercises.Clear();
                    //RemoveExerciseButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Displays a confirmation dialog to edit the currently selected exercise in the workout list. If confirmed,
        /// opens an edit dialog for the selected exercise.
        /// </summary>
        /// <remarks>If no exercise is selected, the method returns without performing any action. Any
        /// exceptions encountered during the operation are logged to the console. This method does not return a
        /// value.</remarks>
        public async void EditSelectedExercise()
        {
            // Guard: Check if an exercise is selected in the workout list
            if (ExerciseListView.SelectedItem is not Exercise selected)
            {
                return;
            }


            // Display a confirmation dialog to the user before proceeding with editing the selected exercise
            try
            {
                ContentDialog dialog = new() // Create a new ContentDialog instance to confirm the edit action
                {
                    Title = "Edit Workout",
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 12,

                        // The dialog content includes an important symbol icon and a text block that asks the user to confirm editing the selected exercise
                        Children =
                        {
                            new SymbolIcon { Symbol = Symbol.Important },
                            new TextBlock
                            {
                                Text = $"Are you sure you want to edit '{selected.Name}'?",
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
                    },
                    PrimaryButtonText = "OK", // Set the text for the primary button to "OK"
                    SecondaryButtonText = "Cancel", // Set the text for the secondary button to "Cancel"
                    DefaultButton = ContentDialogButton.Secondary, // Set the default button to the secondary button
                    XamlRoot = this.XamlRoot // Set the XamlRoot property to ensure the dialog is displayed correctly in the context of the current user control
                };

                ContentDialogResult result = await dialog.ShowAsync(); // Show the dialog asynchronously and await the user's response

                // If the user confirms the edit action by clicking the primary button, proceed to show the edit exercise dialog
                if (result == ContentDialogResult.Primary)
                {
                    await ShowEditExerciseDialog(selected);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Displays a dialog that allows the user to edit the details of the specified exercise.
        /// </summary>
        /// <remarks>The dialog enables modification of the exercise's name, description, weightlifting
        /// status, and weight type. Changes are applied only if the user confirms the dialog by selecting the Save
        /// button.</remarks>
        /// <param name="exercise">The exercise to be edited. This parameter must not be null and should represent a valid exercise instance.</param>
        /// <returns>A task that represents the asynchronous operation of showing the edit dialog. The task completes when the
        /// dialog is closed.</returns>
        private async Task ShowEditExerciseDialog(Exercise exercise)
        {

            // Create UI elements for editing the exercise details, including text boxes for the name and description, a toggle switch for weightlifting status, and a combo box for weight type selection
            TextBox nameTextBox = new() { Text = exercise.Name, PlaceholderText = "Exercise Name" };
            TextBox descriptionTextBox = new() { Text = exercise.Description, PlaceholderText = "Description", AcceptsReturn = true, TextWrapping = TextWrapping.Wrap };
            ToggleSwitch weightLiftingToggle = new() { IsOn = exercise.IsWeightLifting };

            // Initialize the weight type combo box with options based on the current weightlifting status of the exercise
            ComboBox weightTypeComboBox = new()
            {
                ItemsSource = GetWeightTypeOptions(exercise.IsWeightLifting),
                SelectedItem = exercise.WeightType,
                IsEnabled = exercise.IsWeightLifting
            };
            
            // Handle the toggled event of the weightlifting toggle switch to update the weight type combo box accordingly
            weightLiftingToggle.Toggled += (s, args) =>
            {
                bool isWeight = weightLiftingToggle.IsOn;
                weightTypeComboBox.IsEnabled = isWeight;
                weightTypeComboBox.ItemsSource = GetWeightTypeOptions(isWeight);
                weightTypeComboBox.SelectedItem = isWeight ? WeightType.Dumbbell : WeightType.Bodyweight;
            };

            // Create and configure the content dialog for editing the exercise details, including the title, content layout, and button texts
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
                PrimaryButtonText = "Save", // Set the text for the primary button to "Save"
                SecondaryButtonText = "Cancel", // Set the text for the secondary button to "Cancel"
                DefaultButton = ContentDialogButton.Primary, // Set the default button to the primary button
                XamlRoot = this.XamlRoot // Set the XamlRoot property to ensure the dialog is displayed correctly in the context of the current user control
            };

            ContentDialogResult editResult = await editDialog.ShowAsync(); // Show the edit dialog asynchronously and await the user's response

            // If the user confirms the changes by clicking the primary button, update the exercise details with the values from the dialog and save the changes
            if (editResult == ContentDialogResult.Primary)
            {
                exercise.Name = nameTextBox.Text;
                exercise.Description = descriptionTextBox.Text;
                exercise.IsWeightLifting = weightLiftingToggle.IsOn;
                exercise.WeightType = weightTypeComboBox.SelectedItem is WeightType selectedWeightType ? selectedWeightType : WeightType.Bodyweight;

                SaveExercise(exercise); // Save the updated exercise details to a file
                ExerciseListView_SelectionChanged(null, null); // Refresh the selection to update the displayed details in the UI
                Console.WriteLine($"Exercise '{exercise.Name}' updated successfully.");
            }
        }

        /// <summary>
        /// Retrieves the available weight type options based on the specified lifting context.
        /// </summary>
        /// <remarks>Use this method to obtain the appropriate set of weight type options for user
        /// selection, depending on whether the context involves weight lifting or not.</remarks>
        /// <param name="isWeightLifting">true to indicate a weight lifting context, which excludes the Bodyweight option; false to include all weight
        /// types.</param>
        /// <returns>An array of WeightType values representing the selectable weight type options. If isWeightLifting is true,
        /// the array excludes WeightType.Bodyweight.</returns>
        private static WeightType[] GetWeightTypeOptions(bool isWeightLifting)
        {
            // Retrieve the available weight type options based on the specified lifting context
            return Enum.GetValues<WeightType>()
                .Where(wtype => !isWeightLifting || wtype != WeightType.Bodyweight) // Filter out the Bodyweight option if isWeightLifting is true, we don't need it
                .ToArray();
        }

        /// <summary>
        /// Loads exercise data from JSON files located in the specified directory.
        /// </summary>
        /// <remarks>If the directory contains valid JSON files, each file is read and deserialized into
        /// an Exercise object, which is then added to the exercises collection. Any errors encountered during file
        /// reading or deserialization are logged to the console.</remarks>
        /// <param name="directoryPath">The path to the directory containing JSON files representing exercises. This directory must exist;
        /// otherwise, an error message is displayed.</param>
        private void LoadExercises(string directoryPath)
        {
            // Check if the specified directory exists before attempting to load exercises
            if (Directory.Exists(directoryPath))
            {
                exercises.Clear(); // Clear the existing exercises collection to prepare for loading new data
                string[] files = Directory.GetFiles(directoryPath, "*.json"); // Get all JSON files in the specified directory

                // Iterate through each JSON file and attempt to read and deserialize it into an Exercise object
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
        /// <remarks>If the target directory does not exist, it is created automatically. The exercise is
        /// serialized using indented JSON formatting, and any enum values are written as strings. If an error occurs
        /// during saving, the method returns false.</remarks>
        /// <param name="exercise">The exercise to save. Must not be null. The exercise's name is used as the filename.</param>
        /// <returns>true if the exercise was saved successfully; otherwise, false.</returns>
        private bool SaveExercise(Exercise exercise)
        {
            // Attempt to save the specified exercise to a JSON file in the user's application data directory
            try
            {
                // Configure JSON serialization options to use indented formatting and convert enum values to strings
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                string json = JsonSerializer.Serialize(exercise, options); // Serialize the exercise object to a JSON string using the specified options
                string romingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); // Get the path to the user's application data directory
                string filePath = Path.Combine(romingAppDataPath, "Workout\\Exercises"); // Construct the full path to the exercises directory
                Directory.CreateDirectory(filePath); // Ensure the directory exists before attempting to save the file
                string fullFilePath = Path.Combine(filePath, $"{exercise.Name}.json"); // Construct the full file path for the exercise JSON file
                File.WriteAllText(fullFilePath, json); // Write the serialized JSON to the file
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
        /// Handles the event that occurs when the selection changes in the workout list view, updating the display to
        /// reflect the details of the newly selected exercise or indicating when no exercise is selected.
        /// </summary>
        /// <remarks>If an exercise is selected, this method updates the relevant UI elements to display
        /// the exercise's name, description, weight lifting status, and weight type. If no exercise is selected, the UI
        /// is reset to indicate that no selection has been made. This method also invokes the SelectionChanged event
        /// with a value indicating whether an exercise is currently selected.</remarks>
        /// <param name="sender">The source of the event, typically the workout list view control that triggered the selection change.</param>
        /// <param name="e">An object that contains data about the selection change event, including information about the items that
        /// were selected or deselected.</param>
        private void ExerciseListView_SelectionChanged(object? sender, SelectionChangedEventArgs? e)
        {
            // Check if the selected item in the workout list view is an Exercise object and update the UI accordingly
            if (ExerciseListView.SelectedItem is Exercise selectedExercise)
            {
                ExerciseNameTextBlock.Text = selectedExercise.Name;
                ExerciseDescriptionTextBlock.Text = selectedExercise.Description;
                IsWeightLiftingTextBlock.Text = selectedExercise.IsWeightLifting ? "Yes" : "No";
                WeightTypeTextBlock.Text = selectedExercise.WeightType.ToString().Replace('_', ' ');
                SelectionChanged?.Invoke(true);
            }
            else // If no exercise is selected, reset the UI elements to indicate that no selection has been made
            {
                ExerciseNameTextBlock.Text = "No exercise selected";
                ExerciseDescriptionTextBlock.Text = "";
                IsWeightLiftingTextBlock.Text = "";
                WeightTypeTextBlock.Text = "";
                SelectionChanged?.Invoke(false);
            }
        }
        
        /// <summary>
        /// Handles the event that occurs when the exercise list view is tapped, allowing for deselection of items.
        /// </summary>
        /// <param name="sender">The source of the event, typically the exercise list view control that was tapped.</param>
        /// <param name="e">An object that contains data about the tap event, including information about the original source of the tap.</param>
        private void ExerciseListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Check if the tap occurred on a ListViewItem, and if not, deselect the current item
            DependencyObject? tapped = e.OriginalSource as DependencyObject;
            while (tapped != null && tapped != ExerciseListView)
            {
                // If the tapped element is a ListViewItem, we do not want to deselect the item, so we return early
                if (tapped is ListViewItem)
                {
                    return;
                }
                tapped = VisualTreeHelper.GetParent(tapped); // Traverse up the visual tree to check if any parent element is a ListViewItem
            }

            ExerciseListView.SelectedItem = null; // If the tap did not occur on a ListViewItem, deselect the current item by setting SelectedItem to null
        }
    }
}