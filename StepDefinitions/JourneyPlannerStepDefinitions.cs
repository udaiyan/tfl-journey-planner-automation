using Reqnroll;
using TfLJourneyPlanner.Automation.Drivers;
using TfLJourneyPlanner.Automation.Pages;
using TfLJourneyPlanner.Automation.Tools;

namespace TfLJourneyPlanner.Automation.StepDefinitions
{
    /// <summary>
    /// The Reqnroll binding class that acts as the bridge between the Gherkin feature file 
    /// and the underlying Page Object Model. 
    /// It translates human-readable business steps into technical UI interactions and assertions, 
    /// managing the lifecycle and lazy initialization of Page Objects for each test scenario.
    /// </summary>
    [Binding]
    public class JourneyPlannerStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;

        // Lazy-initialized Page Objects to ensure a clean state for each scenario.
        private JourneyPlannerWidget _widget;
        private JourneyResultsPage _results;
        private JourneyDetailsPage _details;

        /// <summary>
        /// Initializes a new instance of the JourneyPlannerStepDefinitions class.
        /// </summary>
        /// <param name="scenarioContext">The Reqnroll context used to share state (like the driver and settings) across step definitions within the same scenario.</param>
        public JourneyPlannerStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        /// <summary>
        /// Retrieves the PlaywrightDriver instance from the ScenarioContext.
        /// </summary>
        private PlaywrightDriver Driver => _scenarioContext.Get<PlaywrightDriver>();

        /// <summary>
        /// Retrieves the TestSettings configuration from the ScenarioContext.
        /// </summary>
        private TestSettings Settings => _scenarioContext.Get<TestSettings>();

        /// <summary>
        /// Lazily initializes and retrieves the JourneyPlannerWidget Page Object.
        /// </summary>
        private JourneyPlannerWidget Widget =>
            _widget ??= new JourneyPlannerWidget(Driver.Page, Settings);

        /// <summary>
        /// Lazily initializes and retrieves the JourneyResultsPage Page Object.
        /// </summary>
        private JourneyResultsPage Results =>
            _results ??= new JourneyResultsPage(Driver.Page, Settings);

        /// <summary>
        /// Lazily initializes and retrieves the JourneyDetailsPage Page Object.
        /// </summary>
        private JourneyDetailsPage Details =>
            _details ??= new JourneyDetailsPage(Driver.Page, Settings);

        /// <summary>
        /// Navigates the browser to the TfL Journey Planner homepage.
        /// </summary>
        [Given("the user is on the Journey Planner page")]
        public async Task GivenUserIsOnJourneyPlannerPage()
        {
            await Widget.NavigateToJourneyPlannerAsync();
        }

        /// <summary>
        /// Dismisses any intrusive cookie banners or overlays that might block UI interactions.
        /// </summary>
        [Given("any cookie banner is dismissed")]
        public async Task GivenCookieBannerIsDismissed()
        {
            await Widget.AcceptCookiesAsync();
        }

        /// <summary>
        /// Orchestrates the happy path: enters a valid origin and destination, submits the form, 
        /// and waits for the results page to load.
        /// </summary>
        [When("the user plans a valid journey from Leicester Square to Covent Garden")]
        public async Task WhenUserPlansValidJourney()
        {
            await Widget.PlanValidJourneyAsync();
            await Results.WaitForResultsAsync();
        }

        /// <summary>
        /// Setup step for scenarios that require the user to already be on the results page.
        /// </summary>
        [Given("the user has planned a valid journey from Leicester Square to Covent Garden")]
        public async Task GivenUserHasPlannedValidJourney()
        {
            await Widget.PlanValidJourneyAsync();
            await Results.WaitForResultsAsync();
        }

        /// <summary>
        /// Asserts that the journey results page has successfully loaded and displays the correct locations.
        /// </summary>
        [Then("the journey results are displayed")]
        public async Task ThenJourneyResultsAreDisplayed()
        {
            await Results.WaitForResultsAsync();
        }

