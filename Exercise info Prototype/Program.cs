using System.Text.Json;
using System.Text.Json.Serialization;

namespace Exercise_info_Prototype
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //Exercise exercise1 = new Exercise("Squat", "A lower body exercise that targets the thighs and glutes.", true);
            //Exercise exercise2 = new Exercise("Push-up", "An upper body exercise that targets the chest, shoulders, and triceps.", false);
            //Exercise exercise3 = new Exercise("Deadlift", "A compound exercise that targets the back, glutes, and hamstrings.", true);

            Exercise? exercise1 = LoadExercise("Squat");
            Exercise? exercise2 = LoadExercise("Push-up");
            Exercise? exercise3 = LoadExercise("Deadlift");

            //Plan plan1 = new Plan();
            //if (exercise1 != null) plan1.AddExercise(exercise1);
            //if (exercise2 != null) plan1.AddExercise(exercise2);
            //if (exercise3 != null) plan1.AddExercise(exercise3);

            //SavePlan(plan1);
            Plan? plan = LoadPlan("plan");


            Console.WriteLine("\nCreating workout from plan...");
            Workout workout1 = new Workout(plan);
            workout1.PrintWorkout();

            Console.WriteLine("\nModifying Squat exercise...");

            workout1.ModifyExercise("Squat", 4, 10, 100);
            workout1.PrintWorkout();

            SaveWorkout(workout1);

            if (exercise1 != null) SaveExercise(exercise1);
            if (exercise2 != null) SaveExercise(exercise2);
            if (exercise3 != null) SaveExercise(exercise3);

            string?[] exerciseNames = GetAllFileNames();

            foreach (string? name in exerciseNames)
            {
                Console.WriteLine("Found exercise file: " + name);
            }

            Console.WriteLine();

            Workout? loadedWorkout = LoadWorkout("workout");
            if (loadedWorkout != null)
            {
                loadedWorkout.PrintWorkout();
            }

            Console.ReadKey();
        }

        /// <summary>
        /// Retrieves the names of all JSON files in the Workout\Exercises directory within the user's application data
        /// folder.
        /// </summary>
        /// <remarks>If the specified directory does not exist, a message is printed to the console
        /// indicating the absence of the exercises directory.</remarks>
        /// <returns>An array of strings containing the names of all JSON files found in the specified directory, without their
        /// file extensions. The array will be empty if no files are found or if the directory does not exist.</returns>
        static public string?[] GetAllFileNames()
        {
            string romingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string filePath = Path.Combine(romingAppDataPath, "Workout\\Exercises");
            if (Directory.Exists(filePath))
            {
                return Directory.GetFiles(filePath, "*.json").Select(Path.GetFileNameWithoutExtension).ToArray();
            }
            else
            {
                Console.WriteLine("\nNo exercises directory found at: " + filePath);
                return Array.Empty<string>();
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
        static public bool SaveExercise(Exercise exercise)
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
        /// Loads an exercise from a JSON file located in the user's application data directory based on the specified
        /// exercise name.
        /// </summary>
        /// <remarks>This method attempts to read a JSON file from the path
        /// 'ApplicationData\Workout\Exercises\{exerciseName}.json'. If the file does not exist, it returns null and
        /// logs a message indicating the absence of the saved exercise. Any exceptions encountered during the loading
        /// process are caught and logged, and null is returned in such cases.</remarks>
        /// <param name="exerciseName">The name of the exercise to load. This corresponds to the filename of the JSON file (without the .json
        /// extension) stored in the 'Workout\Exercises' folder within the user's application data directory.</param>
        /// <returns>An instance of the Exercise class if the exercise is successfully loaded; otherwise, null if the exercise
        /// does not exist or an error occurs during loading.</returns>
        static public Exercise? LoadExercise(string exerciseName)
        {
            try
            {
                string romingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(romingAppDataPath, "Workout\\Exercises", $"{exerciseName}.json");
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() }
                    };
                    Exercise? exercise = JsonSerializer.Deserialize<Exercise>(json, options);

                    if (exercise == null || string.IsNullOrEmpty(exercise.Name) || exercise.Name != exerciseName)
                    {
                        Console.WriteLine("Failed to deserialize exercise from: " + filePath);
                        return null;
                    }
                    else
                    {
                        Console.WriteLine("Exercise loaded from: " + filePath);
                        return exercise;
                    }
                }
                else
                {
                    Console.WriteLine("No saved exercise found at: " + filePath);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Deletes the specified exercise file from the application's exercise directory.
        /// </summary>
        /// <remarks>If the specified exercise does not exist, the method returns false without throwing
        /// an exception. Any exceptions that occur during file operations are caught and handled internally, resulting
        /// in a return value of false.</remarks>
        /// <param name="exerciseName">The name of the exercise to delete, without the file extension. Cannot be null or empty.</param>
        /// <returns>true if the exercise was successfully deleted; otherwise, false.</returns>
        static public bool DeleteExerciseFile(string exerciseName)
        {
            try
            {
                string romingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(romingAppDataPath, "Workout\\Exercises", $"{exerciseName}.json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine("Exercise deleted: " + filePath);
                    return true;
                }
                else
                {
                    Console.WriteLine("No exercise found to delete at: " + filePath);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
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
        static public bool SaveWorkout(Workout workout)
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
                string fullFilePath = Path.Combine(filePath, "workout.json");
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
        /// Loads a workout from a JSON file located in the user's roaming application data directory.
        /// </summary>
        /// <remarks>The method attempts to read a file named 'workout.json' from the 'Workout'
        /// subdirectory within the roaming application data folder. If the file exists, it deserializes the JSON
        /// content into a <see cref="Workout"/> object. If the file does not exist, it returns null and logs a message
        /// indicating that no saved workout was found. Any exceptions encountered during the file reading or
        /// deserialization process are caught, and null is returned.</remarks>
        /// <returns>An instance of the <see cref="Workout"/> class representing the loaded workout, or null if no saved workout
        /// is found or an error occurs during loading.</returns>
        static public Workout? LoadWorkout(string workoutName)
        {
            try
            {
                string roamingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(roamingAppDataPath, "Workout", workoutName + ".json");
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() }
                    };
                    Workout? workout = JsonSerializer.Deserialize<Workout>(json, options);

                    if (workout == null || workout.Exercises == null) 
                    {
                        Console.WriteLine("Failed to deserialize workout from: " + filePath);
                        return null;
                    }
                    else
                    {
                        Console.WriteLine("Workout loaded from: " + filePath);
                        return workout;
                    }
                }
                else
                {
                    Console.WriteLine("No saved workout found at: " + filePath);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Deletes the workout file located in the user's roaming application data folder if it exists.
        /// </summary>
        /// <remarks>If the workout file does not exist, the method returns false without throwing an
        /// exception. Any exceptions encountered during the deletion process are caught and logged, and the method
        /// returns false in such cases.</remarks>
        /// <returns>true if the workout file was successfully deleted; otherwise, false.</returns>
        static public bool DeleteWorkoutFile()
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

        /// <summary>
        /// Saves the specified workout plan to a JSON file in the user's roaming application data directory.
        /// </summary>
        /// <remarks>The method creates the target directory if it does not exist and writes the plan to a
        /// file named 'plan.json'. If an error occurs during the save operation, the exception details are logged to
        /// the console.</remarks>
        /// <param name="plan">The workout plan to serialize and save. Cannot be null.</param>
        /// <returns>true if the plan was successfully saved; otherwise, false.</returns>
        static public bool SavePlan(Plan plan)
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
        /// Loads a workout plan from a JSON file located in the user's roaming application data directory.
        /// </summary>
        /// <remarks>The method searches for a 'plan.json' file in the 'Workout\Plan' subdirectory of the
        /// roaming application data folder. If the file exists but cannot be deserialized into a valid <see
        /// cref="Plan"/>, or if no file is found, the method returns <see langword="null"/>. Callers should check the
        /// return value for <see langword="null"/> to handle missing or invalid plans.</remarks>
        /// <returns>A <see cref="Plan"/> object representing the loaded workout plan, or <see langword="null"/> if the plan
        /// could not be found or deserialized.</returns>
        static public Plan? LoadPlan(string planName)
        {
            try
            {
                string roamingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(roamingAppDataPath, "Workout\\Plan", planName + ".json");
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() }
                    };
                    Plan? plan = JsonSerializer.Deserialize<Plan>(json, options);
                    if (plan == null || plan.WorkoutPlan == null)
                    {
                        Console.WriteLine("Failed to deserialize plan from: " + filePath);
                        return null;
                    }
                    else
                    {
                        Console.WriteLine("Plan loaded from: " + filePath);
                        return plan;
                    }
                }
                else
                {
                    Console.WriteLine("No saved plan found at: " + filePath);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public static bool DeletePlan(string planName)
        {
            try
            {
                string roamingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(roamingAppDataPath, "Workout\\Plan", planName + ".json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine("Plan deleted: " + filePath);
                    return true;
                }
                else
                {
                    Console.WriteLine("No plan found to delete at: " + filePath);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Deletes the plan file from the user's roaming application data folder if it exists.
        /// </summary>
        /// <remarks>This method attempts to remove the 'plan.json' file located in the 'Workout\Plan'
        /// directory within the user's roaming application data path. If the file is not found, no deletion occurs and
        /// false is returned. Any exceptions encountered during the process are handled internally and result in a
        /// return value of false.</remarks>
        /// <returns>true if the plan file was successfully deleted; otherwise, false if the file did not exist or an error
        /// occurred during deletion.</returns>
        public static bool DeletePlanFile()
        {
            try
            {
                string roamingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string filePath = Path.Combine(roamingAppDataPath, "Workout\\Plan", "plan.json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine("Plan deleted: " + filePath);
                    return true;
                }
                else
                {
                    Console.WriteLine("No plan found to delete at: " + filePath);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Deletes all exercise files from the user's application data directory.
        /// </summary>
        /// <remarks>This method checks for the existence of the 'Exercises' directory within the
        /// 'Workout' folder in the roaming application data path. If the directory exists, it attempts to delete it and
        /// all its contents. If the directory does not exist, a message is logged indicating that no directory was
        /// found to clear. Note that this operation may throw exceptions if there are issues with file access or
        /// permissions.</remarks>
        static public void ClearAllExerciseFiles()
        {
            string roamingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string directoryPath = Path.Combine(roamingAppDataPath, "Workout\\Exercises");
            if (Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.Delete(directoryPath, true);
                    Console.WriteLine("All exercise files deleted from: " + directoryPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                Console.WriteLine("No exercises directory found to clear at: " + directoryPath);
            }
        }

        /// <summary>
        /// Creates a new Workout instance associated with the specified training plan.
        /// </summary>
        /// <param name="plan">The training plan to be used for initializing the Workout. This parameter cannot be null.</param>
        /// <returns>A Workout object initialized with the provided training plan.</returns>
        static public Workout CreateWorkout(Plan plan)
        {
            return new Workout(plan);
        }

        /// <summary>
        /// Creates a new Plan instance that can be used to define a workout routine. The returned Plan object is initialized with an empty workout plan dictionary, allowing for exercises to be added as needed.
        /// </summary>
        /// <returns>A new Plan object initialized with an empty workout plan dictionary.</returns>
        static public Plan CreatePlan()
        {
            return new Plan();
        }

        /// <summary>
        /// Creates a new Exercise instance with the specified name, description, and weightlifting status. The created Exercise object is initialized with the provided parameters, allowing for further customization such as setting the number of sets and repetitions as needed.
        /// </summary>
        /// <param name="name">The name of the exercise. This parameter cannot be null or empty.</param>
        /// <param name="description">A brief description of the exercise. This parameter cannot be null.</param>
        /// <param name="isWeightLifting">Indicates whether the exercise involves weightlifting.</param>
        /// <returns>A new Exercise object initialized with the specified parameters.</returns>
        static public Exercise CreateExercise(string name, string description, bool isWeightLifting)
        {
            return new Exercise(name, description, isWeightLifting);
        }
    }
}
