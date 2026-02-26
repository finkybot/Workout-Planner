using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.ApplicationModel.DataTransfer;

namespace Workout_Planner
{
    public sealed partial class PlannerView : UserControl
    {
        private ObservableCollection<Plan> plans = new();
        private ObservableCollection<Exercise> exercises = new();
        private ObservableCollection<Exercise> allExercises = new();
        private bool isSyncingExercises;
        public event Action<bool>? SelectionChanged;


        /// <summary>
        /// Initializes a new instance of the PlannerView class and sets up the data sources for the plan and exercise
        /// lists.
        /// </summary>
        /// <remarks>This constructor loads workout plans and exercises from the user's application data
        /// directory. The directories 'Workout\Plan' and 'Workout\Exercises' must exist within the application data
        /// path for plans and exercises to be loaded successfully.</remarks>
        public PlannerView()
        {
            InitializeComponent();

            // Set the data sources for the ListView controls to the corresponding ObservableCollection instances
            PlanListView.ItemsSource = plans;
            ExerciseListView.ItemsSource = exercises;
            AllExercisesListView.ItemsSource = allExercises;

            exercises.CollectionChanged += Exercises_CollectionChanged;

            string romingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); // Get the roaming application data path
            string planPath = Path.Combine(romingAppDataPath, "Workout\\Plan"); // Construct the full path to the plans directory
            string exercisePath = Path.Combine(romingAppDataPath, "Workout\\Exercises"); // Construct the full path to the exercises directory
            LoadPlans(planPath); // Load workout plans from the specified directory
            LoadExercises(exercisePath); // Load exercises from the specified directory
        }

