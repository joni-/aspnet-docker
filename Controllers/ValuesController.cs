using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Net;

namespace DotnetDocker.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IConnectionMultiplexer _connection;

        public ValuesController(IConnectionMultiplexer connection)
        {
            this._connection = connection;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var db = this._connection.GetDatabase();

            var allKeys = new List<RedisKey>();

            foreach (EndPoint ep in this._connection.GetEndPoints())
            {
                var server = this._connection.GetServer(ep);
                var keys = server.Keys(db.Database, "*");
                foreach (RedisKey k in keys)
                {
                    allKeys.Add(k.ToString());
                }
            }

            var entries = new List<string>();

            foreach (RedisKey key in allKeys)
            {
                entries.Add(String.Format("{0} => {1}", key, db.StringGet(key)));
            }

            return entries;
        }

        // GET api/values/5
        [HttpGet("{val}")]
        public void Get(string val)
        {
            var guid = Guid.NewGuid().ToString();
            var db = this._connection.GetDatabase();
            db.StringSet(guid, val);
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
