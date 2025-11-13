using LDApi.RIS.Interfaces;
using System.Net.Sockets;
using System.Text;

public class MllpClientService : IMllpClientService
{
    private readonly string _host;
    private readonly int _port;

    public MllpClientService(string host, int port)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _port = port;
    }

    public async Task<string> SendMessageAsync(string hl7Message)
    {
        const byte VT = 0x0B; // Start block
        const byte FS = 0x1C; // End block
        const byte CR = 0x0D; // Carriage return

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_host, _port);

            if (!client.Connected)
                throw new Exception($"Impossible de se connecter à {_host}:{_port}");

            using var stream = client.GetStream();

            // Encapsulation MLLP
            var messageBytes = Encoding.ASCII.GetBytes(hl7Message);
            using var ms = new MemoryStream();
            ms.WriteByte(VT);
            ms.Write(messageBytes, 0, messageBytes.Length);
            ms.WriteByte(FS);
            ms.WriteByte(CR);
            var toSend = ms.ToArray();

            //  Envoi du message HL7
            await stream.WriteAsync(toSend, 0, toSend.Length);
            await stream.FlushAsync();

            //  Lecture de la réponse (ACK)
            var buffer = new byte[4096];
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            int bytesRead = 0;
            try
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("Aucun ACK reçu de HL7 Soup (timeout).");
            }

            // Convertir et nettoyer la réponse
            var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            response = response.Trim((char)VT, (char)FS, (char)CR);

            // Log utile pour debug
            Console.WriteLine("Réponse HL7 Soup (ACK brut) : " + response);

            return response;
        }
        catch (SocketException se)
        {
            throw new Exception($"Erreur socket : {se.Message}", se);
        }
        catch (IOException ioe)
        {
            throw new Exception($"Erreur I/O lors de la communication MLLP : {ioe.Message}", ioe);
        }
    }
}
