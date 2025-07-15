using Microsoft.AspNetCore.Mvc;
using SupplierService.Models.Domain;
using SupplierService.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using MicroServices.API.Common;

namespace SupplierService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ISupplierDomain _supplierRepo;

        public SupplierController(ISupplierDomain supplierRepo, IMapper mapper)
        {
            _supplierRepo = supplierRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResult<IEnumerable<SupplierDomainEntity>>>> GetAllSuppliers()
        {
            var suppliers = await _supplierRepo.GetAllSuppliersAsync();

            if (suppliers == null || !suppliers.Any())
            {
                var errorResult = ApiResultHelper.ErrorResult<IEnumerable<SupplierDomainEntity>>("No suppliers found", 404);
                return NotFound(errorResult);
            }

            var result = ApiResultHelper.SuccessResult(suppliers);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<SupplierDomainEntity>>> GetSupplierById(int id)
        {
            var supplier = await _supplierRepo.GetSupplierByIdAsync(id);

            if (supplier == null)
            {
                var errorResult = ApiResultHelper.ErrorResult<SupplierDomainEntity>("Supplier not found", 404);
                return NotFound(errorResult);
            }

            var result = ApiResultHelper.SuccessResult(supplier);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResult<SupplierDomainEntity>>> AddSupplier([FromBody] SupplierAddDomainEntity supplier)
        {
            if (supplier == null)
            {
                var errorResult = ApiResultHelper.ErrorResult<SupplierDomainEntity>("Supplier data is required.", 400);
                return BadRequest(errorResult);
            }

            var addedSupplier = await _supplierRepo.AddSupplierAsync(supplier);

            var result = ApiResultHelper.SuccessResult(addedSupplier, "Supplier added successfully", 201);
            return CreatedAtAction(nameof(GetSupplierById), new { id = addedSupplier.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] SupplierUpdateDomainEntity supplier)
        {
            if (supplier == null || supplier.Id != id)
            {
                var errorResult = ApiResultHelper.ErrorResult<SupplierDomainEntity>("Invalid supplier data.", 400);
                return BadRequest(errorResult);
            }

            var existingSupplier = await _supplierRepo.GetSupplierByIdAsync(id);
            if (existingSupplier == null)
            {
                var errorResult = ApiResultHelper.ErrorResult<SupplierDomainEntity>("Supplier not found", 404);
                return NotFound(errorResult);
            }

            await _supplierRepo.UpdateSupplierAsync(supplier);

            var successResult = ApiResultHelper.SuccessResult(supplier, "Supplier updated successfully", 200);
            return Ok(successResult);
        }

        [HttpPost("getbyids")]
        public async Task<ActionResult<ApiResult<object>>> GetSuppliersByIds([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                var errorResult = ApiResultHelper.ErrorResult<object>("No supplier IDs were provided.");
                return BadRequest(errorResult);
            }

            var supplierEntities = await _supplierRepo.GetByIdsAsync(ids);

            if (supplierEntities == null)
            {
                var errorResult = ApiResultHelper.ErrorResult<object>("Error retrieving suppliers.");
                return BadRequest(errorResult);
            }

            var foundIds = supplierEntities.Select(s => s.Id).ToList();
            var missingIds = ids.Except(foundIds).ToList();
            var supplierDomainEntities = supplierEntities.Select(s => _mapper.Map<SupplierDomainEntity>(s)).ToList();

            var payload = new
            {
                Found = supplierDomainEntities,
                Missing = missingIds
            };

            var resultMessage = missingIds != null && missingIds.Any() ? "Some suppliers were not found." : null;

            var result = ApiResultHelper.SuccessResult(payload, resultMessage);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var existingSupplier = await _supplierRepo.GetSupplierByIdAsync(id);
            if (existingSupplier == null)
            {
                var errorResult = ApiResultHelper.ErrorResult<SupplierDomainEntity>("Supplier not found", 404);
                return NotFound(errorResult);
            }

            await _supplierRepo.DeleteSupplierAsync(id);

            var successResult = ApiResultHelper.SuccessResult<string>(null, "Supplier deleted successfully", 204);
            return Ok(successResult);
        }
    }
}
