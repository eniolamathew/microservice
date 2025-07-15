using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServices.API.Common
{
    public class ApiResult<T>
    {
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public T Payload { get; set; }
        public string Message { get; set; }

        public ApiResult(int statusCode, bool isSuccess, T payload, string message = null)
        {
            StatusCode = statusCode;
            IsSuccess = isSuccess;
            Payload = payload;
            Message = message;
        }
    }
}
