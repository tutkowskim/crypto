﻿using Microsoft.Extensions.Logging;

namespace miner_functions
{
    public class HttpTracerLoggerToMicrosoftLoggerWrapper : HttpTracer.Logger.ILogger
    {
        private readonly Microsoft.Extensions.Logging.ILogger logger;

        public HttpTracerLoggerToMicrosoftLoggerWrapper(Microsoft.Extensions.Logging.ILogger logger)
        {
            this.logger = logger;
        }

        public void Log(string message)
        {
            this.logger.LogDebug(message);
        }
    }
}
