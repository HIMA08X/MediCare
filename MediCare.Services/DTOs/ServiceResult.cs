using MediCare.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Services.DTOs
{
    public sealed record ServiceResult<T>(
     bool Succeeded,
     T? Data,
     ServiceError Error,
     string? Message)
    {
        public static ServiceResult<T> Success(T data) =>
            new(true, data, ServiceError.None, null);

        public static ServiceResult<T> Failure(
            ServiceError error,
            string message) =>
            new(false, default, error, message);
    }
}
