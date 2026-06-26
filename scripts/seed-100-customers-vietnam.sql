/*
  Script: Tạo 100 khách hàng mẫu (dữ liệu Việt Nam) — SQL Server

  Điều kiện:
  - Bảng Customers khớp entity Core.Entities.Customer (migration hiện tại).
  - Seed mặc định đã có PipelineStages + CustomerTypes (xem Core/SeedData/ModelBuilderSeedExtensions.cs).

  Chạy trên database đã migrate (ví dụ SSMS / sqlcmd).

  Lưu ý: Mã Code dạng KH-2026-00001 … KH-2026-00100 — nếu trùng mã trong DB, sửa prefix hoặc xóa bản ghi cũ trước.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @i INT = 1;
DECLARE @now DATETIME2 = SYSUTCDATETIME();

-- Pipeline stages (seed cố định)
DECLARE @s1 UNIQUEIDENTIFIER = '038433b7-aacd-478a-8279-5d8ae9c4ab87';
DECLARE @s2 UNIQUEIDENTIFIER = '23c69eca-4a71-414a-b8a0-daead0b4787b';
DECLARE @s3 UNIQUEIDENTIFIER = '2e63017b-0a49-40ff-84da-ef6213a8f2fb';
DECLARE @s4 UNIQUEIDENTIFIER = '53901964-4ec6-44a4-aacf-071709ba8aea';
DECLARE @s5 UNIQUEIDENTIFIER = 'e69e7e51-77f9-45e8-b920-c144c413d1a6';
DECLARE @s6 UNIQUEIDENTIFIER = '1d5c677b-9a3f-48ac-801f-9813ef5bc408';

DECLARE @n1 NVARCHAR(200) = N'Khách hàng tiềm năng mới';
DECLARE @n2 NVARCHAR(200) = N'Đang tiếp cận';
DECLARE @n3 NVARCHAR(200) = N'Có nhu cầu thực';
DECLARE @n4 NVARCHAR(200) = N'Báo giá';
DECLARE @n5 NVARCHAR(200) = N'Chốt giao dịch';
DECLARE @n6 NVARCHAR(200) = N'Hậu mãi';

-- Customer types (seed cố định)
DECLARE @tB2B UNIQUEIDENTIFIER = '3a3f0c20-7d3c-4a67-9b33-9e1f7a8c4f01';
DECLARE @tB2C UNIQUEIDENTIFIER = '3a3f0c20-7d3c-4a67-9b33-9e1f7a8c4f02';
DECLARE @tB2BName NVARCHAR(200) = N'Khách hàng Doanh nghiệp (B2B)';
DECLARE @tB2CName NVARCHAR(200) = N'Khách hàng Cá nhân (B2C)';

WHILE @i <= 100
BEGIN
  DECLARE @id UNIQUEIDENTIFIER = NEWID();
  DECLARE @code NVARCHAR(64) = N'KH-2026-' + RIGHT(REPLICATE(N'0', 5) + CAST(@i AS NVARCHAR(5)), 5);

  DECLARE @ho NVARCHAR(32) = CHOOSE(
    ((@i - 1) % 15) + 1,
    N'Nguyễn', N'Trần', N'Lê', N'Phạm', N'Hoàng', N'Phan', N'Vũ', N'Võ',
    N'Đặng', N'Bùi', N'Đỗ', N'Hồ', N'Ngô', N'Dương', N'Đinh'
  );

  DECLARE @lot NVARCHAR(64) = CHOOSE(
    ((@i + 3) % 18) + 1,
    N'Văn An', N'Thị Mai', N'Minh Tuấn', N'Thị Lan', N'Quốc Huy', N'Thị Hương',
    N'Đức Thắng', N'Thị Ngọc', N'Văn Nam', N'Thị Phương', N'Hoàng Long', N'Thị Linh',
    N'Văn Khôi', N'Thị Hà', N'Ngọc Anh', N'Thị Trang', N'Quang Minh', N'Thị Yến'
  );

  DECLARE @name NVARCHAR(256) = @ho + N' ' + @lot;

  DECLARE @isB2B BIT = CASE WHEN (@i % 2) = 0 THEN 1 ELSE 0 END;
  DECLARE @typeId UNIQUEIDENTIFIER = CASE WHEN @isB2B = 1 THEN @tB2B ELSE @tB2C END;
  DECLARE @typeName NVARCHAR(200) = CASE WHEN @isB2B = 1 THEN @tB2BName ELSE @tB2CName END;

  DECLARE @company NVARCHAR(256) = CASE @isB2B
    WHEN 1 THEN N'Công ty TNHH ' + CHOOSE((@i % 8) + 1,
      N'Thành Công', N'Ánh Dương', N'Hoàng Gia', N'Minh Anh', N'Đại Việt', N'Kim Ngân', N'Phú Quý', N'Thịnh Vượng')
    ELSE NULL
  END;

  DECLARE @tax NVARCHAR(32) = CASE @isB2B
    WHEN 1 THEN RIGHT(REPLICATE(N'0', 10) + CAST(3000000000 + @i AS NVARCHAR(20)), 10)
    ELSE NULL
  END;

  DECLARE @phone NVARCHAR(32) = N'09' + RIGHT(REPLICATE(N'0', 8) + CAST(10000000 + (@i * 7919) % 90000000 AS NVARCHAR(16)), 8);

  DECLARE @email NVARCHAR(256) = LOWER(
    REPLACE(REPLACE(REPLACE(@ho + N'.' + @lot, N' ', N''), N'ị', N'i'), N'đ', N'd')
  ) + CAST(@i AS NVARCHAR(10)) + N'@example.vn';

  DECLARE @city NVARCHAR(128) = CHOOSE(
    ((@i + 5) % 12) + 1,
    N'Hà Nội', N'TP. Hồ Chí Minh', N'Đà Nẵng', N'Hải Phòng', N'Cần Thơ', N'Nha Trang',
    N'Huế', N'Vũng Tàu', N'Bắc Ninh', N'Đồng Nai', N'Bình Dương', N'Quảng Ninh'
  );

  DECLARE @addr NVARCHAR(512) = N'Số ' + CAST((@i * 17) % 200 + 1 AS NVARCHAR(10)) + N', đường '
    + CHOOSE((@i % 6) + 1, N'Nguyễn Huệ', N'Lê Lợi', N'Trần Hưng Đạo', N'Lý Thường Kiệt', N'Phan Chu Trinh', N'Hai Bà Trưng')
    + N', phường ' + CHOOSE((@i % 5) + 1, N'Bến Nghé', N'Đa Kao', N'Cầu Giấy', N'Hai Bà Trưng', N'Lê Chân')
    + N', ' + @city;

  DECLARE @gender NVARCHAR(16) = CHOOSE((@i % 3) + 1, N'Nam', N'Nữ', N'Khác');

  DECLARE @dob DATETIME2 = DATEADD(DAY, -((@i * 137) % 20000), CAST('1990-01-01' AS DATETIME2));

  DECLARE @stageIdx INT = ((@i - 1) % 6) + 1;
  DECLARE @statusId UNIQUEIDENTIFIER = CHOOSE(
    @stageIdx,
    @s1, @s2, @s3, @s4, @s5, @s6
  );
  DECLARE @statusName NVARCHAR(200) = CHOOSE(
    @stageIdx,
    @n1, @n2, @n3, @n4, @n5, @n6
  );

  DECLARE @job NVARCHAR(256) = CHOOSE(
    ((@i + 1) % 10) + 1,
    N'Giám đốc', N'Trưởng phòng Kinh doanh', N'Kế toán trưởng', N'Nhân viên mua hàng',
    N'Chuyên viên IT', N'Kỹ sư', N'Chủ cửa hàng', N'Freelancer', N'Nhân viên hành chính', N'Giám sát dự án'
  );

  DECLARE @src NVARCHAR(256) = CHOOSE(
    ((@i + 2) % 8) + 1,
    N'Website', N'Facebook', N'Zalo OA', N'Giới thiệu', N'Hội chợ', N'Google Ads', N'Đối tác', N'Khác'
  );

  DECLARE @zalo NVARCHAR(256) = CASE WHEN (@i % 4) = 0 THEN @phone ELSE NULL END;

  DECLARE @note NVARCHAR(4000) = N'Khách hàng mẫu #' + CAST(@i AS NVARCHAR(10)) + N' — liên hệ ưu tiên buổi sáng.';

  INSERT INTO dbo.Customers (
    Id,
    Code,
    Name,
    CompanyName,
    TaxCode,
    Phone,
    Email,
    Address,
    Gender,
    DayOfBirth,
    City,
    CustomerStatusId,
    CustomerStatusName,
    CustomerTypeId,
    CustomerTypeName,
    Note,
    Zalo,
    JobTitle,
    Source,
    CreatedAt,
    ModifiedAt,
    CreatedBy,
    ModifiedBy,
    IsDeleted
  )
  VALUES (
    @id,
    @code,
    @name,
    @company,
    @tax,
    @phone,
    @email,
    @addr,
    @gender,
    @dob,
    @city,
    @statusId,
    @statusName,
    @typeId,
    @typeName,
    @note,
    @zalo,
    @job,
    @src,
    @now,
    @now,
    NULL,
    NULL,
    0
  );

  SET @i += 1;
END;

COMMIT TRANSACTION;

PRINT N'Đã chèn 100 khách hàng (KH-2026-00001 … KH-2026-00100).';
