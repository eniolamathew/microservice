using Microsoft.AspNetCore.Mvc;
using BrandService.Models.Domain;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using MicroServices.API.Common;
using BrandService.Interfaces;

namespace BrandService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IBrandDomain _brandRepo;

        public BrandController(IBrandDomain brandRepo, IMapper mapper)
        {
            _brandRepo = brandRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResult<IEnumerable<BrandDomainEntity>>>> GetAllBrands()
        {
            var brands = await _brandRepo.GetAllBrandsAsync();

            if (brands == null || !brands.Any())
            {
                var errorResult = ApiResultHelper.ErrorResult<IEnumerable<BrandDomainEntity>>("No brands found", 404);
                return NotFound(errorResult);
            }

            var result = ApiResultHelper.SuccessResult(brands);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<BrandDomainEntity>>> GetBrandById(int id)
        {
            var brand = await _brandRepo.GetBrandByIdAsync(id);

            if (brand == null)
            {
                var errorResult = ApiResultHelper.ErrorResult<BrandDomainEntity>("Brand not found", 404);
                return NotFound(errorResult);
            }

            var result = ApiResultHelper.SuccessResult(brand);
            return Ok(result);
        }

        // POST api/[controller]
        [HttpPost]
        public async Task<ActionResult<ApiResult<BrandDomainEntity>>> AddBrand([FromBody] BrandAddDomainEntity brand)
        {
            // If the brand data is invalid (null)
            if (brand == null)
            {
                var errorResult = ApiResultHelper.ErrorResult<BrandDomainEntity>("Brand data is required.", 400);
                return BadRequest(errorResult); // Return BadRequest with ApiResult
            }

            // Add brand to the repository
            var addedBrand = await _brandRepo.AddBrandAsync(brand);

            // Return created platform result with success message
            var result = ApiResultHelper.SuccessResult(addedBrand, "Brand added successfully", 201);
            return CreatedAtAction(nameof(GetBrandById), new { id = addedBrand.Id }, result); 
        }


        // PUT api/[controller]/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBrand(int id, [FromBody] BrandUpdateDomainEntity brand)
        {
            // Validate brand data
            if (brand == null || brand.Id != id)
            {
                var errorResult = ApiResultHelper.ErrorResult<BrandDomainEntity>("Invalid brand data.", 400);
                return BadRequest(errorResult);
            }

            // Retrieve brand by ID to ensure it exists
            var existingBrand = await _brandRepo.GetBrandByIdAsync(id);
            if (existingBrand == null)
            {
                var errorResult = ApiResultHelper.ErrorResult<BrandDomainEntity>("Brand not found", 404);
                return NotFound(errorResult);
            }

            // Update brand in the repository
            await _brandRepo.UpdateBrandAsync(brand);

            // Return success response
            var successResult = ApiResultHelper.SuccessResult(brand, "Brand updated successfully", 200);
            return Ok(successResult);
        }

        [HttpPost("getbyids")]
        public async Task<ActionResult<ApiResult<object>>> GetBrandsByIds([FromBody] List<int> ids)
        {
            // Check if the IDs are null or empty
            if (ids == null || !ids.Any())
            {
                var errorResult = ApiResultHelper.ErrorResult<object>("No brand IDs were provided.");
                return BadRequest(errorResult);
            }

            // Get the brand entities from the database (this could be null, so we handle it)
            var brandEntities = await _brandRepo.GetByIdsAsync(ids);

            // Check if the result is null or empty
            if (brandEntities == null)
            {
                var errorResult = ApiResultHelper.ErrorResult<object>("Error retrieving brands.");
                return BadRequest(errorResult);
            }

            var foundIds = brandEntities.Select(p => p.Id).ToList();
            var missingIds = ids.Except(foundIds).ToList();
            var brandDomainEntities = brandEntities.Select(brand => _mapper.Map<BrandDomainEntity>(brand)).ToList();

            var payload = new
            {
                Found = brandDomainEntities,
                Missing = missingIds
            };

            var resultMessage = missingIds != null && missingIds.Any() ? "Some brands were not found." : null;

            var result = ApiResultHelper.SuccessResult(payload, resultMessage);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            // Retrieve brand by ID to check if it exists
            var existingBrand = await _brandRepo.GetBrandByIdAsync(id);
            if (existingBrand == null)
            {
                var errorResult = ApiResultHelper.ErrorResult<BrandDomainEntity>("Brand not found", 404);
                return NotFound(errorResult);
            }

            // Delete the brand from the repository
            await _brandRepo.DeleteBrandAsync(id);

            // Return success response with no data
            var successResult = ApiResultHelper.SuccessResult<string?>(null, "Brand deleted successfully", 204);
            return Ok(successResult); // Return Ok with ApiResult
        }
    }
}
