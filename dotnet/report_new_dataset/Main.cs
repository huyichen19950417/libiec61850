using System;
using System.Collections.Generic;

using IEC61850.Common;
using IEC61850.Client;
using System.Threading;

namespace report_new_dataset
{
    class ReportNewDataSetExample
    {
		private static void reportHandler (Report report, object parameter)
		{
			Console.WriteLine ("Received report:\n----------------");

			if (report.HasTimestamp ())
				Console.WriteLine ("  timestamp: " + MmsValue.MsTimeToDateTimeOffset (report.GetTimestamp ()).ToString ());

			MmsValue values = report.GetDataSetValues ();

			Console.WriteLine ("  report dataset contains " + values.Size () + " elements");

			for (int i = 0; i < values.Size(); i++) {
				if (report.GetReasonForInclusion(i) != ReasonForInclusion.REASON_NOT_INCLUDED) {
					Console.WriteLine("    element " + i + " included for reason " + report.GetReasonForInclusion(i).ToString() + " " + values.GetElement(i));
				}
			}
		}

		private static bool running = true;

        public static void Main (string[] args)
        {
            IedConnection con = new IedConnection ();

            string hostname;

            if (args.Length > 0)
                hostname = args[0];
            else
                hostname = "10.0.2.2";
                //hostname = "localhost";

            Console.WriteLine("Connect to " + hostname);

            try
            {
                con.Connect(hostname, 102);

                List<string> serverDirectory = con.GetServerDirectory(false);

                foreach (string entry in serverDirectory)
                {
                    Console.WriteLine("LD: " + entry);
                }

				// create a new data set

				List<string> dataSetElements = new List<string>();

				dataSetElements.Add("simpleIOGenericIO/GGIO1.AnIn1.mag.f[MX]");
				dataSetElements.Add("simpleIOGenericIO/GGIO1.AnIn2.mag.f[MX]");
				dataSetElements.Add("simpleIOGenericIO/GGIO1.AnIn3.mag.f[MX]");
				dataSetElements.Add("simpleIOGenericIO/GGIO1.AnIn4.mag.f[MX]");

				string dataSetReference = "simpleIOGenericIO/LLN0.ds1";

                // Note: this function will throw an exception when a data set with the same name already exists
				con.CreateDataSet(dataSetReference, dataSetElements);

				// reconfigure existing RCB with new data set

				string rcbReference = "simpleIOGenericIO/LLN0.RP.EventsRCB01";

				ReportControlBlock rcb = con.GetReportControlBlock(rcbReference);

				rcb.GetRCBValues();

				// note: the second parameter is not required!
				rcb.InstallReportHandler(reportHandler, rcb);

				string rcbDataSetReference = dataSetReference.Replace('.', '$');

				rcb.SetDataSetReference(rcbDataSetReference);
				rcb.SetTrgOps(TriggerOptions.DATA_CHANGED | TriggerOptions.INTEGRITY);				
				rcb.SetIntgPd(5000);
				rcb.SetRptEna(true);

				rcb.SetRCBValues();

				/* run until Ctrl-C is pressed */
				Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e) {
					e.Cancel = true;
					running = false;
				};

				while (running) {
					Thread.Sleep(1000);
					Console.WriteLine("Total memory: " + GC.GetTotalMemory(false));
				}

				// delete the data set
				con.DeleteDataSet("simpleIOGenericIO/LLN0.ds1");

                con.Abort();
            }
            catch (IedConnectionException e)
            {
				Console.WriteLine(e.Message);
            }

        }
    }
}