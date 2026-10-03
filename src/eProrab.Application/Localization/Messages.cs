using eProrab.Domain.Enums;

namespace eProrab.Application.Localization;

/// <summary>
/// Az/En/Ru text for every <see cref="SystemMessageKey"/>. Deliberately a plain
/// static dictionary (not .resx/IStringLocalizer) so it needs no satellite
/// assemblies or designer files — trivial to extend, easy to unit test, and
/// works identically everywhere the app runs.
/// </summary>
public static class Messages
{
    private static readonly Dictionary<SystemMessageKey, Dictionary<Language, string>> Text = new()
    {
        [SystemMessageKey.InvalidCredentials] = new()
        {
            [Language.Az] = "Yanlış e-poçt və ya şifrə.",
            [Language.En] = "Invalid email or password.",
            [Language.Ru] = "Неверный email или пароль."
        },
        [SystemMessageKey.AccountDeactivated] = new()
        {
            [Language.Az] = "Hesabınız deaktiv edilib. Zəhmət olmasa admin ilə əlaqə saxlayın.",
            [Language.En] = "Your account has been deactivated. Please contact an administrator.",
            [Language.Ru] = "Ваша учётная запись деактивирована. Свяжитесь с администратором."
        },
        [SystemMessageKey.EmailAlreadyRegistered] = new()
        {
            [Language.Az] = "Bu e-poçt artıq qeydiyyatdan keçib.",
            [Language.En] = "This email is already registered.",
            [Language.Ru] = "Этот email уже зарегистрирован."
        },
        [SystemMessageKey.PhoneNumberAlreadyRegistered] = new()
        {
            [Language.Az] = "Bu telefon nömrəsi artıq qeydiyyatdan keçib.",
            [Language.En] = "This phone number is already registered.",
            [Language.Ru] = "Этот номер телефона уже зарегистрирован."
        },
        [SystemMessageKey.InvalidOrExpiredRefreshToken] = new()
        {
            [Language.Az] = "Sessiya etibarsızdır və ya vaxtı bitib. Yenidən daxil olun.",
            [Language.En] = "Session is invalid or has expired. Please log in again.",
            [Language.Ru] = "Сессия недействительна или истекла. Войдите снова."
        },
        [SystemMessageKey.ItemNotFound] = new()
        {
            [Language.Az] = "Məhsul tapılmadı.",
            [Language.En] = "Item was not found.",
            [Language.Ru] = "Товар не найден."
        },
        [SystemMessageKey.CategoryNotFound] = new()
        {
            [Language.Az] = "Kateqoriya tapılmadı.",
            [Language.En] = "Category was not found.",
            [Language.Ru] = "Категория не найдена."
        },
        [SystemMessageKey.UserNotFound] = new()
        {
            [Language.Az] = "İstifadəçi tapılmadı.",
            [Language.En] = "User was not found.",
            [Language.Ru] = "Пользователь не найден."
        },
        [SystemMessageKey.SpecializationNotFound] = new()
        {
            [Language.Az] = "İxtisas tapılmadı.",
            [Language.En] = "Specialization was not found.",
            [Language.Ru] = "Специализация не найдена."
        },
        [SystemMessageKey.WorkerProfileNotFound] = new()
        {
            [Language.Az] = "İşçi profili tapılmadı.",
            [Language.En] = "Worker profile was not found.",
            [Language.Ru] = "Профиль работника не найден."
        },
        [SystemMessageKey.WorkerProfileAlreadyExists] = new()
        {
            [Language.Az] = "Artıq işçi profiliniz mövcuddur.",
            [Language.En] = "You already have a worker profile.",
            [Language.Ru] = "У вас уже есть профиль работника."
        },
        [SystemMessageKey.JobPostingNotFound] = new()
        {
            [Language.Az] = "Vakansiya tapılmadı.",
            [Language.En] = "Job posting was not found.",
            [Language.Ru] = "Вакансия не найдена."
        },
        [SystemMessageKey.JobPostingNotOpen] = new()
        {
            [Language.Az] = "Bu vakansiya artıq açıq deyil.",
            [Language.En] = "This job posting is no longer open.",
            [Language.Ru] = "Эта вакансия больше не открыта."
        },
        [SystemMessageKey.JobApplicationNotFound] = new()
        {
            [Language.Az] = "Müraciət tapılmadı.",
            [Language.En] = "Job application was not found.",
            [Language.Ru] = "Заявка не найдена."
        },
        [SystemMessageKey.AlreadyAppliedToJob] = new()
        {
            [Language.Az] = "Siz artıq bu vakansiyaya müraciət etmisiniz.",
            [Language.En] = "You have already applied to this job.",
            [Language.Ru] = "Вы уже откликнулись на эту вакансию."
        },
        [SystemMessageKey.CannotApplyToOwnJob] = new()
        {
            [Language.Az] = "Öz elanınıza müraciət edə bilməzsiniz.",
            [Language.En] = "You cannot apply to your own job posting.",
            [Language.Ru] = "Нельзя откликнуться на собственную вакансию."
        },
        [SystemMessageKey.SkuAlreadyExists] = new()
        {
            [Language.Az] = "Bu SKU artıq mövcuddur.",
            [Language.En] = "This SKU already exists.",
            [Language.Ru] = "Этот SKU уже существует."
        },
        [SystemMessageKey.SlugAlreadyExists] = new()
        {
            [Language.Az] = "Bu slug artıq mövcuddur.",
            [Language.En] = "This slug already exists.",
            [Language.Ru] = "Этот slug уже существует."
        },
        [SystemMessageKey.CannotDeleteLastAdmin] = new()
        {
            [Language.Az] = "Sonuncu admin istifadəçini silmək və ya deaktiv etmək olmaz.",
            [Language.En] = "You cannot delete or deactivate the last remaining admin.",
            [Language.Ru] = "Нельзя удалить или деактивировать последнего администратора."
        },
        [SystemMessageKey.AccountLockedOut] = new()
        {
            [Language.Az] = "15 dəqiqə ərzində 5 dəfə ardıcıl yanlış şifrə daxil edildiyi üçün giriş 15 dəqiqəlik bloklanıb.",
            [Language.En] = "Access has been locked for 15 minutes due to 5 consecutive failed login attempts.",
            [Language.Ru] = "Вход заблокирован на 15 минут из-за 5 подряд неверных попыток ввода пароля."
        }
    };

    public static string Get(SystemMessageKey key, Language language) =>
        Text.TryGetValue(key, out var byLanguage)
            ? byLanguage.GetValueOrDefault(language, byLanguage[Language.En])
            : key.ToString();
}
