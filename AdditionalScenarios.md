# Additional Test Scenarios for TfL Journey Planner

As requested, below is a list of additional functional and non-functional scenarios that should be tested for this public-facing component. These are not automated in this repository but represent a comprehensive testing strategy for a production-grade release.

## Functional Scenarios

### 1. Date and Time Travel Planning
* **Scenario:** Verify the user can plan a journey for a specific future date and time.
* **Scenario:** Verify the user can select "Arrive by" instead of "Leaving at" and that the results reflect the correct arrival time.
* **Scenario:** Verify the widget handles invalid date/time inputs (e.g., past dates, malformed times) gracefully.

### 2. Advanced Accessibility Preferences
* **Scenario:** Verify that selecting "Step-free access only" filters out journeys requiring stairs or escalators.
* **Scenario:** Verify that selecting "Avoiding stairs" correctly routes the user through lifts or ramps.

### 3. Multi-Modal & Complex Journeys
* **Scenario:** Verify a journey that requires multiple modes of transport (e.g., Bus + Tube + Walking) displays clear interchange instructions.
* **Scenario:** Verify the widget correctly calculates and displays the total journey time, including wait times and walking transfers.

### 4. "Edit Journey" & Favourites
* **Scenario:** Verify the "Edit journey" link on the results page correctly pre-populates the widget with the current From/To locations.
* **Scenario:** Verify a user can save a planned journey to "Favourites" and successfully load it later.
* **Scenario:** Verify the "Swap" (reverse From/To) functionality works correctly from the results page.

---

## Non-Functional Scenarios

### 1. Performance & Network Resilience
* **Scenario:** Verify the widget loads and becomes interactive within 3 seconds on a standard 4G network connection.
* **Scenario:** Verify the widget gracefully degrades (shows a user-friendly error message) if the underlying TfL Journey API times out or returns a 500 error.
* **Scenario:** Verify performance under throttled 3G networks to ensure the UI doesn't freeze or show broken loading states.

### 2. Security (Input Validation)
* **Scenario:** Verify the From/To input fields sanitize against Cross-Site Scripting (XSS) attempts (e.g., entering `<script>alert(1)</script>` should not execute).
* **Scenario:** Verify the autocomplete API is rate-limited or protected against Denial of Service (DoS) via rapid, automated keystroke injection.

### 3. Accessibility (a11y) & Usability
* **Scenario:** Verify the entire journey planning flow can be completed using only a keyboard (Tab, Enter, Arrow Keys) without a mouse.
* **Scenario:** Verify the autocomplete dropdown and journey results are correctly announced by screen readers (WCAG 2.1 AA compliance).
* **Scenario:** Verify sufficient color contrast ratios for text, especially for the "Walking", "Cycling", and "Tube" mode indicators.

### 4. Cross-Browser & Responsive Compatibility
* **Scenario:** Verify the widget functions identically across Chromium, Firefox, and WebKit (Safari) browsers.
* **Scenario:** Verify the widget is fully responsive and usable on mobile viewports (e.g., iPhone SE, iPad) without horizontal scrolling or overlapping UI elements.