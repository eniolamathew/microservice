using Microsoft.AspNetCore.Mvc;
using PlatformService.Models.Domain;
using PlatformService.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using MicroServices.API.Common;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IPlatformDomain _platformRepo;

        public PlatformController(IPlatformDomain platformRepo, IMapper mapper)
        {
            _platformRepo = platformRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResult<IEnumerable<PlatformDomainEntity>>>> GetAllPlatforms()
        {
            var platforms = await _platformRepo.GetAllPlatformsAsync();

            if (platforms == null || !platforms.Any())
            {
                var errorResult = ApiResultHelper.ErrorResult<IEnumerable<PlatformDomainEntity>>("No platforms found", 404);
                return NotFound(errorResult);
            }

            var result = ApiResultHelper.SuccessResult(platforms);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<PlatformDomainEntity>>> GetPlatformById(int id)
        {
            var platform = await _platformRepo.GetPlatformByIdAsync(id);

            if (platform == null)
            {
                var errorResult = ApiResultHelper.ErrorResult<PlatformDomainEntity>("Platform not found", 404);
                return NotFound(errorResult);
            }

            var result = ApiResultHelper.SuccessResult(platform);
            return Ok(result);
        }

        // POST api/[controller]
        [HttpPost]
        public async Task<ActionResult<ApiResult<PlatformDomainEntity>>> AddPlatform([FromBody] PlatformAddDomainEntity platform)
        {
            // If the platform data is invalid (null)
            if (platform == null)
            {
                var errorResult = ApiResultHelper.ErrorResult<PlatformDomainEntity>("Platform data is required.", 400);
                return BadRequest(errorResult); // Return BadRequest with ApiResult
            }

            // Add platform to the repository
            var addedPlatform = await _platformRepo.AddPlatformAsync(platform);

            // Return created platform result with success message
            var result = ApiResultHelper.SuccessResult(addedPlatform, "Platform added successfully", 201);
            return CreatedAtAction(nameof(GetPlatformById), new { id = addedPlatform.Id }, result); // Return Created with ApiResult
        }


        // PUT api/[controller]/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlatform(int id, [FromBody] PlatformUpdateDomainEntity platform)
        {
            // Validate platform data
            if (platform == null || platform.Id != id)
            {
                var errorResult = ApiResultHelper.ErrorResult<PlatformDomainEntity>("Invalid platform data.", 400);
                return BadRequest(errorResult); // Return BadRequest with ApiResult
            }

            // Retrieve platform by ID to ensure it exists
            var existingPlatform = await _platformRepo.GetPlatformByIdAsync(id);
            if (existingPlatform == null)
            {
                var errorResult = ApiResultHelper.ErrorResult<PlatformDomainEntity>("Platform not found", 404);
                return NotFound(errorResult);
            }

            // Update platform in the repository
            await _platformRepo.UpdatePlatformAsync(platform);

            // Return success response
            var successResult = ApiResultHelper.SuccessResult(platform, "Platform updated successfully", 200);
            return Ok(successResult);
        }

        [HttpPost("getbyids")]
        public async Task<ActionResult<ApiResult<object>>> GetPlatformsByIds([FromBody] List<int> ids)
        {
            // Check if the IDs are null or empty
            if (ids == null || !ids.Any())
            {
                var errorResult = ApiResultHelper.ErrorResult<object>("No platform IDs were provided.");
                return BadRequest(errorResult);
            }

            // Get the platform entities from the database (this could be null, so we handle it)
            var platformEntities = await _platformRepo.GetByIdsAsync(ids);

            // Check if the result is null or empty
            if (platformEntities == null)
            {
                var errorResult = ApiResultHelper.ErrorResult<object>("Error retrieving platforms.");
                return BadRequest(errorResult);
            }

            var foundIds = platformEntities.Select(p => p.Id).ToList();
            var missingIds = ids.Except(foundIds).ToList();
            var platformDomainEntities = platformEntities.Select(platform => _mapper.Map<PlatformDomainEntity>(platform)).ToList();

            var payload = new
            {
                Found = platformDomainEntities,
                Missing = missingIds
            };

            var resultMessage = missingIds != null && missingIds.Any() ? "Some platforms were not found." : null;

            var result = ApiResultHelper.SuccessResult(payload, resultMessage);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlatform(int id)
        {
            // Retrieve platform by ID to check if it exists
            var existingPlatform = await _platformRepo.GetPlatformByIdAsync(id);
            if (existingPlatform == null)
            {
                var errorResult = ApiResultHelper.ErrorResult<PlatformDomainEntity>("Platform not found", 404);
                return NotFound(errorResult);
            }

            // Delete the platform from the repository
            await _platformRepo.DeletePlatformAsync(id);

            // Return success response with no data
            var successResult = ApiResultHelper.SuccessResult<string>(null, "Platform deleted successfully", 204);
            return Ok(successResult); // Return Ok with ApiResult
        }
    }
}