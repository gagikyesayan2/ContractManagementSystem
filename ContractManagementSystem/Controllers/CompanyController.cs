using AutoMapper;
using ContractManagementSystem.API.Extensions;
using ContractManagementSystem.API.Models.Company;
using ContractManagementSystem.Business.DTOs;
using ContractManagementSystem.Business.DTOs.Company;
using ContractManagementSystem.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContractManagementSystem.API.Controllers;


[ApiController]
[Route("api/companies")]
public class CompanyController (ICompanyService companyService, IMapper mapper) : ControllerBase
{
    [Authorize]
    [HttpPost("register-company")]
    public async Task<ActionResult<CreateCompanyResponseModel>> Create([FromBody] CreateCompanyRequestModel requestModel, CancellationToken ct)
    {
        var accountId = User.GetAccountId();

       
        var requestDto = mapper.Map<CreateCompanyRequestDto>(requestModel);

        var resultDto = await companyService.CreateCompanyAsync(requestDto, accountId, ct);

        var resultModel = mapper.Map<CreateCompanyResponseModel>(resultDto); 

        return Ok(resultModel);
    }


    [Authorize(Roles = "Admin")]
    [HttpPost("{companyId:guid}/employees")]
    public async Task<ActionResult<RegisterEmployeeResponseModel>> RegisterEmployee(Guid companyId, [FromBody] RegisterEmployeeRequestModel requestModel,CancellationToken ct)
    {
        var adminAccountId = User.GetAccountId();

        var requestDto = mapper.Map<RegisterEmployeeRequestDto>(requestModel);
        requestDto.CompanyId = companyId;

        var resultDto = await companyService.RegisterEmployeeAsync(requestDto,adminAccountId,ct);

        var resultModel = mapper.Map<RegisterEmployeeResponseModel>(resultDto);

        return Ok(resultModel);
    }
}
