using EtherConsole.classes.definitions;
using EtherConsole.classes.implementations;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static async Task Main(string[] args)
    {
        //----------------------------------------------
        //Comando Docker para crear Nodo de Prueba
        //PowerShell: docker run -p 8545:8545 trufflesuite/ganache-cli ganache-cli -h 0.0.0.0
        //Al generarlo se obtiene la clave privada de la cuenta de prueba (10 para usar con 100 ETH)
        //Para persistir los datos (sustituir C:/EtherDB por el path donde se quiera persistir):
        //PowerShell: docker run --name NodoEtherDev -v C:/EtherDB:/data -p 8545:8545 trufflesuite/ganache-cli ganache-cli -h 0.0.0.0 --db /data
        //Para volver a arrancar el nodo:
        //PowerShell: docker start NodoEtherDev
        //----------------------------------------------

        //Configurar servicios con inyección de dependencias
        var serviceProvider = new ServiceCollection()
            .AddTransient<IEthereumData>(provider =>
            {
                //Configura la URL de tu nodo local y tu clave privada.
                //Asegúrate de que el nodo está corriendo y la API RPC esté habilitada.
                string nodeUrl = "http://localhost:8545";
                string privateKey = "ESTABLECE TU CLAVE PRIVADA";
                return new EthereumData(nodeUrl, privateKey);
            }).BuildServiceProvider();

        var ethManager = serviceProvider.GetRequiredService<IEthereumData>();
        string message = "Mensaje a insertar en la blockchain de Ethereum";

        try
        {
            //Enviar (escribir) el mensaje a la blockchain y recuperar el hash de la transacción
            string txnHash = await ethManager.SendStringDataAsync(message);
            Console.WriteLine($"Transacción enviada. Hash: {txnHash}");

            //Esperar un momento para que la transacción sea minada (solo en redes rápidas como Ganache)
            //Si estás en una red real, este paso no es necesario
            //Con un nodo real, la transacción puede tardar varios segundos o minutos en ser minada.
            await Task.Delay(1000);

            //Recuperar (leer) y mostrar los datos de la transacción
            var recoveredData = await ethManager.GetDataFromTransactionAsync(txnHash);
            Console.WriteLine("Información de la transacción:");
            Console.WriteLine($"Fecha de inserción: {recoveredData.Timestamp.ToString("dd/MM/yyyy HH:mm:ss")}");
            Console.WriteLine($"De: {recoveredData.Transaction.From}");
            Console.WriteLine($"Para: {recoveredData.Transaction.To}");
            Console.WriteLine($"Hash: {recoveredData.Transaction.TransactionHash}");
            Console.WriteLine($"Bloque: {recoveredData.Transaction.BlockNumber?.Value.ToString() ?? "Pendiente"}");
            Console.WriteLine($"Gas utilizado: {recoveredData.Transaction.Gas.Value}");
            Console.WriteLine($"Mensaje: {recoveredData.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
