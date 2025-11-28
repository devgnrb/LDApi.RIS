using System.Net.Sockets;
using System.Text;
using LDApi.RIS.Interfaces;

namespace LDApi.RIS.Services
{
    public class MllpClientService : IMllpClientService
    {

        private readonly string _host;
        private readonly int _port;
        public MllpClientService(ConfigurationService configService)
        {
                var cfg = configService.Config.Mllp;
                _host = cfg.Host;
                _port = cfg.Port;
        }

        public async Task<string> SendMessageAsync(string hl7Message)
        {
            const byte VT = 0x0B; 
            const byte FS = 0x1C;
            const byte CR = 0x0D;


            try
            {
                using var client = new TcpClient();
               
                await client.ConnectAsync(_host, _port);

                if (!client.Connected)
                    throw new Exception($"Impossible de se connecter à {_host}:{_port}");

                using var stream = client.GetStream();

                var messageBytes = Encoding.ASCII.GetBytes(hl7Message);
                using var ms = new MemoryStream();
                ms.WriteByte(VT);
                ms.Write(messageBytes, 0, messageBytes.Length);
                ms.WriteByte(FS);
                ms.WriteByte(CR);

                await stream.WriteAsync(ms.ToArray());
                await stream.FlushAsync();

                // Lire ACK
                var buffer = new byte[4096];
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);

                var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                return response.Trim((char)VT, (char)FS, (char)CR);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur MLLP vers {_host}:{_port} : {ex.Message}", ex);
            }
        }
    }

}
