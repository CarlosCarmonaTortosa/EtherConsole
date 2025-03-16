using System;
using System.Text;
using System.Threading.Tasks;
using EtherConsole.classes.definitions;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace EtherConsole.classes.implementations
{
    public class EthereumData : IEthereumData
    {
        private readonly string _nodeUrl;
        private readonly string _privateKey;
        private readonly string _accountAddress;

        public EthereumData(string nodeUrl, string privateKey)
        {
            _nodeUrl = nodeUrl;
            _privateKey = privateKey;

            //Crear una cuenta para obtener la dirección
            var account = new Account(privateKey);
            _accountAddress = account.Address;
        }

        /// <summary>
        /// Envía una transacción que contiene un string en el campo "data".
        /// La transacción se envía a la misma dirección para registrar la información.
        /// </summary>
        /// <param name="data">El string a insertar en la blockchain.</param>
        /// <returns>El hash de la transacción enviada.</returns>
        public async Task<string> SendStringDataAsync(string data)
        {
            //Crear la cuenta y establecer conexión con el nodo
            var account = new Account(_privateKey);
            var web3 = new Web3(account, _nodeUrl);

            //Convertir el string a hexadecimal (prefijado con "0x")
            var hexData = "0x" + Encoding.UTF8.GetBytes(data).ToHex();

            //Crear el objeto TransactionInput. Se envía a la misma dirección (self-tx)
            var txnInput = new TransactionInput
            {
                From = _accountAddress,
                To = _accountAddress,
                Data = hexData
            };

            //Estima el gas requerido
            var gasEstimate = await web3.Eth.Transactions.EstimateGas.SendRequestAsync(txnInput);

            //Anadir un margen del 20% al gas estimado (para evitar errores de Gas insuficiente)
            var adjustedGas = gasEstimate.Value + (gasEstimate.Value * 20 / 100);
            txnInput.Gas = new Nethereum.Hex.HexTypes.HexBigInteger(adjustedGas);
            Console.WriteLine($"Gas estimado: {gasEstimate.Value} | Con margen: {adjustedGas}");

            //Enviar la transacción a la red
            var txnHash = await web3.Eth.Transactions.SendTransaction.SendRequestAsync(txnInput);
            return txnHash;
        }

        /// <summary>
        /// Devuelve el mensaje y la fecha de inserción de una transacción en la blockchain.
        /// </summary>
        /// <param name="txnHash">El hash de la transacción a consultar.</param>
        /// <returns>Una tupla con la transacción, la fecha de inserción y el mensaje.</returns>
        public async Task<(Transaction Transaction, DateTime Timestamp, string Message)> GetDataFromTransactionAsync(string txnHash)
        {
            //Establecer conexión con el nodo
            var web3 = new Web3(_nodeUrl);

            //Obtener la transacción por su hash
            var transaction = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txnHash);

            if (transaction == null)
            {
                throw new Exception($"No se encontró la transacción con hash: {txnHash}");
            }

            //Extraer el payload hexadecimal (sin el prefijo 0x)
            string hexData = transaction.Input.Substring(2);

            //Convertir de hex a bytes y luego a string
            byte[] byteData = hexData.HexToByteArray();
            string decodedString = Encoding.UTF8.GetString(byteData);

            //Obtener información del bloque que contiene la transacción
            var block = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(
                new Nethereum.Hex.HexTypes.HexBigInteger(transaction.BlockNumber.Value));

            //Convertir el timestamp del bloque (Unix timestamp) a DateTime
            //El timestamp está en segundos desde el 1 de enero de 1970
            DateTime timestamp = DateTimeOffset.FromUnixTimeSeconds((long)block.Timestamp.Value).DateTime.ToLocalTime();

            return (transaction, timestamp, decodedString);
        }
    }
}
