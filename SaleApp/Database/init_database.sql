-- ============================================================
-- SaleApp Database Initialization Script
-- SQL Server (LocalDB) - Run once to set up everything
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SaleAppDB')
    CREATE DATABASE SaleAppDB;
GO

USE SaleAppDB;
GO

-- ============================================================
-- TABLES
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Roles')
CREATE TABLE dbo.Roles (
    RoleId      INT            IDENTITY(1,1) PRIMARY KEY,
    RoleName    NVARCHAR(50)   NOT NULL UNIQUE,
    Description NVARCHAR(200),
    CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
CREATE TABLE dbo.Users (
    UserId           INT            IDENTITY(1,1) PRIMARY KEY,
    Username         NVARCHAR(50)   NOT NULL UNIQUE,
    Email            NVARCHAR(150)  NOT NULL UNIQUE,
    PasswordHash     NVARCHAR(256)  NOT NULL,
    RoleId           INT            NOT NULL REFERENCES dbo.Roles(RoleId),
    IsActive         BIT            NOT NULL DEFAULT 1,
    IsEmailVerified  BIT            NOT NULL DEFAULT 0,
    FailedLoginCount INT            NOT NULL DEFAULT 0,
    LockedUntil      DATETIME       NULL,
    LastLoginAt      DATETIME       NULL,
    CreatedAt        DATETIME       NOT NULL DEFAULT GETDATE(),
    UpdatedAt        DATETIME       NOT NULL DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserProfiles')
CREATE TABLE dbo.UserProfiles (
    ProfileId   INT            IDENTITY(1,1) PRIMARY KEY,
    UserId      INT            NOT NULL UNIQUE REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    FullName    NVARCHAR(150),
    PhoneNumber NVARCHAR(20),
    Address     NVARCHAR(300),
    AvatarUrl   NVARCHAR(500),
    DateOfBirth DATE,
    Gender      TINYINT,       -- 0=Male, 1=Female, 2=Other
    UpdatedAt   DATETIME       NOT NULL DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Categories')
CREATE TABLE dbo.Categories (
    CategoryId   INT            IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100)  NOT NULL,
    Description  NVARCHAR(300),
    ImageUrl     NVARCHAR(500),
    DisplayOrder INT            NOT NULL DEFAULT 0,
    IsActive     BIT            NOT NULL DEFAULT 1,
    CreatedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    UpdatedAt    DATETIME       NOT NULL DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Products')
CREATE TABLE dbo.Products (
    ProductId        INT             IDENTITY(1,1) PRIMARY KEY,
    CategoryId       INT             NOT NULL REFERENCES dbo.Categories(CategoryId),
    ProductName      NVARCHAR(200)   NOT NULL,
    Description      NVARCHAR(1000),
    Price            DECIMAL(12,2)   NOT NULL DEFAULT 0,
    CostPrice        DECIMAL(12,2),
    StockQty         INT             NOT NULL DEFAULT 0,
    Unit             NVARCHAR(30)    DEFAULT N'phần',
    ImageUrl         NVARCHAR(500),
    IsActive         BIT             NOT NULL DEFAULT 1,
    IsFeatured       BIT             NOT NULL DEFAULT 0,
    CreatedByStaffId INT             REFERENCES dbo.Users(UserId),
    CreatedAt        DATETIME        NOT NULL DEFAULT GETDATE(),
    UpdatedAt        DATETIME        NOT NULL DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Carts')
CREATE TABLE dbo.Carts (
    CartId      INT      IDENTITY(1,1) PRIMARY KEY,
    CustomerId  INT      NOT NULL REFERENCES dbo.Users(UserId),
    Status      TINYINT  NOT NULL DEFAULT 0,  -- 0=Active, 1=CheckedOut, 2=Abandoned
    CreatedAt   DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt   DATETIME NOT NULL DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CartItems')
CREATE TABLE dbo.CartItems (
    CartItemId INT           IDENTITY(1,1) PRIMARY KEY,
    CartId     INT           NOT NULL REFERENCES dbo.Carts(CartId) ON DELETE CASCADE,
    ProductId  INT           NOT NULL REFERENCES dbo.Products(ProductId),
    Quantity   INT           NOT NULL DEFAULT 1,
    UnitPrice  DECIMAL(12,2) NOT NULL,
    AddedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdatedAt  DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_CartItems_Cart_Product UNIQUE (CartId, ProductId)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Orders')
CREATE TABLE dbo.Orders (
    OrderId          INT            IDENTITY(1,1) PRIMARY KEY,
    CustomerId       INT            NOT NULL REFERENCES dbo.Users(UserId),
    OrderCode        NVARCHAR(20)   NOT NULL UNIQUE DEFAULT '',
    SubTotal         DECIMAL(14,2)  NOT NULL DEFAULT 0,
    DiscountAmount   DECIMAL(14,2)  NOT NULL DEFAULT 0,
    TotalAmount      DECIMAL(14,2)  NOT NULL DEFAULT 0,
    PaymentMethod    NVARCHAR(50)   DEFAULT 'Cash',
    PaymentStatus    TINYINT        NOT NULL DEFAULT 0,  -- 0=Pending, 1=Paid, 2=Refunded
    OrderStatus      TINYINT        NOT NULL DEFAULT 0,  -- 0=Pending, 1=Confirmed, 2=Preparing, 3=Completed, 4=Cancelled
    Note             NVARCHAR(500),
    ServedByStaffId  INT            REFERENCES dbo.Users(UserId),
    OrderedAt        DATETIME       NOT NULL DEFAULT GETDATE(),
    UpdatedAt        DATETIME       NOT NULL DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OrderItems')
CREATE TABLE dbo.OrderItems (
    OrderItemId  INT           IDENTITY(1,1) PRIMARY KEY,
    OrderId      INT           NOT NULL REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
    ProductId    INT           NOT NULL REFERENCES dbo.Products(ProductId),
    ProductName  NVARCHAR(200) NOT NULL,
    Quantity     INT           NOT NULL DEFAULT 1,
    UnitPrice    DECIMAL(12,2) NOT NULL,
    TotalPrice   DECIMAL(14,2) NOT NULL
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Promotions')
CREATE TABLE dbo.Promotions (
    PromotionId  INT           IDENTITY(1,1) PRIMARY KEY,
    Code         NVARCHAR(50)  NOT NULL UNIQUE,
    Description  NVARCHAR(200),
    DiscountPct  DECIMAL(5,2)  DEFAULT 0,
    DiscountAmt  DECIMAL(12,2) DEFAULT 0,
    MinOrderAmt  DECIMAL(12,2) DEFAULT 0,
    StartDate    DATETIME,
    EndDate      DATETIME,
    IsActive     BIT           NOT NULL DEFAULT 1,
    UsageLimit   INT,
    UsageCount   INT           NOT NULL DEFAULT 0
);
GO

-- ============================================================
-- VIEWS
-- ============================================================

IF OBJECT_ID('dbo.vw_TopSellingProducts', 'V') IS NOT NULL DROP VIEW dbo.vw_TopSellingProducts;
GO
CREATE VIEW dbo.vw_TopSellingProducts AS
SELECT
    p.ProductId,
    p.ProductName,
    c.CategoryName,
    ISNULL(SUM(oi.Quantity), 0)   AS TotalQuantitySold,
    ISNULL(SUM(oi.TotalPrice), 0) AS TotalRevenue,
    COUNT(DISTINCT oi.OrderId)    AS OrderCount
FROM dbo.Products p
INNER JOIN dbo.Categories c ON p.CategoryId = c.CategoryId
LEFT  JOIN dbo.OrderItems oi ON p.ProductId = oi.ProductId
LEFT  JOIN dbo.Orders o ON oi.OrderId = o.OrderId AND o.OrderStatus = 3
GROUP BY p.ProductId, p.ProductName, c.CategoryName;
GO

IF OBJECT_ID('dbo.vw_MonthlyRevenue', 'V') IS NOT NULL DROP VIEW dbo.vw_MonthlyRevenue;
GO
CREATE VIEW dbo.vw_MonthlyRevenue AS
SELECT
    YEAR(o.OrderedAt)  AS [Year],
    MONTH(o.OrderedAt) AS [Month],
    SUM(o.TotalAmount) AS TotalRevenue,
    COUNT(*)           AS TotalOrders
FROM dbo.Orders o
WHERE o.OrderStatus = 3 AND o.PaymentStatus = 1
GROUP BY YEAR(o.OrderedAt), MONTH(o.OrderedAt);
GO

-- ============================================================
-- STORED PROCEDURES
-- ============================================================

IF OBJECT_ID('dbo.sp_GetOrCreateActiveCart', 'P') IS NOT NULL DROP PROC dbo.sp_GetOrCreateActiveCart;
GO
CREATE PROCEDURE dbo.sp_GetOrCreateActiveCart
    @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @CartId INT;

    SELECT @CartId = CartId FROM dbo.Carts
    WHERE CustomerId = @CustomerId AND Status = 0;

    IF @CartId IS NULL
    BEGIN
        INSERT INTO dbo.Carts (CustomerId, Status) VALUES (@CustomerId, 0);
        SET @CartId = SCOPE_IDENTITY();
    END

    SELECT @CartId AS CartId;
END;
GO

IF OBJECT_ID('dbo.sp_CheckoutCart', 'P') IS NOT NULL DROP PROC dbo.sp_CheckoutCart;
GO
CREATE PROCEDURE dbo.sp_CheckoutCart
    @CartId       INT,
    @PaymentMethod NVARCHAR(50) = 'Cash',
    @Note         NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Get customer
        DECLARE @CustomerId INT;
        SELECT @CustomerId = CustomerId FROM dbo.Carts WHERE CartId = @CartId AND Status = 0;
        IF @CustomerId IS NULL
            THROW 50001, 'Cart not found or already checked out.', 1;

        -- Calculate totals
        DECLARE @SubTotal DECIMAL(14,2);
        SELECT @SubTotal = SUM(Quantity * UnitPrice) FROM dbo.CartItems WHERE CartId = @CartId;
        IF @SubTotal IS NULL OR @SubTotal = 0
            THROW 50002, 'Cart is empty.', 1;

        -- Generate order code
        DECLARE @OrderCode NVARCHAR(20);
        SET @OrderCode = 'ORD-' + CONVERT(NVARCHAR(8), GETDATE(), 112) +
                         '-' + RIGHT('000' + CAST(ABS(CHECKSUM(NEWID())) % 1000 AS NVARCHAR(3)), 3);

        -- Create order
        INSERT INTO dbo.Orders (CustomerId, OrderCode, SubTotal, DiscountAmount, TotalAmount,
                                 PaymentMethod, PaymentStatus, OrderStatus, Note)
        VALUES (@CustomerId, @OrderCode, @SubTotal, 0, @SubTotal, @PaymentMethod, 1, 3, @Note);

        DECLARE @OrderId INT = SCOPE_IDENTITY();

        -- Copy items to OrderItems & reduce stock
        INSERT INTO dbo.OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPrice, TotalPrice)
        SELECT @OrderId, ci.ProductId, p.ProductName, ci.Quantity,
               ci.UnitPrice, ci.Quantity * ci.UnitPrice
        FROM dbo.CartItems ci
        INNER JOIN dbo.Products p ON ci.ProductId = p.ProductId
        WHERE ci.CartId = @CartId;

        -- Reduce stock
        UPDATE p SET p.StockQty = p.StockQty - ci.Quantity, p.UpdatedAt = GETDATE()
        FROM dbo.Products p
        INNER JOIN dbo.CartItems ci ON p.ProductId = ci.ProductId
        WHERE ci.CartId = @CartId;

        -- Mark cart as checked out
        UPDATE dbo.Carts SET Status = 1, UpdatedAt = GETDATE() WHERE CartId = @CartId;

        COMMIT TRANSACTION;
        SELECT @OrderId AS OrderId, @OrderCode AS OrderCode, @SubTotal AS TotalAmount;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

IF OBJECT_ID('dbo.sp_GetOrderDetail', 'P') IS NOT NULL DROP PROC dbo.sp_GetOrderDetail;
GO
CREATE PROCEDURE dbo.sp_GetOrderDetail
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Result set 1: Order header
    SELECT o.OrderId, o.CustomerId,
           u.Username AS CustomerUsername,
           up.FullName AS CustomerName,
           up.PhoneNumber AS CustomerPhone,
           o.OrderCode, o.OrderedAt, o.SubTotal, o.DiscountAmount, o.TotalAmount,
           o.PaymentMethod, o.PaymentStatus, o.OrderStatus, o.Note,
           su.Username AS StaffUsername, sup.FullName AS StaffName
    FROM dbo.Orders o
    INNER JOIN dbo.Users u         ON o.CustomerId     = u.UserId
    LEFT  JOIN dbo.UserProfiles up ON u.UserId          = up.UserId
    LEFT  JOIN dbo.Users su        ON o.ServedByStaffId = su.UserId
    LEFT  JOIN dbo.UserProfiles sup ON su.UserId        = sup.UserId
    WHERE o.OrderId = @OrderId;

    -- Result set 2: Order items
    SELECT oi.OrderItemId, oi.ProductId, oi.ProductName, c.CategoryName,
           oi.Quantity, oi.UnitPrice, oi.TotalPrice, p.ImageUrl AS ProductImageUrl
    FROM dbo.OrderItems oi
    INNER JOIN dbo.Products p    ON oi.ProductId   = p.ProductId
    INNER JOIN dbo.Categories c  ON p.CategoryId   = c.CategoryId
    WHERE oi.OrderId = @OrderId;
END;
GO

IF OBJECT_ID('dbo.sp_UpdateOrderPayment', 'P') IS NOT NULL DROP PROC dbo.sp_UpdateOrderPayment;
GO
CREATE PROCEDURE dbo.sp_UpdateOrderPayment
    @OrderId       INT,
    @PaymentStatus TINYINT,
    @OrderStatus   TINYINT,
    @StaffId       INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Orders
    SET PaymentStatus   = @PaymentStatus,
        OrderStatus     = @OrderStatus,
        ServedByStaffId = @StaffId,
        UpdatedAt       = GETDATE()
    WHERE OrderId = @OrderId;
    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

IF OBJECT_ID('dbo.sp_GetCustomerPurchaseHistory', 'P') IS NOT NULL DROP PROC dbo.sp_GetCustomerPurchaseHistory;
GO
CREATE PROCEDURE dbo.sp_GetCustomerPurchaseHistory
    @CustomerId   INT  = NULL,
    @FromDate     DATE = NULL,
    @ToDate       DATE = NULL,
    @OrderStatus  TINYINT = NULL,
    @PaymentStatus TINYINT = NULL,
    @PageIndex    INT = 1,
    @PageSize     INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;

    SELECT
        u.UserId          AS CustomerId,
        u.Username        AS CustomerUsername,
        up.FullName       AS CustomerName,
        up.PhoneNumber    AS CustomerPhone,
        u.Email           AS CustomerEmail,
        o.OrderId,
        o.OrderCode,
        o.OrderedAt,
        o.SubTotal,
        o.DiscountAmount,
        o.TotalAmount,
        o.PaymentMethod,
        o.PaymentStatus,
        CASE o.PaymentStatus WHEN 0 THEN N'Chờ thanh toán' WHEN 1 THEN N'Đã thanh toán' WHEN 2 THEN N'Đã hoàn tiền' ELSE 'N/A' END AS PaymentStatusText,
        o.OrderStatus,
        CASE o.OrderStatus WHEN 0 THEN N'Chờ xử lý' WHEN 1 THEN N'Đã xác nhận' WHEN 2 THEN N'Đang chuẩn bị' WHEN 3 THEN N'Hoàn thành' WHEN 4 THEN N'Đã huỷ' ELSE 'N/A' END AS OrderStatusText,
        o.Note,
        su.Username       AS StaffUsername,
        sup.FullName      AS StaffName,
        (SELECT COUNT(DISTINCT oi.ProductId) FROM dbo.OrderItems oi WHERE oi.OrderId = o.OrderId) AS TotalItems,
        (SELECT ISNULL(SUM(oi.Quantity), 0) FROM dbo.OrderItems oi WHERE oi.OrderId = o.OrderId) AS TotalQuantity,
        COUNT(*) OVER()   AS TotalCount
    FROM dbo.Orders o
    INNER JOIN dbo.Users u         ON o.CustomerId     = u.UserId
    LEFT  JOIN dbo.UserProfiles up ON u.UserId          = up.UserId
    LEFT  JOIN dbo.Users su        ON o.ServedByStaffId = su.UserId
    LEFT  JOIN dbo.UserProfiles sup ON su.UserId        = sup.UserId
    WHERE (@CustomerId   IS NULL OR o.CustomerId     = @CustomerId)
      AND (@FromDate     IS NULL OR CAST(o.OrderedAt AS DATE) >= @FromDate)
      AND (@ToDate       IS NULL OR CAST(o.OrderedAt AS DATE) <= @ToDate)
      AND (@OrderStatus  IS NULL OR o.OrderStatus    = @OrderStatus)
      AND (@PaymentStatus IS NULL OR o.PaymentStatus = @PaymentStatus)
    ORDER BY o.OrderedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

IF OBJECT_ID('dbo.sp_GetCustomerStats', 'P') IS NOT NULL DROP PROC dbo.sp_GetCustomerStats;
GO
CREATE PROCEDURE dbo.sp_GetCustomerStats
    @CustomerId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        u.UserId          AS CustomerId,
        u.Username        AS CustomerUsername,
        up.FullName       AS CustomerName,
        up.PhoneNumber,
        u.CreatedAt       AS RegisteredAt,
        u.LastLoginAt,
        COUNT(o.OrderId)  AS TotalOrders,
        SUM(CASE WHEN o.OrderStatus = 4 THEN 1 ELSE 0 END) AS CancelledOrders,
        SUM(CASE WHEN o.OrderStatus = 3 THEN 1 ELSE 0 END) AS CompletedOrders,
        ISNULL(SUM(CASE WHEN o.PaymentStatus = 1 THEN o.TotalAmount ELSE 0 END), 0) AS TotalSpent,
        ISNULL(AVG(CASE WHEN o.PaymentStatus = 1 THEN o.TotalAmount ELSE NULL END), 0) AS AvgOrderValue,
        MAX(o.OrderedAt)  AS LastOrderAt,
        MIN(o.OrderedAt)  AS FirstOrderAt
    FROM dbo.Users u
    INNER JOIN dbo.Roles r        ON u.RoleId = r.RoleId AND r.RoleName = 'Customer'
    LEFT  JOIN dbo.UserProfiles up ON u.UserId = up.UserId
    LEFT  JOIN dbo.Orders o        ON u.UserId = o.CustomerId
    WHERE (@CustomerId IS NULL OR u.UserId = @CustomerId)
    GROUP BY u.UserId, u.Username, up.FullName, up.PhoneNumber,
             u.CreatedAt, u.LastLoginAt;
END;
GO

-- ============================================================
-- SEED DATA
-- ============================================================

-- Roles
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'StoreStaff')
    INSERT INTO dbo.Roles (RoleName, Description)
    VALUES ('StoreStaff', N'Nhân viên cửa hàng – Quản lý sản phẩm và đơn hàng'),
           ('Customer',   N'Khách hàng – Mua sắm tại cửa hàng');

-- Staff account  (password = Admin@123)
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'admin_staff')
BEGIN
    DECLARE @StaffRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = 'StoreStaff');
    INSERT INTO dbo.Users (Username, Email, PasswordHash, RoleId, IsActive, IsEmailVerified)
    VALUES ('admin_staff', 'staff@saleapp.vn',
            '$2a$10$PLACEHOLDER_STAFF_HASH_PLACEHOLDER_STAFF', @StaffRoleId, 1, 1);
    DECLARE @StaffId INT = SCOPE_IDENTITY();
    INSERT INTO dbo.UserProfiles (UserId, FullName, PhoneNumber)
    VALUES (@StaffId, N'Nguyễn Văn Staff', '0901234567');
END

-- Customer account  (password = Customer@123)
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'customer_anh')
BEGIN
    DECLARE @CustRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = 'Customer');
    INSERT INTO dbo.Users (Username, Email, PasswordHash, RoleId, IsActive, IsEmailVerified)
    VALUES ('customer_anh', 'anh@example.vn',
            '$2a$10$PLACEHOLDER_CUSTOMER_HASH_PLACEHOLDER_CUST', @CustRoleId, 1, 1);
    DECLARE @CustId INT = SCOPE_IDENTITY();
    INSERT INTO dbo.UserProfiles (UserId, FullName, PhoneNumber)
    VALUES (@CustId, N'Trần Thị Ánh', '0912345678');
END

-- Categories
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryName = N'Cơm & Bún')
    INSERT INTO dbo.Categories (CategoryName, Description, DisplayOrder, IsActive) VALUES
    (N'Cơm & Bún',      N'Các món cơm, bún, phở',               1, 1),
    (N'Đồ Nướng',       N'Thịt nướng, hải sản nướng',            2, 1),
    (N'Đồ Chiên',       N'Khoai tây chiên, gà rán...',           3, 1),
    (N'Đồ Uống Nóng',  N'Cà phê, trà, sữa nóng',               4, 1),
    (N'Đồ Uống Lạnh',  N'Nước ép, sinh tố, trà sữa',           5, 1),
    (N'Tráng Miệng',   N'Bánh ngọt, kem, chè',                  6, 1),
    (N'Snack & Ăn Vặt',N'Bánh mì, bắp rang, khoai chip',        7, 1);

-- Products
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE ProductName = N'Cơm Gà Xối Mỡ')
BEGIN
    DECLARE @Cat1 INT = (SELECT CategoryId FROM dbo.Categories WHERE CategoryName = N'Cơm & Bún');
    DECLARE @Cat2 INT = (SELECT CategoryId FROM dbo.Categories WHERE CategoryName = N'Đồ Nướng');
    DECLARE @Cat3 INT = (SELECT CategoryId FROM dbo.Categories WHERE CategoryName = N'Đồ Chiên');
    DECLARE @Cat4 INT = (SELECT CategoryId FROM dbo.Categories WHERE CategoryName = N'Đồ Uống Nóng');
    DECLARE @Cat5 INT = (SELECT CategoryId FROM dbo.Categories WHERE CategoryName = N'Đồ Uống Lạnh');
    DECLARE @Cat6 INT = (SELECT CategoryId FROM dbo.Categories WHERE CategoryName = N'Tráng Miệng');

    INSERT INTO dbo.Products (CategoryId, ProductName, Description, Price, CostPrice, StockQty, Unit, IsActive, IsFeatured)
    VALUES
    (@Cat1, N'Cơm Gà Xối Mỡ',    N'Cơm trắng + gà xối mỡ giòn rụm',              55000, 28000, 50, N'phần', 1, 1),
    (@Cat1, N'Bún Bò Huế',        N'Bún bò cay, chả lụa, giò heo',                65000, 32000, 40, N'tô',   1, 1),
    (@Cat1, N'Phở Bò Tái',        N'Phở Hà Nội truyền thống bò tái',              70000, 35000, 40, N'tô',   1, 0),
    (@Cat1, N'Cơm Chiên Dương Châu',N'Cơm chiên hải sản kiểu Dương Châu',         60000, 30000, 30, N'phần', 1, 0),
    (@Cat2, N'Gà Nướng Mật Ong',  N'Gà nướng ướp mật ong và gia vị đặc biệt',    85000, 42000, 25, N'phần', 1, 1),
    (@Cat2, N'Sườn Nướng BBQ',    N'Sườn heo ướp BBQ nướng than hoa',            95000, 48000, 20, N'phần', 1, 0),
    (@Cat3, N'Khoai Tây Chiên',   N'Khoai tây chiên vàng giòn – size vừa',        35000, 15000, 80, N'phần', 1, 0),
    (@Cat3, N'Cánh Gà Chiên Giòn',N'Cánh gà chiên giòn KFC style',               75000, 38000, 35, N'phần', 1, 1),
    (@Cat4, N'Cà Phê Đen',        N'Cà phê nguyên chất Tây Nguyên',               30000, 10000, 100,N'ly',   1, 0),
    (@Cat4, N'Bạc Xỉu',           N'Cà phê sữa đặc béo ngậy',                     35000, 12000, 80, N'ly',   1, 0),
    (@Cat4, N'Trà Sữa Trân Châu', N'Trà sữa Đài Loan với trân châu đường đen',    45000, 20000, 60, N'ly',   1, 1),
    (@Cat5, N'Nước Ép Cam',       N'Cam vắt tươi 100%',                            40000, 18000, 50, N'ly',   1, 0),
    (@Cat5, N'Sinh Tố Bơ',        N'Bơ đặc + sữa đặc + đá xay',                  50000, 22000, 40, N'ly',   1, 1),
    (@Cat6, N'Bánh Tiramisu',     N'Bánh tiramisu kem mascarpone Italy',           65000, 30000, 20, N'miếng',1, 0),
    (@Cat6, N'Chè Thái',          N'Chè Thái đầy màu sắc với thạch và trái cây',  45000, 20000, 30, N'ly',   1, 0);
END

PRINT 'SaleAppDB initialized successfully!';
GO
