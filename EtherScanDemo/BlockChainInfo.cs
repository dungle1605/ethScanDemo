using EtherScanDemo.Helper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Globalization;


namespace EtherScanDemo
{
    public class BlockChainInfo
    {
        private HttpClient _client;
        private readonly string _commonAPI = "https://api.etherscan.io/api";
        private readonly string _apiKey;
        private DbIntegration _dbIntegration;
        private readonly string _logsPath;
        private Stopwatch _sw;

        public BlockChainInfo()
        {
            _client = new HttpClient();
            _apiKey = Environment.GetEnvironmentVariable("APIKey");
            _dbIntegration = new DbIntegration();
            _logsPath = Environment.GetEnvironmentVariable("LogsPath");
            _sw = new Stopwatch();
        }

        public async Task ScanBlockHandling(int minRange, int maxRange)
        {
            while (minRange <= maxRange)
            {
                _sw.Restart();
                var runningBlock = String.Format("0x{0:X}", minRange);

                Console.WriteLine($"==== Get Block By Number ({runningBlock}) ====");
                File.AppendAllText(_logsPath, $"==== Get Block By Number ({runningBlock}) ====");

                var blockByNumber = await GetBlockByNumbers(runningBlock);

                var processingTime = _sw.Elapsed.ToString(@"m\:ss\.fff");
                Console.WriteLine($"Processing time: {processingTime}");
                File.AppendAllText(_logsPath, $"Processing time: {processingTime}");

                Console.WriteLine(blockByNumber.ToString());
                File.AppendAllText(_logsPath, blockByNumber.ToString());

                if (blockByNumber["error"] is null)
                {
                    Console.WriteLine($"\nBegin inserting Blocks - Block No: ({runningBlock})");
                    File.AppendAllText(_logsPath, $"\nBegin inserting Blocks - Block No: ({runningBlock})");

                    _sw.Restart();
                    var resultInsertBlock = await InsertBlock(blockByNumber["result"], runningBlock);

                    processingTime = _sw.Elapsed.ToString(@"m\:ss\.fff");
                    Console.WriteLine($"Processing time: {processingTime}");
                    File.AppendAllText(_logsPath, $"Processing time: {processingTime}");

                    Console.WriteLine($"Status: {resultInsertBlock}");
                    File.AppendAllText(_logsPath, $"Status: {resultInsertBlock}");

                    Console.WriteLine($"\n\n ==== GetBlockTransactionCountByNumber ({runningBlock}) ====");
                    File.AppendAllText(_logsPath, $"\n\n ==== GetBlockTransactionCountByNumber ({runningBlock}) ====");

                    _sw.Restart();
                    var totalBlockByNum = await GetBlockTransactionCountByNumber(runningBlock);

                    processingTime = _sw.Elapsed.ToString(@"m\:ss\.fff");
                    Console.WriteLine($"Processing time: {processingTime}");
                    File.AppendAllText(_logsPath, $"Processing time: {processingTime}");

                    Console.WriteLine($"Total Record(s): ${totalBlockByNum}" );
                    File.AppendAllText(_logsPath, $"Total Record(s): ${totalBlockByNum}");

                    if (totalBlockByNum > 0)
                    {
                        for (int i = 0; i < totalBlockByNum; i++)
                        {
                            Console.WriteLine($"\n\n ==== GetTransactionByBlockNumberAndIndex ==== (i: {i}, Block No {runningBlock})");
                            File.AppendAllText(_logsPath, $"\n\n ==== GetTransactionByBlockNumberAndIndex ==== (i: {i}, Block No {runningBlock})");

                            _sw.Restart();
                            var transactionInfo = await GetTransactionByBlockNumberAndIndex(i, runningBlock);

                            processingTime = _sw.Elapsed.ToString(@"m\:ss\.fff");
                            Console.WriteLine($"Processing time: {processingTime}");
                            File.AppendAllText(_logsPath, $"Processing time: {processingTime}");

                            Console.WriteLine(transactionInfo.ToString());
                            File.AppendAllText(_logsPath, transactionInfo.ToString());

                            Console.WriteLine($"\nBegin Inserting Transaction - (i: {i}, Block No {runningBlock})");
                            File.AppendAllText(_logsPath, $"\nBegin Inserting Transaction - (i: {i}, Block No {runningBlock})");

                            _sw.Restart();
                            var resultInsertTransaction = await InsertTransaction(transactionInfo["result"], runningBlock);

                            processingTime = _sw.Elapsed.ToString(@"m\:ss\.fff");
                            Console.WriteLine($"Processing time: {processingTime}");
                            File.AppendAllText(_logsPath, $"Processing time: {processingTime}");

                            Console.WriteLine($"Status: {resultInsertTransaction}");
                            File.AppendAllText(_logsPath, $"Status: {resultInsertTransaction}");

                            Console.WriteLine("\n\n");
                            File.AppendAllText(_logsPath, "\n\n");
                        }
                    }
                }
                minRange++;
            }
        }

