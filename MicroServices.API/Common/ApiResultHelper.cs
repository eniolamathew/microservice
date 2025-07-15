using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServices.API.Common
{
    public static class ApiResultHelper
    {
        public static ApiResult<T> SuccessResult<T>(T data, string message = null, int statusCode = 200)
        {
            return new ApiResult<T>(statusCode, true, data, message);
        }

        public static ApiResult<T> ErrorResult<T>(string message, int statusCode = 400)
        {
            return new ApiResult<T>(statusCode, false, default, message);
        }
    }

}
