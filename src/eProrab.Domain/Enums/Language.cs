namespace eProrab.Domain.Enums;

/// <summary>
/// The three languages eProrab must serve content in.
/// Numeric values are stable and safe to persist in the database.
/// </summary>
public enum Language
{
    Az = 0, // Azərbaycan dili (default)
    En = 1, // English
    Ru = 2  // Русский
}
