using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Domain.Common.Results
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        protected Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None)
                throw new InvalidOperationException();

            if (!isSuccess && error == Error.None)
                throw new InvalidOperationException();

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success()
            => new(true, Error.None);

        public static Result Fail(Error error)
            => new(false, error);
    }
}
