namespace SimpleRPC
{
    public interface IResult
    {
        int Code { get; }
        string Message { get; }
        bool IsFailure { get; }
    }

    public interface IResult<T> : IResult
    {
        T Data { get; }
    }


    public class Result : IResult
    {
        public int Code { get; }

        public string Message { get; }

        public bool IsFailure { get { return Code != 0; } }

        public Result(int code, string message)
        {
            Code = code;
            Message = message;
        }


        // 快捷方法
        public static Result Success()
        {
            return new Result(0, null);
        }

        public static Result Fail(int code, string message)
        {
            return new Result(code, message);
        }

        public static Result Fail(string message)
        {
            return new Result(-1, message);
        }

        public static Result<T> Success<T>(T data)
        {
            return new Result<T>(0, data, null);
        }

        public static Result<T> Fail<T>(int code, string message)
        {
            return new Result<T>(code, default(T), message);
        }

        public static Result<T> Fail<T>(string message)
        {
            return new Result<T>(-1, default(T), message);
        }
        /// <summary>
        /// 创建失败Result＜T＞ 可以不写 ＜T＞
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static FailResult ToFailResult(int code, string message)
        {
            return new FailResult(code, message);
        }

        /// <summary>
        /// 创建失败Result＜T＞ 可以不写 ＜T＞
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static FailResult ToFailResult(string message)
        {
            return new FailResult(-1, message);
        }
    }

    public class Result<T> : Result, IResult<T>
    {
        public virtual T Data { get; }

        public Result(int code, T data, string message)
            : base(code, message)
        {
            Data = data;
        }



        // 支持 FailResult 隐式转换
        public static implicit operator Result<T>(FailResult failResult)
        {
            return new Result<T>(failResult.Code, default(T), failResult.Message);
        }
    }

    public class FailResult : Result
    {
        public FailResult(int code, string message) : base(code, message)
        {
        }
    }
}
