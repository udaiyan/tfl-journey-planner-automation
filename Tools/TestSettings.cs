using System.Text.Json;

namespace TfLJourneyPlanner.Automation.Tools
{
    /// <summary>
    /// Manages the environment-specific configuration for the automation framework.
    /// 
    /// This class implements the "Override Pattern" with sensible defaults:
    /// 1. Sensible Defaults: Properties are initialized with baseline values directly in C#. 
    ///    This acts as a fail-safe, preventing NullReferenceExceptions if the JSON file is missing.
    /// 2. Environment Overrides: The Load() method reads appsettings.json and deserializes it, 
    ///    overriding the C# defaults. This allows the same codebase to run differently in 
    ///    various environments (e.g., Headless=false locally for debugging, Headless=true in CI/CD) 
    ///    without requiring code changes.
    /// </summary>
    public class TestSettings
    {
        /// <summary>
        /// The base URL of the application under test.
        /// Default: "https://tfl.gov.uk"
        /// </summary>
        public string BaseUrl { get; set; } = "https://tfl.gov.uk";

        /// <summary>
        /// The specific path to the Journey Planner widget.
        /// Default: "/plan-a-journey/?cid=plan-a-journey"
        /// </summary>
        public string JourneyPlannerPath { get; set; } = "/plan-a-journey/?cid=plan-a-journey";

        /// <summary>
        /// Determines whether the browser should run in headless mode (no UI).
        /// Default: false (visible browser for local debugging).
        /// Note: This should be overridden to 'true' in CI/CD pipelines.
        /// </summary>
        public bool Headless { get; set; } = false;

        /// <summary>
        /// Adds a delay (in milliseconds) between Playwright actions. 
        /// Useful for visual debugging. 
        /// Default: 0 (fastest execution).
        /// </summary>
        public int SlowMoMs { get; set; } = 0;

        /// <summary>
        /// The default timeout (in milliseconds) for Playwright auto-waiting assertions.
        /// Default: 30000 (30 seconds).
        /// </summary>
        public int DefaultTimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Loads and deserializes the configuration from appsettings.json, 
        /// overriding any default C# property values with the JSON values.
        /// </summary>
        /// <returns>A populated TestSettings instance.</returns>
        /// <exception cref="FileNotFoundException">Thrown if appsettings.json is not found in the build directory.</exception>
        public static TestSettings Load()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"appsettings.json was not found at '{path}'. The framework will fall back to C# default values, but explicit configuration is recommended.");
            }

            var json = File.ReadAllText(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            return JsonSerializer.Deserialize<TestSettings>(json, options);
        }
    }
}
