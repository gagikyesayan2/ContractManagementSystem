using ContractManagementSystem.Api.Extensions;
using ContractManagementSystem.API.Mapping;
using ContractManagementSystem.Business.Mapping;
using ContractManagementSystem.Data.Context;
using ContractManagementSystem.Data.Interfaces.Common;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("Default");


builder.Services.AddControllers();


builder.Services.AddSingleton<IAppDbContext>(new AppDbContext(cs));

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(BusinessMappingProfile).Assembly);
    // later if we create API profile:
    cfg.AddMaps(typeof(ApiMappingProfile).Assembly);
});




builder.Services.AddApplicationServices();
builder.Services.AddRepositories();

builder.Services.AddSwaggerWithJwt();
builder.Services.AddJwtAuthentication(builder.Configuration);



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) { 

    app.UseSwagger();
    app.UseSwaggerUI();
}
Console.WriteLine(app.Environment.EnvironmentName);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
