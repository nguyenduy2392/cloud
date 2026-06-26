-- Script thêm 50 tương tác cho khách hàng có customerId = '35ef3191-2201-4c0b-9140-013d24c373bb'
-- Thực thi trên database MiniCRM

DECLARE @CustomerId UNIQUEIDENTIFIER = '35ef3191-2201-4c0b-9140-013d24c373bb';
DECLARE @EmployeeId UNIQUEIDENTIFIER; -- Sẽ chọn ngẫu nhiên từ bảng Users
DECLARE @StartDate DATETIME = DATEADD(DAY, -180, GETDATE()); -- 180 ngày trước
DECLARE @InteractionId UNIQUEIDENTIFIER;
DECLARE @i INT = 0;
DECLARE @RandomDays INT;
DECLARE @RandomMethod INT;
DECLARE @InteractionDate DATETIME;
DECLARE @HasFollowUp BIT;
DECLARE @FollowUpDate DATETIME;

-- Lấy một EmployeeId hợp lệ từ bảng Users (nếu có)
SELECT TOP 1 @EmployeeId = Id FROM AppUsers WHERE IsDeleted = 0;
IF @EmployeeId IS NULL
BEGIN
    PRINT 'Không tìm thấy Employee hợp lệ trong bảng AppUsers';
    RETURN;
END

-- Tạo 50 bản ghi tương tác
WHILE @i < 50
BEGIN
    SET @InteractionId = NEWID();
    SET @RandomDays = CAST(RAND(CHECKSUM(NEWID())) * 180 AS INT); -- Ngẫu nhiên 0-180 ngày
    SET @InteractionDate = DATEADD(DAY, -@RandomDays, GETDATE());
    SET @RandomMethod = CAST(RAND(CHECKSUM(NEWID())) * 5 AS INT); -- Ngẫu nhiên 0-5 (EnumInteraction)
    SET @HasFollowUp = CASE WHEN RAND(CHECKSUM(NEWID())) > 0.6 THEN 1 ELSE 0 END; -- 40% có follow-up

    IF @HasFollowUp = 1
        SET @FollowUpDate = DATEADD(DAY, CAST(RAND(CHECKSUM(NEWID())) * 30 AS INT) + 1, @InteractionDate);
    ELSE
        SET @FollowUpDate = NULL;

    INSERT INTO CustomerInteractions (
        Id,
        CustomerId,
        EmployeeId,
        Method,
        InteractionDate,
        Content,
        FollowUpDate,
        NextStep,
        CreatedAt,
        CreatedBy,
        ModifiedAt,
        ModifiedBy,
        IsDeleted
    )
    VALUES (
        @InteractionId,
        @CustomerId,
        @EmployeeId,
        @RandomMethod,
        @InteractionDate,
        CASE @RandomMethod
            WHEN 0 THEN N'Gọi điện tư vấn sản phẩm dịch vụ cho khách hàng. Khách hàng quan tâm và muốn được gửi báo giá chi tiết.'
            WHEN 1 THEN N'Nhắn tin SMS cảm ơn khách hàng đã quan tâm đến sản phẩm. Gửi kèm link bài viết giới thiệu.'
            WHEN 2 THEN N'Gửi email báo giá chi tiết theo yêu cầu của khách hàng. Đính kèm catalog sản phẩm mới nhất.'
            WHEN 3 THEN N'Nhắn tin Zalo giới thiệu chương trình khuyến mãi tháng này. Khách hàng phản hồi tích cực.'
            WHEN 4 THEN N'Gọi điện Zalo để xác nhận thông tin đơn hàng và trao đổi về thời gian giao hàng.'
            WHEN 5 THEN N'Gặp trực tiếp tại showroom để trải nghiệm sản phẩm. Khách hàng rất hài lòng với chất lượng.'
        END,
        @FollowUpDate,
        CASE @RandomMethod
            WHEN 0 THEN N'Gửi báo giá chi tiết qua email'
            WHEN 1 THEN N'Tiếp tục follow up sau 3 ngày'
            WHEN 2 THEN N'Gọi điện xác nhận đã nhận email'
            WHEN 3 THEN N'Gửi mẫu thử miễn phí'
            WHEN 4 THEN N'Chuẩn bị hợp đồng và hẹn ký'
            WHEN 5 THEN N'Hẹn gặp lại để ký hợp đồng'
        END,
        GETDATE(),
        @EmployeeId,
        GETDATE(),
        @EmployeeId,
        0
    );

    SET @i = @i + 1;
END

-- Kiểm tra kết quả
SELECT COUNT(*) AS TotalInteractions
FROM CustomerInteractions
WHERE CustomerId = @CustomerId AND IsDeleted = 0;

PRINT 'Đã thêm thành công 50 bản ghi tương tác cho CustomerId: ' + CAST(@CustomerId AS VARCHAR(36));
