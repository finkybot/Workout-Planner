using Microsoft.UI.Xaml;

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

            // Show the Exercise view by default
            NavigateToExerciseView();
        }

        private void ExerciseNav_Click(object sender, RoutedEventArgs e)
        {
            NavigateToExerciseView();
        }

        private void PlannerNav_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new PlannerView();
            HideEditButton();
        }

        private void WorkoutNav_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new WorkoutView();
            HideEditButton();
        }

        private void EditExerciseButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContentArea.Content is ExerciseView exerciseView)
            {
                exerciseView.EditSelectedExercise();
            }
        }

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

        private void HideEditButton()
        {
            EditExerciseButton.Opacity = 0;
            EditExerciseButton.IsHitTestVisible = false;
        }
    }
}
