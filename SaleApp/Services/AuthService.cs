using SaleApp.Models;
using SaleApp.Repositories;

namespace SaleApp.Services;

public class AuthService
{
    private readonly AuthRepository _repo = new();
    private const int MaxFailedAttempts = 5;

    public record LoginResult(bool Success, string? Error, User? User);

    public LoginResult Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return new(false, "Vui lòng nhập tên đăng nhập và mật khẩu.", null);

        var user = _repo.GetByUsername(username.Trim());
        if (user == null)
            return new(false, "Tên đăng nhập không tồn tại.", null);

        if (!user.IsActive)
            return new(false, "Tài khoản đã bị khoá. Liên hệ quản lý.", null);

        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.Now)
        {
            var remaining = (int)(user.LockedUntil.Value - DateTime.Now).TotalMinutes + 1;
            return new(false, $"Tài khoản bị tạm khoá. Thử lại sau {remaining} phút.", null);
        }

        bool valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        if (!valid)
        {
            _repo.IncrementFailedLogin(user.UserId);
            if (user.FailedLoginCount + 1 >= MaxFailedAttempts)
            {
                _repo.LockAccount(user.UserId, 15);
                return new(false, "Sai mật khẩu quá nhiều lần. Tài khoản bị khoá 15 phút.", null);
            }
            int remaining = MaxFailedAttempts - user.FailedLoginCount - 1;
            return new(false, $"Sai mật khẩu. Còn {remaining} lần thử.", null);
        }

        _repo.UpdateLastLogin(user.UserId);
        return new(true, null, user);
    }

    public record RegisterResult(bool Success, string? Error, int UserId);

    public RegisterResult Register(string username, string email, string password,
        string confirmPassword, int roleId, string? fullName, string? phone)
    {
        if (string.IsNullOrWhiteSpace(username))
            return new(false, "Tên đăng nhập không được trống.", 0);

        if (username.Length < 4 || username.Length > 50)
            return new(false, "Tên đăng nhập phải từ 4–50 ký tự.", 0);

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return new(false, "Email không hợp lệ.", 0);

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return new(false, "Mật khẩu phải có ít nhất 6 ký tự.", 0);

        if (password != confirmPassword)
            return new(false, "Mật khẩu xác nhận không khớp.", 0);

        if (_repo.UsernameExists(username.Trim()))
            return new(false, "Tên đăng nhập đã tồn tại.", 0);

        if (_repo.EmailExists(email.Trim()))
            return new(false, "Email đã được đăng ký.", 0);

        var hash = BCrypt.Net.BCrypt.HashPassword(password, 10);
        var userId = _repo.Register(username.Trim(), email.Trim(), hash, roleId,
            string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim(),
            string.IsNullOrWhiteSpace(phone)    ? null : phone.Trim());

        return new(true, null, userId);
    }

    public List<Role> GetRoles() => _repo.GetAllRoles();
}
