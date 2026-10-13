using Microsoft.Playwright;
using NUnit.Framework;
using TfLJourneyPlanner.Automation.Tools;

namespace TfLJourneyPlanner.Automation.Pages
{
    /// <summary>
    /// Represents the main Journey Planner widget on the TfL homepage.
    /// This Page Object encapsulates all interactions with the primary search form, 
    /// including navigating to the page, handling the aggressive cookie overlay via 
    /// JavaScript injection, managing the location autocomplete dropdowns, and 
    /// validating form submission states.
    /// </summary>
    public class JourneyPlannerWidget : BasePage
    {
        /// <summary>
        /// Initializes a new instance of the JourneyPlannerWidget class.
        /// </summary>
        /// <param name="page">The active Playwright IPage instance provided by the driver.</param>
        /// <param name="settings">The loaded TestSettings configuration for the test run.</param>
        public JourneyPlannerWidget(IPage page, TestSettings settings)
            : base(page, settings)
        {
        }

        /// <summary>
        /// Navigates to the TfL Journey Planner URL. 
        /// It includes a self-healing mechanism that reloads the page if the widget 
        /// fails to render initially due to CDN or SPA lifecycle delays.
        /// </summary>
        public async Task NavigateToJourneyPlannerAsync()
        {
            var url = Settings.BaseUrl.TrimEnd('/') + Settings.JourneyPlannerPath;

            await Page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = Settings.DefaultTimeoutMs
            });

            await RemoveOverlayAsync();
            await AcceptCookiesAsync();

            if (!await WaitHelper.IsVisibleAsync(GetFromInput(), 8000))
            {
                await Page.ReloadAsync(new PageReloadOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = Settings.DefaultTimeoutMs
                });

                await RemoveOverlayAsync();
                await AcceptCookiesAsync();

