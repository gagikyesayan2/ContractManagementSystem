using Blazored.LocalStorage;
using ContractManagementSystem.Shared.Models;
using ContractManagementSystem.Shared.Models.Account;
using ContractManagementSystem.Shared.Models.Company;
using ContractManagementSystem.Shared.Models.Employee;
using System.Net.Http.Json;

namespace ClientSide.Services;

public sealed class CompanyApiService
{
    private readonly HttpClient _http;
    private readonly AuthApiClient _authApi;
    private readonly ILocalStorageService _localStorage;
    public CompanyApiService(HttpClient http, ILocalStorageService localStorage, AuthApiClient authApiClient)
    {
        _http = http;
        _localStorage = localStorage;
        _authApi = authApiClient;
    }

    public async Task<CreateCompanyResponseModel> RegisterCompanyAsync(
    CreateCompanyRequestModel req,
    CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            throw new Exception("Company name is required.");

        req.Name = req.Name.Trim();

        var res = await _http.PostAsJsonAsync("api/companies/register-company", req, ct);

        // check success BEFORE parsing
        if (!res.IsSuccessStatusCode)
            throw new Exception("Company creation failed.");

        var body = await res.Content.ReadFromJsonAsync<CreateCompanyResponseModel>(cancellationToken: ct);

        if (body is null)
            throw new Exception("Company creation failed.");

        // refresh using your existing method (sends request body + saves tokens)
        var refreshToken = await _localStorage.GetItemAsync<string>("refresh_token", ct);

        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new Exception("Company created, but refresh token is missing.");

        await _authApi.RefreshAsync(new RefreshTokenRequestModel
        {
            RefreshToken = refreshToken
        }, ct);

        return body;
    }

    public async Task<List<CreateCompanyResponseModel>> GetMyCompaniesAsync(CancellationToken ct = default)
    {
        var res = await _http.GetAsync("api/companies/my-admin-companies", ct);

        // ✅ Employee (not admin) will get 403 -> treat as empty list
        if (res.StatusCode == System.Net.HttpStatusCode.Forbidden ||
            res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return new List<CreateCompanyResponseModel>();

        // other failures -> still show error
        if (!res.IsSuccessStatusCode)
            throw new Exception($"Failed to load companies. Status: {(int)res.StatusCode} {res.ReasonPhrase}");

        var body = await res.Content.ReadFromJsonAsync<List<CreateCompanyResponseModel>>(cancellationToken: ct);

        return body ?? new List<CreateCompanyResponseModel>();
    }


    public async Task<RegisterEmployeeResponseModel> RegisterEmployeeAsync(Guid companyId, RegisterEmployeeRequestModel req, CancellationToken ct = default)
    {
        if (companyId == Guid.Empty)
            throw new Exception("CompanyId is required.");

        if (string.IsNullOrWhiteSpace(req.Email) ||
            string.IsNullOrWhiteSpace(req.FirstName) ||
            string.IsNullOrWhiteSpace(req.LastName))
            throw new Exception("All fields are required.");

        // trim safety
        req.Email = req.Email.Trim();
        req.FirstName = req.FirstName.Trim();
        req.LastName = req.LastName.Trim();

        var res = await _http.PostAsJsonAsync(
            "api/companies/employees/register",
            req,
            ct);

        var body = await res.Content.ReadFromJsonAsync<RegisterEmployeeResponseModel>(
            cancellationToken: ct);

        if (!res.IsSuccessStatusCode || body is null)
            throw new Exception("Employee registration failed.");

        return body;
    }
}