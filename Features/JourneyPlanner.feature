@journeyplanner
Feature: TfL Journey Planner Widget

# The standard TfL journey planner widget currently exposes origin and destination fields only.
# If an intermediate/via field is present in future, a valid start -> invalid middle -> valid end scenario can be added.

Background:
    Given the user is on the Journey Planner page
    And any cookie banner is dismissed

@valid_journey @walking_cycling
Scenario: Plan a valid journey and validate walking and cycling results
    When the user plans a valid journey from Leicester Square to Covent Garden
    Then the journey results are displayed
    And the walking time is displayed
    And the cycling time is displayed

@preferences @least_walking
Scenario: Edit preferences to least walking and validate journey time
    Given the user has planned a valid journey from Leicester Square to Covent Garden
    When the user updates the journey preferences to least walking
    Then the updated journey time is displayed

@details @access_information
Scenario: View journey details and verify Covent Garden access information
    Given the user has planned a valid journey from Leicester Square to Covent Garden
    When the user opens the journey details
    Then access information for Covent Garden Underground Station is displayed

@invalid_journey
Scenario Outline: Invalid journey combinations do not return results
    When the user enters origin "<origin>"
    And the user enters destination "<destination>"
    And the user submits the journey
    Then no journey results are displayed

    Examples:
      | origin                               | destination                          |
      | Leicester Square Underground Station | InvalidDestinationDoNotUse12345      |
      | InvalidOriginDoNotUse12345           | Covent Garden Underground Station    |
      | InvalidOriginDoNotUse12345           | InvalidDestinationDoNotUse12345      |

@empty_journey
Scenario: Empty locations cannot plan a journey
    When the user submits the journey without entering any locations
    Then the widget prevents journey planning
