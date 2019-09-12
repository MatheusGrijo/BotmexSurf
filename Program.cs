using BitBotBackToTheFuture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

enum TendencyMarket
{
    HIGH,
    NORMAL,
    LOW,
    VERY_LOW,
    VERY_HIGH
}

class MainClass
{


    //REAL NET
    public static string version = "0.0.2.14";
    public static string location = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + System.IO.Path.DirectorySeparatorChar;

    public static string bitmexKey = "";
    public static string bitmexSecret = "";
    public static string bitmexKeyWeb = "";
    public static string bitmexSecretWeb = "";
    public static string pair = "";
    public static int qtdyContacts = 0;
    public static int interval = 0;
    public static int intervalOrder = 0;
    public static int intervalCapture = 0;
    public static int intervalCancelOrder = 30;
    public static int positionContracts = 0;
    public static double profit = 0;
    public static string bitmexDomain = "";    


    public static BitMEX.BitMEXApi bitMEXApi = null;





    public static Object data = new Object();
    

    public static void Main(string[] args)
    {
        try
        {
            Console.Title = "Loading...";

            Console.ForegroundColor = ConsoleColor.White;

            log("Deleron - Back to the future - v" + version + " - Bitmex version");
            log("by Matheus Grijo ", ConsoleColor.Green);

            log("GITHUB http://github.com/matheusgrijo", ConsoleColor.Blue);
            log("Load config...");

            String jsonConfig = System.IO.File.ReadAllText(location + "key.txt");
            JContainer jCointaner = (JContainer)JsonConvert.DeserializeObject(jsonConfig, (typeof(JContainer)));
            

            bitmexKey = jCointaner["key"].ToString();
            bitmexSecret = jCointaner["secret"].ToString();
            bitmexKeyWeb = jCointaner["webserverKey"].ToString();
            bitmexSecretWeb = jCointaner["webserverSecret"].ToString();
            bitmexDomain = jCointaner["domain"].ToString();
            pair = jCointaner["pair"].ToString();            
            qtdyContacts = int.Parse(jCointaner["contract"].ToString());
            interval = int.Parse(jCointaner["interval"].ToString());
            intervalCapture = int.Parse(jCointaner["webserverIntervalCapture"].ToString());
                                                
            

            bitMEXApi = new BitMEX.BitMEXApi(bitmexKey, bitmexSecret, bitmexDomain);



            if (jCointaner["webserver"].ToString() == "enable")
            {
                WebServer ws = new WebServer(WebServer.SendResponse, jCointaner["webserverConfig"].ToString());
                ws.Run();
                System.Threading.Thread tCapture = new Thread(Database.captureDataJob);
                tCapture.Start();
                System.Threading.Thread.Sleep(1000);
                OperatingSystem os = Environment.OSVersion;
                PlatformID pid = os.Platform;
                if (pid != PlatformID.Unix)
                {
                    System.Diagnostics.Process.Start(jCointaner["webserverConfig"].ToString());
                }
            }



            log("Total open orders: " + bitMEXApi.GetOpenOrders(pair).Count);

            log("");
            log("Wallet: " + bitMEXApi.GetWallet());

          

            string codeTendency = "";

            //LOOP 
            while (true)
            {

                try
                {



                    //Surf mode


                    

                    String json = Http.get("https://anubis.website/api/anubis/trend/");
                    JContainer jTrrend = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));

                    int open = getPosition();                    

                    //OPEN LONG
                    if (jTrrend["data"][1]["trend"].ToString() == "SHORT" && jTrrend["data"][0]["trend"].ToString() == "LONG" && jTrrend["data"][0]["timestamp"].ToString() != codeTendency)
                    {                        
                        if (open < 0)
                            open = Math.Abs(open);

                        codeTendency = jTrrend["data"][0]["timestamp"].ToString();
                        bitMEXApi.MarketOrder(pair, "Buy", qtdyContacts + open);

                    }

                    //OPEN SHORT
                    if (jTrrend["data"][1]["trend"].ToString() == "LONG" && jTrrend["data"][0]["trend"].ToString() == "SHORT" && jTrrend["data"][0]["timestamp"].ToString() != codeTendency)
                    {                        
                        codeTendency = jTrrend["data"][0]["timestamp"].ToString();
                        bitMEXApi.MarketOrder(pair, "Sell", qtdyContacts + open);
                    }

                    log("wait " + interval + "ms", ConsoleColor.White);
                    Thread.Sleep(interval);


                }
                catch (Exception ex)
                {
                    log("while true::" + ex.Message + ex.StackTrace);
                }


            }

        }
        catch (Exception ex)
        {
            log("ERROR FATAL::" + ex.Message + ex.StackTrace);
        }
    }


    

    //By Lucas Sousa modify MatheusGrijo
    static int getPosition()
    {
        try
        {
            log("getPosition...");
            List<BitMEX.Position> OpenPositions = bitMEXApi.GetOpenPositions(pair);
            int _qtdContacts = 0;
            foreach (var Position in OpenPositions)
                _qtdContacts += (int)Position.CurrentQty;
            log("getPosition: " + _qtdContacts);
            return _qtdContacts;
        }
        catch (Exception ex)
        {
            log("getPosition::" + ex.Message + ex.StackTrace);
            throw new Exception("Error getPosition");
        }
    }
    

    static string getValue(String nameList, String nameIndicator, String nameParameter)
    {
        String jsonConfig = System.IO.File.ReadAllText(location + "key.txt");
        JContainer jCointaner = (JContainer)JsonConvert.DeserializeObject(jsonConfig, (typeof(JContainer)));
        foreach (var item in jCointaner[nameList])
            if (item["name"].ToString().Trim().ToUpper() == nameIndicator.ToUpper().Trim())
                return item[nameParameter].ToString().Trim();
        return null;
    }

    public static void log(string value, ConsoleColor color = ConsoleColor.White)
    {
        try
        {

            value = "[" + DateTime.Now.ToString() + "] - " + value;
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ForegroundColor = ConsoleColor.White;

            System.IO.StreamWriter w = new StreamWriter(location + DateTime.Now.ToString("yyyyMMdd") + "_log.txt", true);
            w.WriteLine(value);
            w.Close();
            w.Dispose();

        }
        catch { }
    }

    


}