                await GetFromInput().WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = Settings.DefaultTimeoutMs
                });
            }
        }

        /// <summary>
        /// Identifies and clicks the cookie acceptance button, then immediately 
        /// removes the underlying DOM overlay to prevent it from intercepting future clicks.
        /// </summary>
        public async Task AcceptCookiesAsync()
        {
            var acceptButton = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept all cookies" }).First;

            try
            {
                await acceptButton.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 3000
                });
                await acceptButton.ClickAsync(new LocatorClickOptions { Force = true, Timeout = 5000 });
            }
            catch { }

            await RemoveOverlayAsync();
        }

        /// <summary>
        /// Orchestrates the full flow of entering a valid origin and destination, 
        /// and submitting the journey form.
        /// </summary>
        public async Task PlanValidJourneyAsync()
        {
            await EnterOriginAsync(TestData.ValidOrigin);
            await EnterDestinationAsync(TestData.ValidDestination);
            await SubmitJourneyAsync();
        }

        /// <summary>
        /// Enters the origin location into the "From" field.
        /// </summary>
        /// <param name="location">The full or partial location string to enter.</param>
        public Task EnterOriginAsync(string location) => EnterLocationAsync(true, location);

        /// <summary>
        /// Enters the destination location into the "To" field.
        /// </summary>
        /// <param name="location">The full or partial location string to enter.</param>
        public Task EnterDestinationAsync(string location) => EnterLocationAsync(false, location);

        /// <summary>
        /// Clicks the "Plan my journey" submit button. 
        /// Uses Force=true to bypass any lingering UI overlays.
        /// </summary>
        public async Task SubmitJourneyAsync()
        {
            await RemoveOverlayAsync();

            try
            {
                var submitButton = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Plan my journey" }).First;
                await submitButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = Settings.DefaultTimeoutMs });
                await submitButton.ClickAsync(new LocatorClickOptions { Timeout = Settings.DefaultTimeoutMs, Force = true });
                return;
            }
            catch { }

            try { await Page.Keyboard.PressAsync("Enter"); } catch { }
        }

        /// <summary>
        /// Validates that the widget prevents journey planning when no locations are entered.
        /// It checks that the form either shows inline validation errors (e.g., aria-invalid="true") 
        /// or fails to navigate to the results page.
        /// </summary>
        public async Task AssertCannotPlanWithoutLocationsAsync()
        {
            await RemoveOverlayAsync();
            await Page.WaitForTimeoutAsync(500);

            var initialUrl = Page.Url;
            await SubmitJourneyAsync();
            await Page.WaitForTimeoutAsync(2000);

            var currentUrl = Page.Url;
            var navigatedToResults = currentUrl.Contains("/results", StringComparison.OrdinalIgnoreCase) &&
                                     !currentUrl.Equals(initialUrl, StringComparison.OrdinalIgnoreCase);

            if (!navigatedToResults)
            {
                var validationError = Page.Locator(".field-validation-error, [aria-invalid='true'], text=/required/i").First;
                if (await WaitHelper.IsVisibleAsync(validationError, 3000))
                {
                    return;
                }
                return;
            }

            Assert.Fail("The widget allowed an empty journey to be planned and navigated to results.");
        }

        private ILocator GetFromInput() => Page.GetByPlaceholder("From").First;
        private ILocator GetToInput() => Page.GetByPlaceholder("To", new PageGetByPlaceholderOptions { Exact = true }).First;
        private ILocator GetSearchInput() => Page.GetByPlaceholder("Place or address").First;

        /// <summary>
        /// Core logic for interacting with the location input fields. 
        /// It handles clicking the main placeholder, switching to the actual search input, 
        /// typing the partial text, and selecting the correct autocomplete suggestion.
        /// </summary>
        /// <param name="isOrigin">True if interacting with the "From" field, false for "To".</param>
        /// <param name="location">The location string to enter.</param>
        private async Task EnterLocationAsync(bool isOrigin, string location)
        {
            await RemoveOverlayAsync();

            var field = isOrigin ? GetFromInput() : GetToInput();
            await field.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = Settings.DefaultTimeoutMs
            });

            await field.ClickAsync(new LocatorClickOptions { Timeout = 10000, Force = true });

            ILocator input = GetSearchInput();
            if (!await WaitHelper.IsVisibleAsync(input, 3000))
            {
                input = field;
            }

            await input.ClickAsync(new LocatorClickOptions { Force = true });
            await input.FillAsync(string.Empty);

            if (IsKnownValidLocation(location, out var optionText, out var partialText))
            {
                await input.FillAsync(partialText);
                await SelectAutoCompleteOptionAsync(optionText);
            }
            else
            {
                await input.FillAsync(location);
                await input.PressAsync("Tab");
            }
        }

        /// <summary>
        /// Determines if the provided location is a known valid station. 
        /// If so, it outputs the exact dropdown text and the partial string 
        /// required to trigger the autocomplete API.
        /// </summary>
        private bool IsKnownValidLocation(string location, out string optionText, out string partialText)
        {
            if (location.Contains("Leicester", StringComparison.OrdinalIgnoreCase))
            {
                optionText = TestData.ValidOriginDropdown;
                partialText = TestData.ValidOriginPartial;
                return true;
            }
            if (location.Contains("Covent", StringComparison.OrdinalIgnoreCase))
            {
                optionText = TestData.ValidDestinationDropdown;
                partialText = TestData.ValidDestinationPartial;
                return true;
            }
            optionText = string.Empty; partialText = string.Empty; return false;
        }

        /// <summary>
        /// Waits for the specific autocomplete dropdown option to appear and clicks it.
        /// </summary>
        /// <param name="optionText">The exact text of the dropdown option to select.</param>
        private async Task SelectAutoCompleteOptionAsync(string optionText)
        {
            var option = Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = optionText }).First;
            try { await option.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = Settings.DefaultTimeoutMs }); await option.ClickAsync(); return; } catch { }
            var fallbackOption = Page.Locator("li, [role='option']").Filter(new LocatorFilterOptions { HasText = optionText }).First;
            await fallbackOption.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = Settings.DefaultTimeoutMs });
            await fallbackOption.ClickAsync();
        }

        /// <summary>
        /// CRITICAL: Injects JavaScript to forcefully remove the TfL cookie overlay 
        /// (cb-cookieoverlay) from the DOM. This prevents Playwright's auto-wait mechanism 
        /// from entering an infinite retry loop when the overlay intercepts pointer events.
        /// </summary>
        private async Task RemoveOverlayAsync()
        {
            try
            {
                await Page.EvaluateAsync(@"() => {
                    const overlay = document.getElementById('cb-cookieoverlay');
                    if (overlay) overlay.remove();
                    document.querySelectorAll('[id*=""cookie""], [class*=""cookie""], [id*=""Cookie""]').forEach(e => e.remove());
                    document.body.style.overflow = 'auto';
                    document.documentElement.style.overflow = 'auto';
                }");
            }
            catch { }
        }
    }
}
