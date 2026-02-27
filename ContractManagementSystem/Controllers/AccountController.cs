using AutoMapper;

using ContractManagementSystem.Business.DTOs.Account;
using ContractManagementSystem.Business.Interfaces;
using ContractManagementSystem.Shared.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ContractManagementSystem.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AccountsController(IAccountService accountService, IMapper mapper) : ControllerBase
{
    [HttpPost("signup")]
    [AllowAnonymous]
    public async Task<ActionResult> SignUp([FromBody] SignUpRequestModel requestModel)
    {
        
        var requestDto = mapper.Map<SignUpRequestDto>(requestModel);
        var result = await accountService.SignUpAsync(requestDto);
      
        return Ok(result);
    }

    [HttpPost("signin")]
    [AllowAnonymous]
    public async Task<ActionResult<RefreshTokenResponseModel>> SignIn([FromBody] SignInRequestModel requestModel)
    {
        var requestDto = mapper.Map<SignInRequestDto>(requestModel);
        var resultDto = await accountService.SignInAsync(requestDto);
        var resultModel = mapper.Map<RefreshTokenResponseModel>(resultDto);

        return Ok(resultModel);
    }
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<RefreshTokenResponseModel>> Refresh(
    [FromBody] RefreshTokenRequestModel model)
    {
        if (string.IsNullOrWhiteSpace(model.RefreshToken))
            return BadRequest("Refresh token is required.");

        var dto = mapper.Map<RefreshTokenRequestDto>(model);

        var resultDto = await accountService.ValidateRefreshTokenAsync(dto);

        var resultModel = mapper.Map<RefreshTokenResponseModel>(resultDto);

        return Ok(resultModel);
    }
}