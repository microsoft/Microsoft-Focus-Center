using System;
using System.Collections.Generic;
using System.Text;

namespace AnonymizationFunctionApp.Models
{
    public class CommandResult<T> : CommandResult
    {
        public CommandResult(CommandStatusCode status, T payload, string message = null) : base(status, message)
        {
            Payload = payload;
        }

        public T Payload { get; }
    }

    public class CommandResult
    {
        public CommandResult(CommandStatusCode status, string message = null)
        {
            Status = status;
            Message = message;
        }

        public CommandStatusCode Status { get; }

        public string Message { get; }
    }

    public enum CommandStatusCode
    {
        Succeeded = 0,
        Failed = 1
    }
}
