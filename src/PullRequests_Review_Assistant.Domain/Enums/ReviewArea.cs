namespace PullRequests_Review_Assistant.Domain.Enums
{
    /// <summary>
    /// Defines all possible areas of code review.
    /// Core areas are always included by default; extended areas are opt-in.
    /// </summary>
    [Flags]
    public enum ReviewArea
    {
        None = 0,

        // ------------------------------
        // CORE (included in base review)
        // ------------------------------
        Performance          = 1 << 0,
        Architecture         = 1 << 1,
        Vulnerabilities      = 1 << 2,
        CodeSmells           = 1 << 3,

        // ----------------------------
        // EXTRA (optional via builder)
        // ----------------------------
        CodeFormatting       = 1 << 4,
        Linting              = 1 << 5,
        Copyrights           = 1 << 6,
        Documentation        = 1 << 7,
        Naming               = 1 << 8,
        ErrorHandling        = 1 << 9,
        Concurrency          = 1 << 10,
        Testing              = 1 << 11,
        DependencyManagement = 1 << 12,
        Accessibility        = 1 << 13,
        Logging              = 1 << 14,
        HardcodedSecrets     = 1 << 15,
        DeadCode             = 1 << 16,
        Complexity           = 1 << 17,
        DuplicateCode        = 1 << 18,
        ApiDesign            = 1 << 19,

        // --------------------------
        // GROUPINGS (combined areas)
        // --------------------------
        /// <summary>
        /// Shorthand for the four core review areas.
        /// </summary>
        CoreReview = Performance | Architecture | Vulnerabilities | CodeSmells,

        /// <summary>
        /// Every available review area.
        /// </summary>
        All                  = (1 << 20) - 1
    }
}