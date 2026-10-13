using Microsoft.Playwright;

namespace TfLJourneyPlanner.Automation.Tools
{
    /// <summary>
    /// Provides custom, non-throwing wait utilities for Playwright locators.
    /// Unlike Playwright's native WaitForAsync (which throws a TimeoutException on failure), 
    /// the methods in this class return a boolean. This makes them ideal for use within 
    /// complex, custom polling loops where a temporary absence of an element is an expected 
    /// condition, not a test failure.
    /// </summary>
    public static class WaitHelper
    {
        /// <summary>
        /// Checks if the specified locator becomes visible within the given timeout period.
        /// 
        /// This method safely wraps Playwright's native WaitForAsync in a try-catch block. 
        /// If the element becomes visible, it returns true. If the timeout is reached or the 
        /// element is detached from the DOM, it gracefully returns false instead of throwing 
        /// an exception, preventing unintended test failures during conditional checks.
        /// </summary>
        /// <param name="locator">The Playwright ILocator to check for visibility.</param>
        /// <param name="timeoutMs">The maximum time in milliseconds to wait for the element to become visible.</param>
        /// <returns>True if the element is visible within the timeout; otherwise, false.</returns>
        public static async Task<bool> IsVisibleAsync(ILocator locator, int timeoutMs)
        {
            try
            {
                await locator.First.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = timeoutMs
                });

                return true;
            }
            catch
            {
                // If it times out or the element is detached, it's not visible.
                // We swallow the exception to allow the calling polling loop to continue or evaluate other conditions.
                return false;
            }
        }
    }
}
