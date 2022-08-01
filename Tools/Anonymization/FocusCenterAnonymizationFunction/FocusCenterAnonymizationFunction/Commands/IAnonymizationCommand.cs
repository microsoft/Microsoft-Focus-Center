using AnonymizationFunctionApp.Interfaces;
using AnonymizationFunctionApp.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AnonymizationFunctionApp.Commands
{
    public interface IAnonymizationCommand 
    {
        public Task<CommandResult> Execute(InputRequestModel input);
    }
}
