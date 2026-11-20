namespace SaleApp.Models;

public class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool IsStaff => RoleName == "StoreStaff";
    public bool IsCustomer => RoleName == "Customer";

    /// <summary>Tên tiếng Việt hiển thị trong UI – không phụ thuộc cột Description của DB.</summary>
    public string DisplayLabel => RoleName switch
    {
        "StoreStaff" => "Nhân viên cửa hàng – Quản lý sản phẩm, đơn hàng",
        "Customer"   => "Khách hàng – Mua sắm tại cửa hàng",
        _            => RoleName
    };
}
