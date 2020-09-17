using System;
using System.Collections.Generic;
using System.Text;

namespace Fundamentals
{
    public enum ResultValue
    { 
        ErrorButcontinue,
        ErrorAndFatal,
        Success

    }

    public enum ErrorCode
    { 
        None = 0,
        RepositoryError=100,
        ServiceInactive = 1000,
        GeographyInactive =  1010,
        ServiceOrGeographyInactive = 1020,
        UserCreationFailed = 2000
    }
    public class Result
    {
        public ResultValue ResultValue { get; set; }
        public dynamic SomeGuy { get; set; }
        public ErrorCode Error { get; set; }
        public List<string> ResultMessages { get; set; }

        public Result(ResultValue ResultValue,ErrorCode error, string ResultMessage)
        {
            this.ResultValue = ResultValue;
            this.ResultMessages = new List<string>() { ResultMessage };
            this.Error = error;
        }

        public Result(ResultValue ResultValue, ErrorCode error, List<string> ResultMessages)
        {
            this.ResultValue = ResultValue;
            this.ResultMessages = ResultMessages;
            this.Error = error;
        }

    }
}
