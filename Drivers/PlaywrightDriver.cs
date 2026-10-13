using System.Threading.Tasks;
using Microsoft.Playwright;
using TfLJourneyPlanner.Automation.Tools;

namespace TfLJourneyPlanner.Automation.Drivers
{
    /// <summary>
    /// Manages the lifecycle and configuration of the Playwright browser instance for UI automation.
    /// This class acts as a wrapper around Playwright's core objects (IPlaywright, IBrowser, IPage), 
    /// ensuring they are initialized with the correct settings (e.g., Headless mode, SlowMo, Timeouts) 
    /// and gracefully disposed of after each test scenario to prevent memory leaks and browser zombie processes.
    /// </summary>
    public class PlaywrightDriver
    {
        private readonly TestSettings _settings;

        /// <summary>
        /// The root Playwright instance.
        /// </summary>
        public IPlaywright Playwright { get; private set; }

        /// <summary>
        /// The launched browser instance (e.g., Chromium).
        /// </summary>
        public IBrowser Browser { get; private set; }

        /// <summary>
        /// The active browser page/tab where test actions are performed.
        /// </summary>
        public IPage Page { get; private set; }

        /// <summary>
        /// Initializes a new instance of the PlaywrightDriver with the specified test settings.
        /// </summary>
        /// <param name="settings">The configuration settings for the test run (e.g., timeouts, headless mode).</param>
        public PlaywrightDriver(TestSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Initializes the Playwright engine, launches the browser, and creates a new page.
        /// Applies default timeouts and launch options defined in the TestSettings.
        /// </summary>
        public async Task StartAsync()
        {
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

            Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = _settings.Headless,
                SlowMo = _settings.SlowMoMs
            });

            Page = await Browser.NewPageAsync();
            Page.SetDefaultTimeout(_settings.DefaultTimeoutMs);
        }

        /// <summary>
        /// Gracefully closes the active page, disposes of the browser instance, 
        /// and cleans up the root Playwright object to free system resources.
        /// </summary>
        public async Task StopAsync()
        {
            if (Page != null)
            {
                await Page.CloseAsync();
            }

            if (Browser != null)
            {
                await Browser.DisposeAsync();
            }

            Playwright?.Dispose();
        }
    }
}
