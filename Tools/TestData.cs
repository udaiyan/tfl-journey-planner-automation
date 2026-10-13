namespace TfLJourneyPlanner.Automation.Tools
{
    /// <summary>
    /// A centralized, type-safe repository for all business-related test data used in the automation suite.
    /// By using static constants instead of hardcoding "magic strings" throughout the Page Objects and Step Definitions, 
    /// this class ensures maintainability, compile-time safety, and a clear separation of concerns from environment 
    /// configuration (which belongs in appsettings.json).
    /// </summary>
    public static class TestData
    {
        /// <summary>
        /// The full, exact name of the valid origin station as it should appear in the final input field.
        /// </summary>
        public const string ValidOrigin = "Leicester Square Underground Station";

        /// <summary>
        /// The exact text of the option as it appears in the TfL autocomplete dropdown menu.
        /// </summary>
        public const string ValidOriginDropdown = "Leicester Square Underground";

        /// <summary>
        /// The partial string typed into the input field to successfully trigger the autocomplete API for the origin.
        /// </summary>
        public const string ValidOriginPartial = "lei";

        /// <summary>
        /// The full, exact name of the valid destination station as it should appear in the final input field.
        /// </summary>
        public const string ValidDestination = "Covent Garden Underground Station";

        /// <summary>
        /// The exact text of the option as it appears in the TfL autocomplete dropdown menu.
        /// </summary>
        public const string ValidDestinationDropdown = "Covent Garden Underground";

        /// <summary>
        /// The partial string typed into the input field to successfully trigger the autocomplete API for the destination.
        /// </summary>
        public const string ValidDestinationPartial = "cov";
    }
}
