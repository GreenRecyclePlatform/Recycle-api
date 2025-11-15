using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Common
{
    public class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }

        public static ServiceResult Success() => new ServiceResult { IsSuccess = true };
        public static ServiceResult Fail(string message) => new ServiceResult { IsSuccess = false, Message = message };
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }

        public static ServiceResult<T> Success(T data) => new ServiceResult<T> { IsSuccess = true, Data = data };
        public static new ServiceResult<T> Fail(string message) => new ServiceResult<T> { IsSuccess = false, Message = message };
    }

}

