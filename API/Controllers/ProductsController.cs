using API.RequestHelpers;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


public class ProductsController(IUnitOfWork unitOfWork): BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts([FromQuery] ProductSpecParams specParams)
    {
        var spec = new ProductSpecification(specParams);

        return await CreatePagedResult(unitOfWork.Repository<Product>(), spec, specParams.PageIndex, specParams.PageSize);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await unitOfWork.Repository<Product>().GetByIdAsync(id);

        if (product == null) return NotFound();

        return product;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        unitOfWork.Repository<Product>().Add(product);

        if(await unitOfWork.Complete())
        {
            return CreatedAtAction(nameof(GetProduct),new {id=product.Id}, product);
        }

        return BadRequest("Problem creating product");
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateProduct(int id, Product product)
    {
        if(id!=product.Id || !ProductExists(id)) return BadRequest("Cannot update this product");

        unitOfWork.Repository<Product>().Update(product);

        if(await unitOfWork.Complete())
        {
            return NoContent();
        }

        return BadRequest("Problem updating product");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var product = await unitOfWork.Repository<Product>().GetByIdAsync(id);

        if (product == null) return NotFound();

        unitOfWork.Repository<Product>().Remove(product);

        if(await unitOfWork.Complete())
        {
            return NoContent();
        }

        return BadRequest("Problem deleting product");
    }

    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetBrangs()
    {
        var spec = new BrandListSpecification();

        var brands = await unitOfWork.Repository<Product>().ListAsync(spec);

        return Ok(brands);
    }

    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
    {
        var spec = new TypeListSpecification();

        var types = await unitOfWork.Repository<Product>().ListAsync(spec);

        return Ok(types);
    }

    private bool ProductExists(int id)
    {
        return unitOfWork.Repository<Product>().Exists(id);
    }
}
