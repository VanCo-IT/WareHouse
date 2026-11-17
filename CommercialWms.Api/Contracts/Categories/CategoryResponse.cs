namespace CommercialWms.Api.Contracts.Categories;

public sealed record CategoryResponse(
    int CategoryId,
    string CategoryCode,
    string CategoryName,
    int? ParentCategoryId,
    bool IsActive);
