using ServiceDefine;
using SimpleRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient
{
    public class TestClient : ClientBase, ITestService
    {
        public int Add(MathAddParam param)
        {
           var result = base.SendRequest(nameof(ITestService), nameof(Add), param, 500000);
            if (result.IsFailure)
            {
                throw new Exception(result.Message);
            }

            var value = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(result.Data);
            return value;
        }

        public async Task<int> AddAsync(MathAddParam param)
        {
            var result = await base.SendRequestAsync(nameof(ITestService), nameof(Add), param, 500000);
            if (result.IsFailure)
            {
                throw new Exception(result.Message);
            }

            var value = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(result.Data);
            return value;
        }

        public string? GetServiceName()
        {
            var result = base.SendRequest(nameof(ITestService), nameof(GetServiceName), null, 500000);
            if (result.IsFailure)
            {
                throw new Exception(result.Message);
            }

            var value = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(result.Data);
            return value;
        }

        public void DoNothing()
        {
            var result = base.SendRequest(nameof(ITestService), nameof(DoNothing), null, 500000);
            if (result.IsFailure)
            {
                throw new Exception(result.Message);
            }
        }
    }
}
