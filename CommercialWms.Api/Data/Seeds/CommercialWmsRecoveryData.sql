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

DECLARE @FrozenFoodCategoryId int;
SELECT @FrozenFoodCategoryId = CategoryId
FROM dbo.ProductCategories
WHERE CategoryCode = 'FROZEN_FOOD';

IF @FrozenFoodCategoryId IS NULL
BEGIN
    THROW 51000, 'The FROZEN_FOOD product category could not be resolved.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM dbo.Products
    WHERE SKU = 'FROZEN-PORK-001'
      AND (SKU IS NULL
           OR SKU <> 'FROZEN-PORK-001'
           OR ProductName IS NULL
           OR ProductName <> N'Frozen Pork'
           OR CategoryId IS NULL
           OR CategoryId <> @FrozenFoodCategoryId
           OR Description IS NULL
           OR Description <> N'Frozen pork from Vietnam'
           OR IsPerishable IS NULL
           OR IsPerishable <> 1
           OR IsActive IS NULL
           OR IsActive <> 1)
)
BEGIN
    ROLLBACK TRANSACTION;
    THROW 51001, 'FROZEN-PORK-001 already exists with different recovery values; no product data was changed.', 1;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Products WITH (UPDLOCK, HOLDLOCK)
    WHERE SKU = 'FROZEN-PORK-001'
)
BEGIN
    INSERT INTO dbo.Products (SKU, ProductName, CategoryId, Description, IsPerishable, IsActive)
    VALUES ('FROZEN-PORK-001', N'Frozen Pork', @FrozenFoodCategoryId, N'Frozen pork from Vietnam', 1, 1);
END;

COMMIT TRANSACTION;
