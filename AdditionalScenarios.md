# Additional Test Scenarios for TfL Journey Planner

Below is a list of additional functional and non-functional scenarios that should be tested for this public-facing component. These represent a production-grade testing strategy covering performance, advanced accessibility, cross-platform consistency, and complex state management.

## 1. Performance & Reliability Engineering
* **Load Testing:** Verify the widget and underlying API can handle the expected concurrent user load (e.g., 10,000 users planning journeys simultaneously during morning rush hour) without degradation.
* **Stress Testing:** Push the system beyond its normal operational capacity to identify the breaking point and ensure it fails gracefully (e.g., returning a friendly "System busy" message rather than a raw 500 error).
* **Spike Testing:** Simulate sudden, massive surges in traffic (e.g., during a sudden Tube strike or severe weather event) to verify auto-scaling and rate-limiting mechanisms.
* **Soak/Endurance Testing:** Run the application under expected load for an extended period (e.g., 24 hours) to detect memory leaks or database connection pool exhaustion.
* **Network Throttling:** Verify UI behavior and loading states under 3G, 4G, and high-latency satellite connections to ensure the widget doesn't freeze or show broken UI elements.

## 2. Advanced Accessibility (a11y) & Inclusivity
* **Visual Impairments (Color Blindness):** Verify that journey modes (Tube lines, Bus, Walking) and status indicators (Good service, Delays) are distinguishable for users with Deuteranopia, Protanopia, and Tritanopia. Ensure color is not the only means of conveying information (e.g., using icons and text labels alongside colors).
* **Deaf and Hard of Hearing:** Ensure the application does not rely on audio cues for alerts or status changes. Verify that all dynamic updates (like live status changes) are visually announced.
* **Motor Impairments:** Verify the entire flow (including complex dropdowns and the "Access options" radio buttons) can be navigated using only a keyboard (Tab, Enter, Space, Arrow keys) and is compatible with switch access devices.
* **Cognitive Disabilities:** Verify that error messages are clear, constructive, and easy to understand. Ensure the UI is not overly cluttered and that "Leaving" vs "Arriving" logic is clearly explained.
* **Screen Reader Compatibility:** Verify that the dynamic "Access, lifts and escalators" expandable section and the live journey results are correctly announced by NVDA, JAWS, and VoiceOver, including proper ARIA live regions for loading states.

## 3. Cross-Platform & API Consistency
* **Web vs. Mobile App Parity:** Verify that planning the exact same journey (From/To, Time, Preferences) on the TfL Go mobile app yields the exact same route, time, and price as the website. This validates that both front-ends are consuming the same API contracts correctly.
* **Cross-Browser Matrix:** Verify functionality and visual layout across Chrome, Firefox, Safari, and Edge on both Windows and macOS.
* **Responsive Design Matrix:** Verify the widget layout adapts correctly to various viewports (e.g., iPhone SE, iPad Pro, 4K Desktop) without horizontal scrolling, overlapping text, or truncated buttons.

## 4. Combinatorial Testing (The "Matrix")
  * **Mode x Time x Accessibility Matrix:** Create a pairwise test matrix combining:
  * **Modes:** Tube, Bus, Overground, Walking, Cycling.
  * **Times:** Peak, Off-Peak, Night Tube hours, Weekend.
  * **Accessibility:** No requirement, Step-free to platform, Full step-free, Escalators only.
  * *Goal:* Ensure the routing engine correctly calculates valid combinations (e.g., a "Full step-free" journey at 2 AM should correctly return "No journeys found" if Night Tube isn't step-free).

## 5. State Management & Persistence
* **"Save Preferences" Persistence:** Verify that checking "Save these preferences for future visits" correctly stores the user's choices (e.g., "Least walking", "Max walk 60 mins") in Local Storage/Cookies and applies them automatically on the next visit.
* **"Recents" and "My Journeys":** Verify that successfully planned journeys are saved to the "Recents" list and can be clicked to instantly re-populate the From/To fields.
* **Session Timeout:** Verify how the widget behaves if a user leaves the "Plan a journey" page open for several hours and then attempts to submit (e.g., does the session token expire gracefully?).

## 6. Complex Functional Edge Cases
* **"Leaving" vs "Arriving" Logic:** Verify that selecting "Arriving" correctly calculates the departure time backwards from the desired arrival time, accounting for transfer wait times.
* **"Travel Via" (Waypoints):** Verify that adding an intermediate location (e.g., From: A, Via: B, To: C) forces the routing engine to include B, even if it's not the fastest route.
* **Fare Calculation Accuracy:** Verify that the displayed prices (e.g., "£3.00 off peak", "£1.75 anytime") match the official TfL fare tables for the specific zones traveled.
* **Daylight Saving Time (DST) & Leap Years:** Verify journey planning across the "spring forward" and "fall back" clock changes, and on Feb 29th, to ensure the backend time calculations do not break.
* **Outside London / National Rail:** Verify the "Search outside London" checkbox correctly integrates National Rail data for destinations beyond the TfL network (e.g., planning a journey to Oxford).