        /// <summary>
        /// Handles the SelectionChanged event for the PlanListView ListView control. When a user selects a plan from the list,
        /// the ExerciseListView is updated with the exercises from the selected plan.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void PlanListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isSyncingExercises = true;
            exercises.Clear();
            if (PlanListView.SelectedItem is Plan selectedPlan)
            {
                PlanNameTextBlock.Text = selectedPlan.PlanName;
                foreach (var exercise in selectedPlan.WorkoutPlan.Values)
                {
                    exercises.Add(exercise);
                }
                SelectionChanged?.Invoke(true);
                DeletePlanButton.Visibility = Visibility.Visible;
            }
            else
            {
                PlanNameTextBlock.Text = "No plan selected";
                SelectionChanged?.Invoke(false);
                DeletePlanButton.Visibility = Visibility.Collapsed;
            }
            isSyncingExercises = false;
            UpdateAddExerciseButtonVisibility();
        }

        /// <summary>
        /// Handles the initiation of a drag-and-drop operation for items in the All Exercises ListView.
        /// </summary>
        /// <remarks>Sets the text data in the drag operation to the name of the exercise being dragged
        /// and specifies that the operation is a copy. This method is triggered when the user starts dragging items
        /// from the ListView.</remarks>
        /// <param name="sender">The source of the event, typically the ListView control that initiated the drag operation.</param>
        /// <param name="e">A DragItemsStartingEventArgs object that contains the event data, including the items being dragged and the
        /// data package for the drag operation.</param>
        private void AllExercisesListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items.Count > 0 && e.Items[0] is Exercise exercise)
            {
                e.Data.SetText(exercise.Name);
                e.Data.RequestedOperation = DataPackageOperation.Copy;
            }
        }

        /// <summary>
        /// Handles the drag-over event for the exercise list view, enabling copy operations when a plan is selected and
        /// text data is available.
        /// </summary>
        /// <remarks>This method allows users to copy text data into the exercise list view only when a
        /// plan is selected in the plan list view. If these conditions are not met, the drag operation is not
        /// accepted.</remarks>
        /// <param name="sender">The source of the event, typically the control that raised the drag-over event.</param>
        /// <param name="e">A DragEventArgs object that contains data about the drag-over event, including the data being dragged and
        /// the allowed operations.</param>
        private void ExerciseListView_DragOver(object sender, DragEventArgs e)
        {
            if (PlanListView.SelectedItem is Plan && e.DataView.Contains(StandardDataFormats.Text))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
        }

        /// <summary>
        /// Handles the drop event for the exercise list view, adding a selected exercise to the current plan if it is
        /// not already included.
        /// </summary>
        /// <remarks>This method checks whether a plan is selected and whether the dropped data contains a
        /// valid exercise name. If the exercise exists and is not already part of the current plan, it is added to both
        /// the exercise list and the selected plan.</remarks>
        /// <param name="sender">The source of the event, typically the exercise list view control that received the drop.</param>
        /// <param name="e">The event data containing information about the drag-and-drop operation, including the data being dropped.</param>
        private async void ExerciseListView_Drop(object sender, DragEventArgs e)
        {
            if (PlanListView.SelectedItem is not Plan selectedPlan)
                return;

            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                string exerciseName = await e.DataView.GetTextAsync();
                Exercise? match = allExercises.FirstOrDefault(ex => ex.Name == exerciseName);
                if (match != null && !exercises.Any(ex => ex.Name == exerciseName))
                {
                    exercises.Add(match);
                    selectedPlan.AddExercise(match);
                    SavePlan(selectedPlan);
                }
            }
        }

        /// <summary>
        /// Loads all exercise data from JSON files located in the specified directory into the exercise collection.
        /// </summary>
        /// <remarks>If the specified directory does not exist, an error message is written to the console
        /// and no exercises are loaded. Any errors encountered while reading files or deserializing JSON are logged to
        /// the console. Each JSON file is expected to contain a valid Exercise object.</remarks>
        /// <param name="directoryPath">The path to the directory containing JSON files that represent serialized Exercise objects. The directory
        /// must exist for exercises to be loaded.</param>
        private void LoadExercises(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                allExercises.Clear();
                string[] files = Directory.GetFiles(directoryPath, "*.json");
                foreach (string file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        Exercise? exercise = JsonSerializer.Deserialize<Exercise>(json);
                        if (exercise != null)
                        {
                            allExercises.Add(exercise);
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
        /// Saves the specified workout plan to a JSON file in the user's roaming application data directory.
        /// </summary>
        /// <remarks>The method creates the target directory if it does not exist and writes the plan to a
        /// file named 'plan.json'. If an error occurs during the save operation, the exception details are logged to
        /// the console.</remarks>
        /// <param name="plan">The workout plan to serialize and save. Cannot be null.</param>
        /// <returns>true if the plan was successfully saved; otherwise, false.</returns>
        static private bool SavePlan(Plan plan)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                };
                string json = JsonSerializer.Serialize(plan, options);
                string roamingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(roamingAppDataPath, "Workout\\Plan");
                Directory.CreateDirectory(filePath);
                string fullFilePath = Path.Combine(filePath, plan.PlanName + ".json");
                File.WriteAllText(fullFilePath, json);
                Console.WriteLine("Plan saved to: " + fullFilePath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Deletes the workout plan file with the specified name from the application's roaming data directory.
        /// </summary>
        /// <remarks>If the specified plan file does not exist, the method returns true and logs a message
        /// indicating that no plan was found to delete. Any exceptions encountered during deletion are caught, logged,
        /// and result in a return value of false.</remarks>
        /// <param name="planName">The name of the workout plan to delete, excluding the file extension. Cannot be null or empty.</param>
        /// <returns>true if the plan file was successfully deleted or if no file was found; otherwise, false.</returns>
        static private bool DeletePlan(string planName)
        {
            bool success = false;
            try
            {
                string roamingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(roamingAppDataPath, "Workout\\Plan", planName + ".json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine("Plan deleted: " + filePath);
                    success = true;
                }
                else
                {
                    Console.WriteLine("No plan found to delete at: " + filePath);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Handles the Tapped event of the PlanNameTextBlock control.
        /// This event is triggered when the user taps on the plan name text block, allowing them to edit the plan name.
        /// </summary>
        /// <param name="sender">The source of the event, typically the PlanNameTextBlock.</param>
        /// <param name="e">The event data associated with the tap event.</param>
        private void PlanNameTextBlock_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // Guard: Ensure that a plan is selected before allowing the user to edit the plan name. If no plan is selected, the method returns early and does not enable editing.
            if (PlanListView.SelectedItem is not Plan)
                return;

            // Set the text of the PlanNameTextBox to match the current text of the PlanNameTextBlock, then hide the text block and show the text box to allow editing. The text box is focused and all text is selected to facilitate immediate editing by the user.
            PlanNameTextBox.Text = PlanNameTextBlock.Text;
            PlanNameTextBlock.Visibility = Visibility.Collapsed;
            PlanNameTextBox.Visibility = Visibility.Visible;
            PlanNameTextBox.Focus(FocusState.Programmatic);
            PlanNameTextBox.SelectAll();
        }

        /// <summary>
        /// Handles key press events for the plan name text box, committing or canceling the current edit based on the
        /// key pressed.
        /// </summary>
        /// <remarks>Pressing the Enter key commits the current plan name edit, while pressing the Escape
        /// key cancels the edit.</remarks>
        /// <param name="sender">The source of the event, typically the plan name text box.</param>
        /// <param name="e">The event data containing information about the key that was pressed.</param>
        private void PlanNameTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            // If the user presses the Enter key, commit the current edit to save the new plan name. If the user presses the Escape key, cancel the edit and revert to the original plan name.
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                CommitPlanNameEdit(); // Commit the edit to save the new plan name
            }
            else if (e.Key == Windows.System.VirtualKey.Escape)
            {
                CancelPlanNameEdit(); // Cancel the edit and revert to the original plan name
            }
        }

        /// <summary>
        /// Handles the LostFocus event for the plan name text box, committing any changes made to the plan name when
        /// the control loses focus.
        /// </summary>
        /// <remarks>This method ensures that any edits to the plan name are saved when the user navigates
        /// away from the text box.</remarks>
        /// <param name="sender">The source of the event, typically the plan name text box that triggered the LostFocus event.</param>
        /// <param name="e">The event data associated with the LostFocus event.</param>
        private void PlanNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CommitPlanNameEdit(); // Commit the edit to save the new plan name when the text box loses focus

            // After committing the edit, attempt to save the updated plan. If the save operation is successful, log a success message to the console; otherwise, log a failure message.
            if (SavePlan((Plan)PlanListView.SelectedItem))
            {
                Console.WriteLine("Plan name updated and saved successfully.");
            }
            else
            {
                Console.WriteLine("Failed to save the updated plan name.");
            }
        }


        /// <summary>
        /// Commits any changes made to the name of the selected plan and updates the display to reflect the new name.
        /// </summary>
        /// <remarks>This method finalizes the editing of a plan's name by validating the input and
        /// updating both the underlying data and the user interface. If the input is valid, the plan's name is updated,
        /// the old plan file is deleted, and the edit controls are hidden. The UI controls are reset to their display
        /// state regardless of whether the validation succeeds. This method should be called after the user finishes
        /// editing a plan name to ensure consistency between the data and the UI.</remarks>
        /// <returns>true if the plan name was successfully validated and updated; otherwise, false.</returns>
        private bool CommitPlanNameEdit()
        {
            bool success = false;
            if (PlanListView.SelectedItem is Plan selectedPlan && !string.IsNullOrWhiteSpace(PlanNameTextBox.Text)) // Validate that a plan is selected and the new name is not empty or whitespace
            {
                string oldPlanName = selectedPlan.PlanName; // Store the old plan name for reference
                selectedPlan.PlanName = PlanNameTextBox.Text.Trim();
                PlanNameTextBlock.Text = selectedPlan.PlanName;
                if (SavePlan(selectedPlan)) // Save the updated plan with the new name
                {
                    DeletePlan(oldPlanName); // Remove the old plan file to prevent orphaned files
                    success = true;
                }
            }

            PlanNameTextBox.Visibility = Visibility.Collapsed;
            PlanNameTextBlock.Visibility = Visibility.Visible;
            return success;
        }
        
        /// <summary>
        /// Cancels the current edit of the plan name, reverting any changes made and restoring the original display.
        /// </summary>
        private void CancelPlanNameEdit()
        {
            PlanNameTextBox.Visibility = Visibility.Collapsed;
            PlanNameTextBlock.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the selection change event for the exercise list view, updating the visibility of the remove
        /// exercise button based on the currently selected item.
        /// </summary>
        /// <remarks>The remove exercise button is only visible when an exercise is selected from the
        /// list. This ensures users cannot attempt to remove an exercise unless one is actively selected.</remarks>
        /// <param name="sender">The source of the event, typically the ExerciseListView control that raised the event.</param>
        /// <param name="e">Provides data for the selection change event, including information about the newly selected and previously
        /// selected items.</param>
        private void ExerciseListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // The Remove Exercise button should only be visible if the selected item in the ExerciseListView is an Exercise. If no exercise is selected, or if the selected item is not an Exercise, the button should be hidden.
            RemoveExerciseButton.Visibility = ExerciseListView.SelectedItem is Exercise
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Update the exercise description text block to show the description of the currently selected exercise in the Exercise ListView. If no exercise is selected, clear the description.
            if (ExerciseListView.SelectedItem is Exercise selectedExercise)
            {
                UpdateExerciseDescription(selectedExercise); // Update the exercise description to show the description of the selected exercise
            }
            else
            {
                UpdateExerciseDescription(null); // Clear the exercise description if no exercise is selected
            }
        }

        /// <summary>
        /// Handles the click event for the Remove Exercise button, prompting the user for confirmation before removing
        /// the selected exercise from the selected plan.
        /// </summary>
        /// <remarks>If the user confirms the removal, the selected exercise is removed from the selected
        /// plan and the exercise list.</remarks>
        /// <param name="sender">The source of the event, typically the Remove Exercise button that was clicked.</param>
        /// <param name="e">The event data associated with the click event, providing additional information about the event.</param>
        private async void RemoveExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlanListView.SelectedItem is not Plan selectedPlan) // Ensure a plan is selected before attempting to remove an exercise
                return;

            if (ExerciseListView.SelectedItem is not Exercise selectedExercise) // Ensure an exercise is selected before attempting to remove it from the plan
                return;

            // Prompt the user with a confirmation dialog before removing the exercise from the plan
            ContentDialog dialog = new()
            {
                Title = "Remove Exercise",
                Content = $"Are you sure you want to remove '{selectedExercise.Name}' from '{selectedPlan.PlanName}'?",
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.XamlRoot
            };

            ContentDialogResult result = await dialog.ShowAsync(); // Show the dialog and wait for the user's response

            // If the user confirms the removal by clicking the primary button, remove the exercise from the selected plan and update the exercise list
            if (result == ContentDialogResult.Primary)
            {
                selectedPlan.RemoveExercise(selectedExercise.Name); // Remove the exercise from the selected plan's workout
                exercises.Remove(selectedExercise); // Remove the exercise from the observable collection to update the UI
                SavePlan(selectedPlan); // Save the updated plan to persist changes
            }
        }

        /// <summary>
        /// Handles the event that occurs when the selection changes in the AllExercisesListView control.
        /// </summary>
        /// <remarks>Updates the visibility of the add exercise button and displays the description of the
        /// selected exercise. If no exercise is selected, the description is cleared.</remarks>
        /// <param name="sender">The source of the event, typically the AllExercisesListView control.</param>
        /// <param name="e">A SelectionChangedEventArgs object that contains data about the selection change event.</param>
        private void AllExercisesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAddExerciseButtonVisibility(); // Update the visibility of the Add Exercise button based on the current selection of a plan and an exercise

            // Update the exercise description text block to show the description of the currently selected exercise in the All Exercises ListView. If no exercise is selected, clear the description.
            if (AllExercisesListView.SelectedItem is Exercise selectedExercise)
            {
                UpdateExerciseDescription(selectedExercise); // Update the exercise description to show the description of the selected exercise
            }
            else
            {
                UpdateExerciseDescription(null); // Clear the exercise description if no exercise is selected
            }
        }

        /// <summary>
        /// Updates the visibility of the Add Exercise to Plan button based on the current selection of a plan and an
        /// exercise.
        /// </summary>
        /// <remarks>The button is shown only when a plan is selected, an exercise is selected, and the
        /// selected exercise is not already part of the selected plan's workout. Otherwise, the button is hidden. This
        /// method should be called whenever the selection in the plan or exercise list changes to ensure the button's
        /// visibility reflects the current state.</remarks>
        private void UpdateAddExerciseButtonVisibility()
        {
            // The Add Exercise button should only be visible if a plan is selected, an exercise is selected, and the selected exercise is not already part of the selected plan's workout.
            if (PlanListView.SelectedItem is Plan selectedPlan
                && AllExercisesListView.SelectedItem is Exercise selectedExercise
                && !selectedPlan.WorkoutPlan.ContainsKey(selectedExercise.Name))
            {
                AddExerciseToPlanButton.Visibility = Visibility.Visible; // Show the Add Exercise button when all conditions are met
            }
            else
            {
                AddExerciseToPlanButton.Visibility = Visibility.Collapsed; // Hide the Add Exercise button when any condition is not met
            }
        }

        /// <summary>
        /// Handles the Click event for the Add Exercise to Plan button, adding the selected exercise to the currently
        /// selected workout plan if it is not already included.
        /// </summary>
        /// <remarks>This method requires both a workout plan and an exercise to be selected. If the
        /// selected exercise is not already part of the selected plan, it is added and the plan is saved. The
        /// visibility of the Add Exercise button is updated after the operation.</remarks>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="e">The event data associated with the Click event.</param>
        private void AddExerciseToPlanButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlanListView.SelectedItem is not Plan selectedPlan)
                return;

            if (AllExercisesListView.SelectedItem is not Exercise selectedExercise)
                return;

            // Check if the selected exercise is already part of the selected plan's workout. If not, add it to the plan and update the exercise list.
            if (!selectedPlan.WorkoutPlan.ContainsKey(selectedExercise.Name))
            {
                selectedPlan.AddExercise(selectedExercise); // Add the exercise to the selected plan's workout
                exercises.Add(selectedExercise); // Add the exercise to the observable collection to update the UI
                SavePlan(selectedPlan); // Save the updated plan after adding the new exercise
                UpdateAddExerciseButtonVisibility(); // Update the visibility of the Add Exercise button after adding the exercise to ensure it reflects the current state
            }
        }

        /// <summary>
        /// Handles changes to the exercises collection, syncing the reordered list back to the selected plan's
        /// WorkoutPlan dictionary and saving the updated plan.
        /// </summary>
        private void Exercises_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (isSyncingExercises)
                return;

            if (PlanListView.SelectedItem is not Plan selectedPlan)
                return;

            // Rebuild the dictionary in the new order
            selectedPlan.WorkoutPlan.Clear();
            foreach (var exercise in exercises)
            {
                selectedPlan.WorkoutPlan[exercise.Name] = exercise; // Add exercises back to the plan in the new order
            }
            SavePlan(selectedPlan); // Save the updated plan after syncing the reordered exercises
        }

        /// <summary>
        /// Updates the exercise description text block to display the description of the specified exercise.
        /// </summary>
        /// <param name="exercise">The exercise whose description will be displayed. If null, the description text block is cleared.</param>
        private void UpdateExerciseDescription(Exercise? exercise)
        {
            ExerciseDescriptionTextBlock.Text = exercise?.Description ?? ""; // Set the description text to the exercise's description if an exercise is provided; otherwise, clear the text block
        }

        /// <summary>
        /// Handles the Click event of the New Plan button by prompting the user to enter a plan name and creating a new
        /// plan if the input is valid.
        /// </summary>
        /// <remarks>If the user confirms the dialog and provides a non-empty plan name, a new plan is
        /// created, added to the plans collection, saved, and selected in the plan list. The dialog is dismissed if the
        /// user cancels or provides invalid input.</remarks>
        /// <param name="sender">The source of the event, typically the New Plan button that was clicked.</param>
        /// <param name="e">The event data associated with the Click event.</param>
        private async void NewPlanButton_Click(object sender, RoutedEventArgs e)
        {
            TextBox planNameInput = new() { PlaceholderText = "Enter plan name" }; // Create a TextBox for user input to enter the name of the new plan

            // Create a ContentDialog to prompt the user for the new plan name, with options to create or cancel
            ContentDialog dialog = new()
            {
                Title = "New Plan",
                Content = planNameInput,
                PrimaryButtonText = "Create",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            // Show the dialog and wait for the user's response. If the user clicks "Create" and provides a valid plan name, a new plan is created and added to the collection.
            ContentDialogResult result = await dialog.ShowAsync();

            // If the user confirms the creation of the new plan and the input is valid, create a new Plan object, add it to the plans collection, save it, and select it in the PlanListView.
            if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(planNameInput.Text))
            {
                Plan newPlan = new() { PlanName = planNameInput.Text.Trim() };
                plans.Add(newPlan);
                SavePlan(newPlan);
                PlanListView.SelectedItem = newPlan;
            }
        }

        /// <summary>
        /// Handles the Click event for the Delete Plan button, prompting the user for confirmation before deleting
        /// the selected plan.
        /// </summary>
        /// <remarks>If the user confirms the deletion, the plan is removed from the plan collection and the
        /// plan list view.</remarks>
        /// <param name="sender">The source of the event, typically the Delete Plan button that was clicked.</param>
        /// <param name="e">The event data associated with the Click event.</param>
        private async void DeletePlanButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlanListView.SelectedItem is not Plan selectedPlan)
                return;

            ContentDialog dialog = new()
            {
                Title = "Delete Plan",
                Content = $"Are you sure you want to delete the plan '{selectedPlan.PlanName}'?",
                PrimaryButtonText = "Delete",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.XamlRoot
            };

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (DeletePlan(selectedPlan.PlanName))
                {
                    plans.Remove(selectedPlan);
                    PlanListView.SelectedItem = null;
                }
            }
        }
    }
}