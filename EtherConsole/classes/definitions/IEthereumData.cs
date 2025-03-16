using System;
using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;

namespace EtherConsole.classes.definitions
{
    /// <summary>
    /// Interfaz que define operaciones para interactuar con la blockchain de Ethereum
    /// y almacenar/recuperar datos en ella.
    /// </summary>
    public interface IEthereumData
    {
        /// <summary>
        /// Envía una transacción que contiene un string en el campo "data" a la blockchain.
        /// </summary>
        /// <param name="data">El string a insertar en la blockchain.</param>
        /// <returns>El hash de la transacción enviada.</returns>
        Task<string> SendStringDataAsync(string data);

        /// <summary>
        /// Recupera el mensaje y la fecha de inserción de una transacción de la blockchain.
        /// </summary>
        /// <param name="txnHash">El hash de la transacción a consultar.</param>
        /// <returns>Una tupla con la transacción, la fecha de inserción y el mensaje.</returns>
        Task<(Transaction Transaction, DateTime Timestamp, string Message)> GetDataFromTransactionAsync(string txnHash);
    }
}
