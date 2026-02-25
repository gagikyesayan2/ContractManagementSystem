using AutoMapper;
using ContractManagementSystem.API.Extensions;
using ContractManagementSystem.API.Models.Company.Contract;
using ContractManagementSystem.Application.Services;
using ContractManagementSystem.Business.DTOs.Company.Contract;
using ContractManagementSystem.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ContractManagementSystem.API.Controllers
{

    [ApiController]
    [Route("api/contracts")]
    public class ContractController(IContractService contractService, IMapper mapper) : ControllerBase
    {
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CreateContractResponseModel>> Create([FromBody] CreateContractRequestModel requestModel,CancellationToken ct)
        {
            var adminId = User.GetAccountId();

            var dto = mapper.Map<CreateContractRequestDto>(requestModel);

            var resultDto = await contractService.CreateContractAsync(adminId, dto, ct);

            var resultModel = mapper.Map<CreateContractResponseModel>(resultDto);
            
            return Ok(resultModel);

           
        }

        // PUT api/contracts/{contractId}
        [HttpPut("Edit{contractId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UpdateContractResponseModel>> Edit([FromRoute] Guid contractId,[FromBody] UpdateContractRequestModel requestUpdateModel,CancellationToken ct)
        {
            var adminId = User.GetAccountId();

            var dto = mapper.Map<UpdateContractRequestDto>(requestUpdateModel);
            dto.Id = contractId;
            var resultDto = await contractService.EditContractAsync(adminId, dto, ct);


            var resultModel = mapper.Map<UpdateContractResponseModel>(resultDto);   
            // Same idea: return 204 and frontend refetch
            return Ok(resultModel);

            
        }



        [HttpDelete("Delete/{contractId}")]
        public async Task<IActionResult> Delete(Guid contractId,[FromBody] DeleteContractRequestModel requestModel,
    CancellationToken ct)
        {
            var adminId = User.GetAccountId(); // your extension

            var requestDto = new DeleteContractRequestDto
            {
                CompanyId = requestModel.CompanyId,
                ContractId = contractId,
                DeletedByAdminAccountId = adminId
            };
           var deleted = await contractService.DeleteContractAsync(requestDto, ct);
           
            if (!deleted)
                return NotFound();   

            return NoContent();     
        }
    }
}