        /// <summary>
        /// Validates that the walking journey time is visible on the results page.
        /// </summary>
        [Then("the walking time is displayed")]
        public async Task ThenWalkingTimeIsDisplayed()
        {
            await Results.AssertModeTimeDisplayedAsync("walking");
        }

        /// <summary>
        /// Validates that the cycling journey time is visible on the results page.
        /// </summary>
        [Then("the cycling time is displayed")]
        public async Task ThenCyclingTimeIsDisplayed()
        {
            await Results.AssertModeTimeDisplayedAsync("cycling");
        }

        /// <summary>
        /// Modifies the journey preferences to prioritize routes with the least amount of walking, 
        /// then updates the journey and waits for the new results to load.
        /// </summary>
        [When("the user updates the journey preferences to least walking")]
        public async Task WhenUserUpdatesJourneyPreferencesToLeastWalking()
        {
            await Results.OpenEditPreferencesAsync();
            await Results.SelectLeastWalkingAsync();
            await Results.UpdateJourneyAsync();
            await Results.WaitForResultsAsync();
        }

        /// <summary>
        /// Validates that a journey time is displayed after the preferences have been updated.
        /// </summary>
        [Then("the updated journey time is displayed")]
        public async Task ThenUpdatedJourneyTimeIsDisplayed()
        {
            await Results.AssertJourneyTimeDisplayedAsync();
        }

        /// <summary>
        /// Attempts to open the detailed view of the first available journey result.
        /// </summary>
        [When("the user opens the journey details")]
        public async Task WhenUserOpensJourneyDetails()
        {
            await Results.OpenFirstDetailsAsync();
        }

        /// <summary>
        /// Verifies that specific accessibility information (e.g., step-free access, lifts) 
        /// is displayed for the destination station.
        /// </summary>
        [Then("access information for Covent Garden Underground Station is displayed")]
        public async Task ThenAccessInformationForCoventGardenIsDisplayed()
        {
            await Details.AssertAccessInformationAsync(TestData.ValidDestination);
        }

        /// <summary>
        /// Enters a specific origin location into the journey planner widget.
        /// </summary>
        /// <param name="origin">The origin location string provided by the feature file.</param>
        [When(@"the user enters origin ""(.*)""")]
        public async Task WhenUserEntersOrigin(string origin)
        {
            await Widget.EnterOriginAsync(origin);
        }

        /// <summary>
        /// Enters a specific destination location into the journey planner widget.
        /// </summary>
        /// <param name="destination">The destination location string provided by the feature file.</param>
        [When(@"the user enters destination ""(.*)""")]
        public async Task WhenUserEntersDestination(string destination)
        {
            await Widget.EnterDestinationAsync(destination);
        }

        /// <summary>
        /// Submits the journey planner form.
        /// </summary>
        [When("the user submits the journey")]
        public async Task WhenUserSubmitsJourney()
        {
            await Widget.SubmitJourneyAsync();
        }

        /// <summary>
        /// Asserts that no valid journey results are displayed, verifying the system's 
        /// handling of invalid location inputs.
        /// </summary>
        [Then("no journey results are displayed")]
        public async Task ThenNoJourneyResultsAreDisplayed()
        {
            await Results.AssertNoResultsAsync();
        }

        /// <summary>
        /// Submits the journey planner form without entering any data to test empty field validation.
        /// </summary>
        [When("the user submits the journey without entering any locations")]
        public async Task WhenUserSubmitsJourneyWithoutEnteringAnyLocations()
        {
            await Widget.SubmitJourneyAsync();
        }

        /// <summary>
        /// Asserts that the widget correctly prevents journey planning when required fields are left empty.
        /// </summary>
        [Then("the widget prevents journey planning")]
        public async Task ThenWidgetPreventsJourneyPlanning()
        {
            await Widget.AssertCannotPlanWithoutLocationsAsync();
        }
    }
}
