using System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Workout_Planner
{
    public sealed partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class and sets up the UI.
        /// </summary>
        /// <remarks>Resizes the window to 1000x800 pixels and loads the ExerciseView by default.</remarks>
        public MainWindow()
        {
            InitializeComponent();
            this.AppWindow.ResizeClient(new Windows.Graphics.SizeInt32(1000, 800));

            // Customize the title bar to blend with the app theme
            ExtendsContentIntoTitleBar = true;
            var titleBar = AppWindow.TitleBar;
            titleBar.BackgroundColor = Colors.Transparent;
            titleBar.InactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            // Show the Exercise view by default
            NavigateToExerciseView();
        }

        /// <summary>
        /// Handles the click event for the exercise navigation button and initiates navigation to the exercise view.
        /// </summary>
        /// <remarks>Use this method to respond to user interaction when navigating to the exercise view
        /// is required. This method is commonly used in user interface event handling scenarios.</remarks>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void ExerciseNav_Click(object sender, RoutedEventArgs e)
        {
            NavigateToExerciseView();
        }

        /// <summary>
        /// Handles the click event for the planner navigation button and updates the main content area to display the
        /// planner view.
        /// </summary>
        /// <remarks>This method also hides the edit button after switching to the planner view.</remarks>
        /// <param name="sender">The source of the event, typically the navigation button that was clicked.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void PlannerNav_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new PlannerView();
            HideEditButton();
        }

        /// <summary>
        /// Handles the click event for the workout navigation button by updating the main content area to display the
        /// workout view.
        /// </summary>
        /// <remarks>After navigating to the workout view, this method also hides the edit button to
        /// prevent editing in this context.</remarks>
        /// <param name="sender">The source of the event, typically the navigation button that was clicked.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void WorkoutNav_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new WorkoutView();
            HideEditButton();
        }

        /// <summary>
        /// Handles the Click event of the Edit Exercise button to initiate editing of the currently selected exercise
        /// in the content area, if available.
        /// </summary>
        /// <remarks>This method checks whether the content area contains an ExerciseView and, if so,
        /// calls its EditSelectedExercise method to allow editing of the selected exercise.</remarks>
        /// <param name="sender">The source of the event, typically the Edit Exercise button that was clicked.</param>
        /// <param name="e">The event data associated with the Click event.</param>
        private void EditExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContentArea.Content is ExerciseView exerciseView)
            {
                exerciseView.EditSelectedExercise();
            }
        }

        /// <summary>
        /// Navigates to the exercise view and updates the user interface to reflect the current selection state.
        /// </summary>
        /// <remarks>When this method is called, the exercise view is displayed in the content area. The
        /// visibility and interactivity of the edit exercise button are dynamically updated based on whether an item is
        /// selected in the exercise view. The edit button is hidden when navigating to the exercise view.</remarks>
        private void NavigateToExerciseView()
        {
            var view = new ExerciseView();
            view.SelectionChanged += (hasSelection) =>
            {
                EditExerciseButton.Opacity = hasSelection ? 1 : 0;
                EditExerciseButton.IsHitTestVisible = hasSelection;
            };
            ContentArea.Content = view;
            HideEditButton();
        }

        /// <summary>
        /// Hides the edit exercise button, making it invisible and unresponsive to user interaction.
        /// </summary>
        /// <remarks>Call this method to prevent users from editing exercises when editing is not
        /// permitted or applicable. The button remains in the visual tree but is not visible or interactive.</remarks>
        private void HideEditButton()
        {
            EditExerciseButton.Opacity = 0;
            EditExerciseButton.IsHitTestVisible = false;
        }

        /// <summary>
        ///  Exit the application after confirming the user's intent to close the app through a content dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a content dialog to confirm the user's intent to exit the application
            ContentDialog dialog = new()
            {
                Title = "Exit Application",
                Content = "Are you sure you want to exit?",
                PrimaryButtonText = "Exit",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = Content.XamlRoot
            };

            ContentDialogResult result = await dialog.ShowAsync(); // Show the dialog and wait for the user's response

            // If the user confirms the exit action by clicking the primary button, close the application
            if (result == ContentDialogResult.Primary)
            {
                Close();
            }
        }
    }
}
