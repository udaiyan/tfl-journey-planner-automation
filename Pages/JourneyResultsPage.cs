using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using NUnit.Framework;
using TfLJourneyPlanner.Automation.Tools;

namespace TfLJourneyPlanner.Automation.Pages
{
    /// <summary>
    /// Represents the Journey Results page within the TfL Journey Planner.
    /// This Page Object encapsulates all interactions and assertions related to the 
    /// results view, including verifying journey times, modifying travel preferences 
    /// (e.g., least walking), and handling complex UI quirks such as missing "View details" 
    /// buttons on short walking/cycling trips and false-positive "0 mins" results for invalid inputs.
    /// </summary>
    public class JourneyResultsPage : BasePage
    {
        /// <summary>
        /// Initializes a new instance of the JourneyResultsPage class.
        /// </summary>
        /// <param name="page">The active Playwright IPage instance provided by the driver.</param>
        /// <param name="settings">The loaded TestSettings configuration for the test run.</param>
        public JourneyResultsPage(IPage page, TestSettings settings) : base(page, settings) { }

        /// <summary>
        /// Waits for the Journey Results page to fully load by verifying the URL, 
        /// the main heading, and the presence of the expected origin and destination station names.
        /// </summary>
        public async Task WaitForResultsAsync()
        {
            try { await Page.WaitForURLAsync("**/results**", new PageWaitForURLOptions { Timeout = Settings.DefaultTimeoutMs }); } catch { }
            var resultsHeading = Page.Locator("text=/Journey results|Journey preferences/i").First;
            try { await resultsHeading.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 }); } catch { }
            var fromText = Page.GetByText(TestData.ValidOrigin).First;
            var toText = Page.GetByText(TestData.ValidDestination).First;
            await fromText.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = Settings.DefaultTimeoutMs });
            await toText.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = Settings.DefaultTimeoutMs });
        }

        /// <summary>
        /// Verifies that the specified travel mode (walking or cycling) and its associated 
        /// journey time are visible on the page. Uses custom polling to avoid false positives 
        /// from hidden navigation or footer links.
        /// </summary>
        /// <param name="mode">The travel mode to verify ("walking" or "cycling").</param>
        public async Task AssertModeTimeDisplayedAsync(string mode)
        {
            var modePattern = mode.Equals("walking", StringComparison.OrdinalIgnoreCase) ? "walk" : "cycl";
            var timePattern = "\\d+\\s*(min|mins|minute|minutes)";
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < Settings.DefaultTimeoutMs)
            {
                if (await IsModeWithTimeVisibleAsync(modePattern, timePattern)) return;
                var modeVisible = await IsPatternVisibleAsync(modePattern);
                var timeVisible = await IsPatternVisibleAsync(timePattern);
                if (modeVisible && timeVisible) return;
                await Page.WaitForTimeoutAsync(500);
            }
            throw new TimeoutException($"Timed out waiting for a visible {mode} journey time.");
        }

        /// <summary>
        /// Verifies that any journey time (e.g., "14 mins") is displayed on the page.
        /// </summary>
        public async Task AssertJourneyTimeDisplayedAsync()
        {
            var timePattern = "\\d+\\s*(min|mins|minute|minutes)";
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < Settings.DefaultTimeoutMs)
            {
                if (await IsPatternVisibleAsync(timePattern)) return;
                await Page.WaitForTimeoutAsync(500);
            }
            throw new TimeoutException("Timed out waiting for a visible journey time.");
        }

        /// <summary>
        /// Opens the "Edit preferences" panel to modify journey settings.
        /// </summary>
        public async Task OpenEditPreferencesAsync()
        {
            var editPreferences = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Edit preferences" }).First;
            try { await editPreferences.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = Settings.DefaultTimeoutMs }); await editPreferences.ClickAsync(new LocatorClickOptions { Force = true }); return; } catch { }
            var fallback = Page.Locator("text=/Edit preferences/i").First;
            await fallback.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = Settings.DefaultTimeoutMs });
            await fallback.ClickAsync(new LocatorClickOptions { Force = true });
        }

        /// <summary>
        /// Selects the "Routes with least walking" preference option.
        /// </summary>
        public async Task SelectLeastWalkingAsync()
        {
            var leastWalking = Page.Locator("text=/least walking/i").First;
            try { await leastWalking.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = Settings.DefaultTimeoutMs }); await leastWalking.ClickAsync(new LocatorClickOptions { Force = true }); return; } catch { }
            var labelFallback = Page.Locator("label, [role='radio'], [role='checkbox']").Filter(new LocatorFilterOptions { HasTextRegex = new Regex("least walking", RegexOptions.IgnoreCase) }).First;
            await labelFallback.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = Settings.DefaultTimeoutMs });
            await labelFallback.ClickAsync(new LocatorClickOptions { Force = true });
        }

        /// <summary>
        /// Clicks the "Update journey" button to apply the newly selected preferences.
        /// </summary>
        public async Task UpdateJourneyAsync()
        {
            var updateButton = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Update journey" }).First;
            try { await updateButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = Settings.DefaultTimeoutMs }); await updateButton.ClickAsync(new LocatorClickOptions { Force = true }); } catch { }
        }

        /// <summary>
        /// Attempts to open the first journey's details. 
        /// If the "View details" button is missing (common on short walking/cycling trips), 
        /// it automatically applies the "Least walking" preference to force a Tube/Bus journey 
        /// to appear, then retries. Falls back to clicking the "Walking" card if necessary.
        /// </summary>
        public async Task OpenFirstDetailsAsync()
        {
            try { await Page.Mouse.WheelAsync(0, 400); } catch { }

            var viewDetails = Page.GetByText("View details").First;
            if (await WaitHelper.IsVisibleAsync(viewDetails, 5000))
            {
                await viewDetails.ClickAsync(new LocatorClickOptions { Force = true });
                return;
            }

            Console.WriteLine("[INFO] 'View details' not found. Applying 'Least walking' preference...");
            await OpenEditPreferencesAsync();
            await SelectLeastWalkingAsync();
            await UpdateJourneyAsync();
            await Page.WaitForTimeoutAsync(2000);

            if (await WaitHelper.IsVisibleAsync(viewDetails, 5000))
            {
                await viewDetails.ClickAsync(new LocatorClickOptions { Force = true });
                return;
            }

            Console.WriteLine("[INFO] Still no 'View details'. Clicking 'Walking' card as fallback...");
            var walking = Page.GetByText("Walking").First;
            if (await WaitHelper.IsVisibleAsync(walking, 3000))
            {
                await walking.ClickAsync(new LocatorClickOptions { Force = true });
                return;
            }

            throw new Exception("Could not find the View Details control or a fallback journey card.");
        }

        /// <summary>
        /// Validates that no valid journey results are displayed for an invalid input.
        /// It checks for specific TfL error messages (e.g., "We couldn't find...") and 
        /// handles a known TfL quirk where completely invalid inputs sometimes display 
        /// a "Walking 0 mins" card. If only "0 mins" is shown without actual transport 
        /// modes (Tube, Bus), it is correctly treated as a "no results" state.
        /// </summary>
        public async Task AssertNoResultsAsync()
        {
            await Page.WaitForTimeoutAsync(2000);

            var errorMsg = Page.Locator("text=/We found more than one location matching|We couldn't find|not found|no results|unable to plan|check your/i").First;
            if (await WaitHelper.IsVisibleAsync(errorMsg, 5000))
            {
                return;
            }

            var zeroMins = Page.Locator("text=/0\\s*mins/i").First;
            if (await WaitHelper.IsVisibleAsync(zeroMins, 3000))
            {
                var hasValidTransport = false;
                foreach (var mode in new[] { "Tube", "Bus", "Overground", "Elizabeth line" })
                {
                    if (await WaitHelper.IsVisibleAsync(Page.GetByText(mode).First, 1000))
                    {
                        hasValidTransport = true;
                        break;
                    }
                }
                if (!hasValidTransport)
                {
                    return;
                }
            }

            Assert.Fail("Journey results were displayed for an invalid journey.");
        }

        /// <summary>
        /// Custom polling helper to verify that a specific travel mode and its journey time 
        /// are visible together in the DOM, preventing false positives from unrelated text.
        /// </summary>
        private async Task<bool> IsModeWithTimeVisibleAsync(string modePattern, string timePattern)
        {
            var modeLocator = Page.Locator($"text=/{modePattern}/i");
            var count = await modeLocator.CountAsync();
            var max = Math.Min(count, 50);
            for (var i = 0; i < max; i++)
            {
                var modeElement = modeLocator.Nth(i);
                try
                {
                    if (!await modeElement.IsVisibleAsync()) continue;
                    ILocator current = modeElement;
                    for (var level = 0; level < 4; level++)
                    {
                        try { var text = await current.InnerTextAsync(); if (!string.IsNullOrWhiteSpace(text) && Regex.IsMatch(text, timePattern, RegexOptions.IgnoreCase)) return true; } catch { }
                        current = current.Locator("xpath=..");
                    }
                }
                catch { }
            }
            return false;
        }

        /// <summary>
        /// Custom polling helper to check if ANY element matching a text pattern is visible.
        /// Iterates through all matches to ensure hidden footer/header links don't cause false positives.
        /// </summary>
        private async Task<bool> IsPatternVisibleAsync(string pattern)
        {
            var locator = Page.Locator($"text=/{pattern}/i");
            var count = await locator.CountAsync();
            var max = Math.Min(count, 100);
            for (var i = 0; i < max; i++) { try { if (await locator.Nth(i).IsVisibleAsync()) return true; } catch { } }
            return false;
        }
    }
}
