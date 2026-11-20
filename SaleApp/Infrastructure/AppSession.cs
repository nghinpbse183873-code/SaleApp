using SaleApp.Models;

namespace SaleApp.Infrastructure;

/// <summary>Singleton that holds the currently logged-in user during the session.</summary>
public static class AppSession
{
    public static User? CurrentUser { get; private set; }
    public static bool IsLoggedIn => CurrentUser != null;
    public static bool IsStaff => CurrentUser?.IsStaff == true;
    public static bool IsCustomer => CurrentUser?.IsCustomer == true;

    public static void Login(User user) => CurrentUser = user;

    public static void Logout() => CurrentUser = null;
}
