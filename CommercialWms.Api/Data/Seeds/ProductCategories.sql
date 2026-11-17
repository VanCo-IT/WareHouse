SET XACT_ABORT ON;
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

BEGIN TRANSACTION;

INSERT INTO dbo.ProductCategories (CategoryCode, CategoryName, IsActive)
SELECT seed.CategoryCode, seed.CategoryName, 1
FROM (VALUES
    ('FROZEN_FOOD', N'Thực phẩm đông lạnh'),
    ('SEAFOOD', N'Hải sản'),
    ('CANDY_SNACK', N'Bánh kẹo và đồ ăn vặt'),
    ('KOREAN_FOOD', N'Thực phẩm Hàn Quốc'),
    ('JAPANESE_FOOD', N'Thực phẩm Nhật Bản')
) AS seed (CategoryCode, CategoryName)
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.ProductCategories AS existing WITH (UPDLOCK, HOLDLOCK)
    WHERE existing.CategoryCode = seed.CategoryCode
);

COMMIT TRANSACTION;
