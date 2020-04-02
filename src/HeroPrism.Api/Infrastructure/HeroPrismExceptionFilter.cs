using System;
using System.Linq;
using System.Net;
using FluentValidation;
using HeroPrism.Api.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HeroPrism.Api.Infrastructure
{
    public class HeroPrismExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            switch (context.Exception)
            {
                case ValidationException ex:
                    SetResponse(context, HttpStatusCode.BadRequest, ex.Errors.Select(c => new ErrorResponse(c.PropertyName, c.ErrorMessage)).ToArray());
                    break;
                case EntityNotFoundException ex:
                    SetResponse(context, HttpStatusCode.NotFound, new ErrorResponse("not_found", "Could not find requested data.  Check parameters."));
                    break;
                case UnauthorizedAccessException ex:
                    SetResponse(context, HttpStatusCode.Unauthorized, new ErrorResponse("unauthorized", "Unauthorized."));
                    break;
                case NoRegistrationException ex:
                    SetResponse(context, HttpStatusCode.Forbidden, new ErrorResponse("no_registration", "No registration."));
                    break;
                default:
                    SetResponse(context, HttpStatusCode.InternalServerError, new ErrorResponse("error", context.Exception.ToString()));
                    break;
            }
        }
        
        private static void SetResponse(ExceptionContext context, HttpStatusCode statusCode, params ErrorResponse[] errors)
        {
            context.HttpContext.Response.StatusCode = (int)statusCode;
            context.Result = new JsonResult(errors);
        }
    }
    
    public class ErrorResponse
    {
        public string Token { get; set; }
        public string Message { get; set; }

        public ErrorResponse(string token, string message)
        {
            Token = token;
            Message = message;
        }
    }
}