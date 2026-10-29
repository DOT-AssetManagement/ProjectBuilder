using ProjectBuilder.Models.ScenarioParamater;
using ProjectBuilder.Models.ScenarioParamater.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using ProjectBuilder.Permissions;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Services
{
    [Authorize(ScenarioPermissions.Edit)]
    public class ScenarioParameterAppService : ApplicationService, IScenarioParameterAppService
    {
     
    }
}