        private async Task<JObject> GetBlockByNumbers(string blockHexNo)
        {
            var resultApi = await _client.GetAsync($"{_commonAPI}?module=proxy&action=eth_getBlockByNumber&tag={blockHexNo}&boolean=true&apikey={_apiKey}");
            var response = await APIIntegrationHelper.GetResponseContent(resultApi);

            return response;
        }

        private async Task<int> GetBlockTransactionCountByNumber(string blockHexNo)
        {
            var resultApi = await _client.GetAsync($"{_commonAPI}?module=proxy&action=eth_getBlockTransactionCountByNumber&tag={blockHexNo}&boolean=true&apikey={_apiKey}");
            var response = await APIIntegrationHelper.GetResponseContent(resultApi);

            return FromHexToInt(response["result"].ToString());
        }

        private async Task<JObject> GetTransactionByBlockNumberAndIndex(int index, string blockHexNo)
        {
            var indexHex = String.Format("0x{0:X}", index);

            var resultApi = await _client.GetAsync($"{_commonAPI}?module=proxy&action=eth_getTransactionByBlockNumberAndIndex&tag={blockHexNo}&index={indexHex}&apikey={_apiKey}");
            var response = await APIIntegrationHelper.GetResponseContent(resultApi);

            return response;
        }

        private async Task<bool> InsertBlock(JToken param, string blockNumber)
        {
            if (param == null)
            {
                return false;
            }

            try
            {
                var lstParam = new MySqlParameter[]
                {
                new MySqlParameter("in_blockNumber", FromHexToInt(blockNumber)),
                new MySqlParameter("in_hash", param["hash"] != null ? param["hash"].ToString() : string.Empty),
                new MySqlParameter("in_parentHash", param["parentHash"] != null ? param["parentHash"].ToString() : string.Empty),
                new MySqlParameter("in_miner", param["miner"] != null ? param["miner"]?.ToString() : string.Empty),
                new MySqlParameter("in_blockReward", param["blockReward"] != null ? FromHexToDecimal(param["blockReward"].ToString()) : -1),
                new MySqlParameter("in_gasLimit", param["gasLimit"] != null ? FromHexToDecimal(param["gasLimit"].ToString()) : -1),
                new MySqlParameter("in_gasUsed", param["gasUsed"] != null ? FromHexToDecimal(param["gasUsed"].ToString()) : -1)
                };
                return _dbIntegration.ExecuteCommand("SPP_INSERT_BLOCK", lstParam);
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        private async Task<bool> InsertTransaction(JToken param, string blockNumber)
        {
            if (param == null)
            {
                return false;
            }

            try
            {
                var lstParam = new MySqlParameter[]
                {
                new MySqlParameter("in_blockNumber", FromHexToInt(blockNumber)),
                new MySqlParameter("in_hash", param["hash"] != null ? param["hash"].ToString() : string.Empty),
                new MySqlParameter("in_from", param["from"] != null ? param["from"].ToString() : string.Empty),
                new MySqlParameter("in_to", param["to"] != null ? param["to"].ToString() : string.Empty),
                new MySqlParameter("in_value", param["value"] != null ? FromHexToDecimal(param["value"].ToString()) : -1),
                new MySqlParameter("in_gas", param["gas"] != null ? FromHexToDecimal(param["gas"].ToString()) : -1),
                new MySqlParameter("in_gasPrice", param["gasPrice"] != null ? FromHexToDecimal(param["gasPrice"].ToString()) : -1),
                new MySqlParameter("in_transactionIndex", param["transactionIndex"] != null ? FromHexToDecimal(param["transactionIndex"].ToString()) : -1)
                };
                return _dbIntegration.ExecuteCommand("SPP_INSERT_TRANSACTION", lstParam);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private int FromHexToInt(string value)
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                value = value.ToUpper().Substring(2);
            }

            return Int32.Parse(value, NumberStyles.HexNumber);
        }

        private decimal FromHexToDecimal(string value)
        {
            return Convert.ToInt64(value, 16);
        }

    }
}
