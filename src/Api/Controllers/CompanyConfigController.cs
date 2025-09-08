using Application.Dtos;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/company-config")]
[Authorize(Roles = "admin")] // solo admin puede ver/editar config
public class CompanyConfigController(ICompanyConfigService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CompanyConfigDto>> Get(CancellationToken ct) => Ok(await service.GetAsync(ct));

    [HttpPut]
    public async Task<ActionResult<CompanyConfigDto>> Update([FromBody] UpdateCompanyConfigRequest req, CancellationToken ct) => Ok(await service.UpdateAsync(req, ct));
}

