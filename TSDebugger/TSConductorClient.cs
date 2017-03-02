using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

namespace TSDebugger
{
    class TSConductorClient
    {
        public class LocoData
        {
            public LocoData(String provider, String product, String enginename)
            {
                Provider = provider;
                Product = product;
                EngineName = enginename;
            }
            
            public String Provider;
            public String Product;
            public String EngineName;
        }

        public class TSVariable
        {
            public TSVariable(String name, int id, float min, float max, float cur)
            {
                Name = name;
                ID = id;
                Minimum = min;
                Maximum = max;
                Current = cur;
            }

            private String _name;
            private int _id;
            private float _minimum;
            private float _maximum;
            private float _current;

            public String Name { get { return _name; } set { _name = value; } }
            public int ID { get { return _id; } set { _id = value; } }
            public float Minimum { get { return _minimum; } set { _minimum = value; } }
            public float Maximum { get { return _maximum; } set { _maximum = value; } }
            public float Current { get { return _current; } set { _current = value; } }
        }

        public bool Connnected = false;
        public bool Polling = false;

        SemaphoreSlim pollingSemaphore = new SemaphoreSlim(1);

        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;

        private static String[] COLON_DELIM = {".:."};
        private static String[] SEMI_DELIM = { ";" };
        private static CultureInfo TSC_CULTURE = new CultureInfo("de-DE");

        public async Task ConnectAsync(String hostname, int port, String clientName)
        {
            IPAddress[] address = await Dns.GetHostAddressesAsync(hostname).ConfigureAwait(false);
            client = new TcpClient();
            await client.ConnectAsync(address, port);
            Connnected = true;
            reader = new StreamReader(client.GetStream());
            writer = new StreamWriter(client.GetStream());
            writer.AutoFlush = true;

            await writer.WriteAsync("EnableMessageLengthHeader()<END>");
            await Task.Delay(500);
            String setClientCall = "SetClientName(" + clientName + ")<END>";
            await writer.WriteAsync(setClientCall);
            await Task.Delay(500);
        }

        public void Disconnect()
        {
            client.Close();
            Connnected = false;
        }

        public async Task<LocoData> GetLocoNameAsync()
        {
            String result = await CallFunction("GetLocoName()");
            String[] resultSplit = result.Split(COLON_DELIM, StringSplitOptions.None);
            LocoData loco = new LocoData(resultSplit[0], resultSplit[1], resultSplit[2]);
            return loco;
        }

        public async Task SetControllerValueAsync(int id, float value)
        {
            String funcCall = "SetControllerValue(" + id.ToString() + ";" + value.ToString(TSC_CULTURE) + ")<END>";
            await writer.WriteAsync(funcCall);
        }

        public async Task StartPollingAsync(int interval, IProgress<List<TSVariable>> updater)
        {
            if (Polling)
                return;

            Polling = true;

            String funcCall = "StartPolling(" + interval.ToString() + ";FLAT)<END>";
            String setupMsg = await CallFunction(funcCall);
            String[] setupSplit = setupMsg.Split(SEMI_DELIM, StringSplitOptions.RemoveEmptyEntries);

            SortedDictionary<int, TSVariable> vars = new SortedDictionary<int, TSVariable>();

            vars.Add(400, new TSVariable("Latitude of train", 400, -90.0f, 90.0f, 0.0f));
            vars.Add(401, new TSVariable("Longitude of train", 401, -180.0f, 180.0f, 0.0f));
            vars.Add(402, new TSVariable("Fuel", 402, 0.0f, 0.0f, 0.0f));
            vars.Add(403, new TSVariable("Is in tunnel", 403, 0.0f, 1.0f, 0.0f));
            vars.Add(404, new TSVariable("Gradient", 404, -1.0f, 1.0f, 0.0f));
            vars.Add(405, new TSVariable("Heading", 405, 0.0f, 0.0f, 0.0f));
            vars.Add(406, new TSVariable("Time - Hours", 406, 0.0f, 23.0f, 0.0f));
            vars.Add(407, new TSVariable("Time - Minutes", 407, 0.0f, 59.0f, 0.0f));
            vars.Add(408, new TSVariable("Time - Seconds", 408, 0.0f, 59.0f, 0.0f));

            await writer.WriteAsync("EnableExtendedPolling()<END>"); 

            for (int i = 0; i < setupSplit.Length - 3; i += 4)
            { 
                TSVariable var = new TSVariable(setupSplit[i + 1], Int32.Parse(setupSplit[i], TSC_CULTURE), float.Parse(setupSplit[i + 2], TSC_CULTURE), float.Parse(setupSplit[i + 3], TSC_CULTURE), 0.0f);
                vars.Add(var.ID, var);
            }

            while (client.Connected)
            {
                String msg = await ReceiveMessage();

                if(msg.EndsWith("<MSG>"))
                {
                    break;
                }

                String[] msgSplit = msg.Split(SEMI_DELIM, StringSplitOptions.RemoveEmptyEntries);

                for(int i = 0; i < msgSplit.Length - 2; i += 2)
                {
                    vars[Int32.Parse(msgSplit[i])].Current = float.Parse(msgSplit[i + 1], TSC_CULTURE);
                }

                List<TSVariable> varList = new List<TSVariable>(vars.Values);
                updater.Report(varList);

            }

            pollingSemaphore.Release();
            Polling = false;
        }

        public async Task StopPollingAsync()
        {
            if (!Polling)
                return;
            await writer.WriteAsync("StopPolling()<END>");
            pollingSemaphore = new SemaphoreSlim(0, 1);
            await pollingSemaphore.WaitAsync();
        }

        private async Task<String> CallFunction(String funcCall)
        {
            String funcCallFull = funcCall + "<END>";
            await writer.WriteAsync(funcCallFull);

            String result = await ReceiveMessage();

            if (result.EndsWith("<ERR>"))
            {
                String[] errMsg = result.Split(SEMI_DELIM,StringSplitOptions.None);
                throw new ArgumentException("TSConductor: " + errMsg[1]);
            }

            return result;
            
        }

        private async Task<String> ReceiveMessage()
        {
            char[] buffer = new char[32];

            //Read # and first character of length
            await reader.ReadBlockAsync(buffer, 0, 2);

            //Keep reading 1 character a time until the end of the message length header is found
            int index;
            for (index = 2; buffer[index - 1] != ';'; ++index)
            {
                await reader.ReadBlockAsync(buffer, index, 1);
            }

            int messageLength = Int32.Parse(new String(buffer, 1, index - 2));
            buffer = new char[messageLength];

            //Read the message
            await reader.ReadBlockAsync(buffer, 0, messageLength);

            return new String(buffer);
        }

    }
}
