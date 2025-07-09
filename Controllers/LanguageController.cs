using Microsoft.AspNetCore.Mvc;
using TRT_backend.Models.DTO;
using TRT_backend.Services;

namespace TRT_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LanguageController : ControllerBase
    {
        private readonly ILanguageService _languageService;

        public LanguageController(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        [Tags("LanguageManagement")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LanguageDto>>> GetAll()
        {
            var languages = await _languageService.GetAllLanguagesAsync();
            return Ok(languages);
        }

        [Tags("LanguageManagement")]
        [HttpGet("claim-language/{claimId}/{languageId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClaimLanguageDto>> GetClaimLanguageByClaimIdAndLanguageId(int claimId, int languageId)
        {
            var claimLanguage = await _languageService.GetClaimLanguageByClaimIdAndLanguageIdAsync(claimId, languageId);
            if (claimLanguage == null)
            {
                return NotFound("İlgili claim ve dil için çeviri bulunamadı.");
            }
            return Ok(claimLanguage);
        }
    }
} 