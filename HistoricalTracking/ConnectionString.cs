using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoricalTracking
{
    public static class PtDbConnection
    {
        public static readonly string ConnectionString = "mongodb://localhost:27017?socketTimeoutMS=19000";
        //public static readonly string ConnectionString = "mongodb://Runeweaver:Fourcats4@ac-oqlurky-shard-00-00.ufwkgz2.mongodb.net:27017,ac-oqlurky-shard-00-01.ufwkgz2.mongodb.net:27017,ac-oqlurky-shard-00-02.ufwkgz2.mongodb.net:27017/?ssl=true&replicaSet=atlas-133h6i-shard-0&authSource=admin&retryWrites=true&w=majority";
    }
}
