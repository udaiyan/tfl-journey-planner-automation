using Reqnroll;
using TfLJourneyPlanner.Automation.Drivers;
using TfLJourneyPlanner.Automation.Tools;

namespace TfLJourneyPlanner.Automation.Hooks
{
    /// <summary>
    /// Manages the Reqnroll lifecycle hooks for the UI automation framework.
    /// This class ensures that a fresh, isolated browser instance is created and 
    /// properly disposed of for every single test scenario, preventing state 
    /// leakage and memory accumulation between tests.
    /// </summary>
    [Binding]
    public class TestHooks
    {
        private readonly ScenarioContext _scenarioContext;

        /// <summary>
        /// Initializes a new instance of the TestHooks class.
        /// </summary>
        /// <param name="scenarioContext">The Reqnroll context used to share state (like the driver and settings) across step definitions within the same scenario.</param>
        public TestHooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        /// <summary>
        /// Executes before each scenario begins.
        /// Loads the environment configuration, initializes the Playwright browser driver, 
        /// and registers both the settings and the driver into the ScenarioContext for use by step definitions.
        /// </summary>
        [BeforeScenario(Order = 1)]
        public async Task BeforeScenario()
        {
            var settings = TestSettings.Load();
            var driver = new PlaywrightDriver(settings);

            await driver.StartAsync();

            _scenarioContext.Set(settings);
            _scenarioContext.Set(driver);
        }

        /// <summary>
        /// Executes after each scenario completes (whether it passed or failed).
        /// Safely disposes of the Playwright browser instance to free up system resources 
        /// and prevent zombie browser processes. Catches exceptions in case the browser 
        /// was already forcefully closed due to a critical test failure.
        /// </summary>
        [AfterScenario(Order = 100)]
        public async Task AfterScenario()
        {
            try
            {
                var driver = _scenarioContext.Get<PlaywrightDriver>();
                await driver.StopAsync();
            }
            catch
            {
                // The browser may already have been closed for failed scenarios.
                // Swallowing the exception ensures the test runner can cleanly report the original failure.
            }
        }
    }
}
