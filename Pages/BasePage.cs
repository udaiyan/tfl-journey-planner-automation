using Microsoft.Playwright;
using TfLJourneyPlanner.Automation.Tools;

namespace TfLJourneyPlanner.Automation.Pages
{
    /// <summary>
    /// The foundational base class for all Page Objects in the automation framework.
    /// It provides shared dependencies (the active Playwright IPage instance and TestSettings) 
    /// to derived classes, ensuring consistency, reducing boilerplate code, and enforcing 
    /// a standardized structure across all UI interactions.
    /// </summary>
    public abstract class BasePage
    {
        /// <summary>
        /// The active Playwright page/tab where test actions and assertions are performed.
        /// </summary>
        protected readonly IPage Page;

        /// <summary>
        /// The configuration settings for the current test run (e.g., timeouts, base URLs).
        /// </summary>
        protected readonly TestSettings Settings;

        /// <summary>
        /// Initializes a new instance of the BasePage class.
        /// </summary>
        /// <param name="page">The active Playwright IPage instance provided by the driver.</param>
        /// <param name="settings">The loaded TestSettings configuration for the test run.</param>
        protected BasePage(IPage page, TestSettings settings)
        {
            Page = page;
            Settings = settings;
        }
    }
}
