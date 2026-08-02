namespace eProrab.Application.Localization;

/// <summary>Keys for the small set of system-level messages that need az/en/ru text
/// (auth errors, generic validation problems). Domain data translations
/// (items, categories, specializations) go through their own Translation entities instead.</summary>
public enum SystemMessageKey
{
    InvalidCredentials,
    AccountDeactivated,
    EmailAlreadyRegistered,
    InvalidOrExpiredRefreshToken,
    ItemNotFound,
    CategoryNotFound,
    UserNotFound,
    SpecializationNotFound,
    WorkerProfileNotFound,
    WorkerProfileAlreadyExists,
    JobPostingNotFound,
    JobPostingNotOpen,
    JobApplicationNotFound,
    AlreadyAppliedToJob,
    CannotApplyToOwnJob,
    SkuAlreadyExists,
    SlugAlreadyExists,
    CannotDeleteLastAdmin
}
