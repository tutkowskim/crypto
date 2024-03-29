﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

// In order to debug on mac use https://github.com/azure/azurite

namespace functions
{
    public static class RequestNiceHashPayout
    {
        [FunctionName("RequestNiceHashPayout")]
        public static void Run([TimerTrigger("0 0 8 * * 1")] TimerInfo myTimer, ILogger log)
        {
            var threshold = 0.0005;
            var niceHashUrl = "https://api2.nicehash.com";
            var orgId = Environment.GetEnvironmentVariable("NICE_HASH_ORG_ID");
            var apiKey = Environment.GetEnvironmentVariable("NICE_HASH_API_KEY");
            var apiSecret = Environment.GetEnvironmentVariable("NICE_HASH_API_SECRET");
            NiceHashApi api = new NiceHashApi(niceHashUrl, orgId,apiKey, apiSecret, new HttpTracerLoggerToMicrosoftLoggerWrapper(log));

            Task task = new Task(async () => {
                var accountInfo = await api.GetAccountInfo();
                var amountAvailable = accountInfo.total.available;
                if (amountAvailable > threshold)
                {
                    var withdrawlAddresses = await api.GetWithdrawalAddresses();
                    if (withdrawlAddresses.list.Count >= 0)
                    {
                        var wallet = withdrawlAddresses.list[0];
                        await api.RequestWithdrawl(wallet.id, amountAvailable);
                        log.LogInformation("Requested a payout of " + amountAvailable + " to " + wallet.address);
                    }
                    else
                    {
                        log.LogInformation("No wallets have been white listed.");
                    }
                }
                else
                {
                    log.LogInformation("Account balance " + accountInfo.total.available + " doesn't meet the " + threshold + " thresold for payout.");
                }
            });

            task.Start();
            task.Wait();
        }
    }
}
