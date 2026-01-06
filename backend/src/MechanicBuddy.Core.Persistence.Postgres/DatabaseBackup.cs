using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Carmasters.Core.Application;
using Carmasters.Core.Application.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Carmasters.Core.Persistence.Postgres
{

   
    public class DatabaseBackup
    {
        private readonly ILogger<DatabaseBackup> logger;
        private readonly DbOptions options;
        private readonly string dumpCommand;
        private readonly string dumpProgram;
        public DatabaseBackup(ILogger<DatabaseBackup> logger,IConfiguration configuration)
        {
            options = new DbOptions(); configuration.GetSection("DbOptions").Bind(options);
            dumpCommand =configuration.GetSection("DatabaseBackup:DumpCommand").Value;
            dumpProgram = configuration.GetSection("DatabaseBackup:Program").Value;
            this.logger = logger;
        }

        
        public async Task<string> Dump()
        {
            if (string.IsNullOrWhiteSpace(dumpCommand)) throw new Exception("DumpCommand missing.");
             
            var commandText = string.Format(dumpCommand, options.Host,options.Password,options.UserId,options.Name);
            var program = dumpProgram;

            return await new ShellCommand().Run(program, commandText); 
        } 

    }
}
