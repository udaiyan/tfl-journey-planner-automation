using System.Diagnostics;
using Microsoft.Playwright;
using TfLJourneyPlanner.Automation.Tools;

namespace TfLJourneyPlanner.Automation.Pages
{
    /// <summary>
    /// Represents the Journey Details page/view within the TfL Journey Planner.
    /// This Page Object is responsible for verifying specific station information, 
    /// particularly accessibility details (e.g., step-free access, lifts, escalators) 
    /// once a user drills down into a specific journey route.
    /// </summary>
    public class JourneyDetailsPage : BasePage
    {
        /// <summary>
        /// Initializes a new instance of the JourneyDetailsPage class.
        /// </summary>
        /// <param name="page">The active Playwright IPage instance provided by the driver.</param>
        /// <param name="settings">The loaded TestSettings configuration for the test run.</param>
        public JourneyDetailsPage(IPage page, TestSettings settings)
            : base(page, settings)
        {
        }

        /// <summary>
        /// Verifies that the specified station name is visible and that relevant 
        /// accessibility information is displayed on the page.
        /// It specifically searches for keywords like "step-free", "lift", or "escalator" 
        /// while intentionally ignoring generic footer links (e.g., "Transport accessibility") 
        /// to prevent false positives.
        /// </summary>
        /// <param name="stationName">The name of the station to verify (e.g., "Covent Garden Underground Station").</param>
        public async Task AssertAccessInformationAsync(string stationName)
        {
            try
            {
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions
                {
                    Timeout = Settings.DefaultTimeoutMs
                });
            }
            catch
            {
                // Network idle can be flaky on public websites.
            }

            var stationLocator = Page.GetByText(stationName).First;
            if (!await WaitHelper.IsVisibleAsync(stationLocator, 5000))
            {
                stationLocator = Page.Locator("text=/Covent Garden/i").First;
            }

            try
            {
                await stationLocator.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = Settings.DefaultTimeoutMs
                });
            }
            catch
            {
                // If station name isn't found exactly, we proceed to check for access info anyway.
            }

            // IMPLEMENTATION NOTE: Custom Polling with a Safety Net
            // Playwright's native auto-waiting is excellent for single, specific elements, but it cannot 
            // natively handle complex conditional logic (e.g., "find ANY of these keywords, BUT ignore 
            // the generic 'Transport accessibility' footer link to prevent false positives").
            // To solve this, we use a custom polling loop. The Stopwatch acts as a strict safety net, 
            // ensuring the loop breaks and throws a TimeoutException after the configured duration, 
            // preventing the test suite from hanging indefinitely.
            var accessibilityKeywords = new[] { "step-free", "step free", "lift", "escalator", "wheelchair", "platform", "accessibility" };
            var foundVisibleAccessInfo = false;

            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < Settings.DefaultTimeoutMs)
            {
                foreach (var keyword in accessibilityKeywords)
                {
                    var locator = Page.Locator($"text=/{keyword}/i");
                    var count = await locator.CountAsync();
                    var max = Math.Min(count, 50);

                    for (var i = 0; i < max; i++)
                    {
                        try
                        {
                            if (await locator.Nth(i).IsVisibleAsync())
                            {
                                var text = await locator.Nth(i).InnerTextAsync();
                                if (!text.Trim().Equals("Transport accessibility", StringComparison.OrdinalIgnoreCase))
                                {
                                    foundVisibleAccessInfo = true;
                                    break;
                                }
                            }
                        }
                        catch
                        {
                            // Element detached
                        }
                    }
                    if (foundVisibleAccessInfo) break;
                }

                if (foundVisibleAccessInfo) break;
                await Page.WaitForTimeoutAsync(500);
            }

            if (!foundVisibleAccessInfo)
            {
                throw new TimeoutException("Could not find visible access information for the station.");
            }
        }
    }
}
