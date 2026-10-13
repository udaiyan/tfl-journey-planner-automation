# TfL Journey Planner UI Automation Framework

This repository contains a UI automation framework built to test the Transport for London (TfL) Journey Planner widget. The framework is written in C# using the Page Object Model (POM) design pattern, BDD (Behavior-Driven Development) via Reqnroll, and Microsoft Playwright for reliable browser automation.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher.
- [Playwright Browsers](https://playwright.dev/dotnet/docs/browsers) installed. 
  - Run the following command in the project root to install the required Chromium browser:
    `pwsh bin/Debug/net8.0/playwright.ps1 install chromium`

## How to Run the Tests

1. Clone the repository and navigate to the project folder.
2. Restore dependencies and build the project:
   `dotnet restore`
   `dotnet build`
3. Run the test suite:
   `dotnet test --logger "console;verbosity=normal"`

*(Note: By default, the tests run in headed mode so you can watch the browser. To run headless, update `Headless` to `true` in `appsettings.json`)*.

## Framework Architecture & Development Decisions

### 1. Design Pattern: Page Object Model (POM)
The framework strictly follows the Page Object Model. Each distinct UI view (`JourneyPlannerWidget`, `JourneyResultsPage`, `JourneyDetailsPage`) is encapsulated in its own class. This ensures that if the UI changes, updates only need to be made in one place, keeping the test scripts (Step Definitions) clean and readable.

### 2. State Management & Lazy Initialization
Page Objects are lazily initialized within the `JourneyPlannerStepDefinitions` class. This ensures that a fresh, isolated instance of the page interactions is used for every scenario, preventing state leakage between tests while minimizing memory overhead.

### 3. Configuration: The Override Pattern
Environment-specific settings (Base URL, Timeouts, Headless mode) are managed via `appsettings.json`. The `TestSettings` class implements the "Override Pattern" with sensible C# defaults. This prevents `NullReferenceExceptions` if the config file is missing and allows the exact same codebase to run differently in local vs. CI/CD environments without code changes.

### 4. Test Data Separation
Business test data (e.g., Station names, partial autocomplete strings) is centralized in a static `TestData` class. This separates immutable business data from environment configuration, provides compile-time type safety, and eliminates "magic strings" scattered throughout the codebase.

### 5. Handling Complex UI Quirks (TfL Specific)
Automating public-facing websites often requires handling aggressive UI overlays and dynamic SPAs. Key decisions made to ensure stability include:
* **Cookie Overlay Interception:** The TfL site uses a stubborn `#cb-cookieoverlay` that intercepts pointer events. Instead of relying on flaky CSS hiders, the framework uses `Page.EvaluateAsync` to forcefully remove the overlay from the DOM via JavaScript before interactions.
* **Custom Polling with Safety Nets:** For complex assertions (e.g., verifying accessibility info while ignoring generic footer links), native Playwright waits were insufficient. Custom polling loops utilizing a `Stopwatch` safety net were implemented to prevent infinite hangs while accurately evaluating conditional UI states.
* **Dynamic Fallbacks:** Because short journeys (Leicester Square to Covent Garden) only display "Walking/Cycling" by default, the "View Details" button is missing. The framework dynamically detects this, applies the "Least walking" preference to force a Tube journey, and then retries, ensuring the test fulfills the business requirement without hardcoding brittle waits.

## CI/CD Pipeline & Bot Protection Note
⚠️ **Important:** The GitHub Actions pipeline is configured and functional, but tests may fail in the CI environment due to Transport for London's Web Application Firewall (WAF) blocking shared GitHub Actions IP addresses.

**Why this happens:**
- GitHub Actions runners use public, shared Azure IP addresses
- TfL employs aggressive bot protection mechanisms (e.g., Akamai/Cloudflare) to prevent automated scraping
- These security systems detect and block the CI environment, preventing the Journey Planner widget from loading

**How this is resolved in enterprise environments:**
1. **Self-hosted runners with static IPs:** Run the CI pipeline on TfL's internal network or approved static IP addresses that are allow-listed
2. **API mocking:** Mock the TfL backend API responses for CI validation, reserving full UI E2E tests for a staging environment with relaxed WAF rules
3. **Test environment:** Use a dedicated test/staging environment of the TfL website that has bot protection disabled for automation

**Local execution:** All tests pass successfully when run locally, demonstrating the framework is functionally correct. The CI failures are infrastructure-related, not code-related.

## AI Disclaimer
**Responsible Use of AI:** 
AI tools (specifically Large Language Models) were utilized responsibly during the development of this framework. AI was used to assist in generating boilerplate code, debugging complex Playwright locator issues (such as the cookie overlay interception), and refactoring code for senior-level best practices (like the Override Pattern and lazy initialization). However, the overall architectural decisions, test strategy, Page Object design, and the handling of specific TfL UI quirks were driven by manual analysis, domain knowledge, and senior test automation principles.

---

## Future Improvements & Technical Debt
While the current framework successfully fulfills the challenge requirements, the following architectural improvements would be implemented for a long-term, enterprise-scale rollout:

* **Dynamic Location Handling:** Currently, the `JourneyPlannerWidget` uses hardcoded mappings (`IsKnownValidLocation`) to translate full station names into partial autocomplete triggers. To support a wider matrix of locations, this logic should be refactored to either dynamically select the first available autocomplete suggestion regardless of the input string, or externalize the location mappings to a JSON/CSV data file to remove conditional logic from the Page Object.
* **API-Driven Test Data Setup:** To reduce UI flakiness and execution time, valid journey locations could be seeded by querying the TfL backend API directly, rather than relying on the UI autocomplete dropdowns for test setup.

## Additional Test Scenarios
Please refer to the [AdditionalScenarios.md](./AdditionalScenarios.md) file for a list of recommended functional and non-functional test scenarios for this public-facing component.
