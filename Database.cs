using BitBotBackToTheFuture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Database
{
    
    //private static string dataBaseFile = MainClass.location + "bd.xml";
    public static void captureDataJob()
    {
        while (true)
        {
            try
            {
                captureData();
            }
            catch
            {

            }
            System.Threading.Thread.Sleep(MainClass.intervalCapture);
        }
    }

    public static void captureData()
    {
        lock (MainClass.data)
        {
            string dataBaseFile = MainClass.location + "bd.xml";
            try
            {
                System.Data.DataSet ds = null;
                bool create = false;
                if (!System.IO.File.Exists(dataBaseFile))
                {
                    System.Data.DataTable dt = new System.Data.DataTable("Balances");
                    dt.Columns.Add("Date");
                    dt.Columns.Add("Coin");
                    dt.Columns.Add("Amount");

                    dt.Rows.Add("", "", "");

                    System.Data.DataTable dtParameters = new System.Data.DataTable("Parameters");
                    dtParameters.Columns.Add("Parameter");
                    dtParameters.Columns.Add("Value");
                    dtParameters.Rows.Add("", "");


                    ds = new System.Data.DataSet();
                    ds.DataSetName = "Database";
                    ds.Tables.Add(dt);
                    ds.Tables.Add(dtParameters);
                    ds.WriteXml(dataBaseFile);
                    create = true;
                }

                ds = new System.Data.DataSet();
                ds.ReadXml(dataBaseFile);

                BitMEX.BitMEXApi bitMEXApi = new BitMEX.BitMEXApi(MainClass.bitmexKey, MainClass.bitmexSecret, MainClass.bitmexDomain);
                string json = bitMEXApi.GetWallet();
                JContainer jCointaner = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));

                
                //ClassDB.execS(ClassDB.dbquery.Replace("@balance", jCointaner[0]["walletBalance"].ToString().Replace(",", ".")));

                if (create)
                    ds.Tables[0].Rows.Clear();

                ds.Tables[0].Rows.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), MainClass.pair, jCointaner[0]["walletBalance"].ToString());

                ds.Tables[1].Rows.Clear();
                ds.Tables[1].Rows.Add("OpenOrders", bitMEXApi.GetOpenOrders(MainClass.pair).Count);
                ds.Tables[1].Rows.Add("Amount", jCointaner[0]["walletBalance"].ToString());


                System.IO.File.Delete(dataBaseFile);
                ds.WriteXml(dataBaseFile);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
