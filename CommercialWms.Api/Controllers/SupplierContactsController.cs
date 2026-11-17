using CommercialWms.Api.Contracts.Suppliers;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/suppliers/{supplierId:int}/contacts")]
public sealed class SupplierContactsController(CommercialWmsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SupplierContactResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SupplierContactResponse>>> GetContacts(
        int supplierId,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Suppliers.AnyAsync(supplier => supplier.SupplierId == supplierId, cancellationToken))
        {
            return NotFound();
        }

        var contacts = await dbContext.SupplierContacts
            .AsNoTracking()
            .Where(contact => contact.SupplierId == supplierId)
            .OrderBy(contact => contact.ContactId)
            .Select(contact => ToResponse(contact))
            .ToListAsync(cancellationToken);

        return Ok(contacts);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SupplierContactResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierContactResponse>> CreateContact(
        int supplierId,
        SupplierContactCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Suppliers.AnyAsync(supplier => supplier.SupplierId == supplierId, cancellationToken))
        {
            return NotFound();
        }

        var contact = new SupplierContact
        {
            SupplierId = supplierId,
            ContactName = request.ContactName.Trim(),
            JobTitle = NormalizeOptional(request.JobTitle),
            Email = NormalizeOptional(request.Email),
            Phone = NormalizeOptional(request.Phone)
        };

        dbContext.SupplierContacts.Add(contact);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = ToResponse(contact);
        return CreatedAtAction(nameof(GetContacts), new { supplierId }, response);
    }

    [HttpPut("{contactId:int}")]
    [ProducesResponseType(typeof(SupplierContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierContactResponse>> UpdateContact(
        int supplierId,
        int contactId,
        SupplierContactUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var contact = await dbContext.SupplierContacts
            .SingleOrDefaultAsync(
                item => item.SupplierId == supplierId && item.ContactId == contactId,
                cancellationToken);
        if (contact is null)
        {
            return NotFound();
        }

        contact.ContactName = request.ContactName.Trim();
        contact.JobTitle = NormalizeOptional(request.JobTitle);
        contact.Email = NormalizeOptional(request.Email);
        contact.Phone = NormalizeOptional(request.Phone);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(contact));
    }

    [HttpDelete("{contactId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContact(
        int supplierId,
        int contactId,
        CancellationToken cancellationToken)
    {
        var contact = await dbContext.SupplierContacts
            .SingleOrDefaultAsync(
                item => item.SupplierId == supplierId && item.ContactId == contactId,
                cancellationToken);
        if (contact is null)
        {
            return NotFound();
        }

        dbContext.SupplierContacts.Remove(contact);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static SupplierContactResponse ToResponse(SupplierContact contact)
    {
        return new SupplierContactResponse(
            contact.ContactId,
            contact.SupplierId,
            contact.ContactName,
            contact.JobTitle,
            contact.Email,
            contact.Phone);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
