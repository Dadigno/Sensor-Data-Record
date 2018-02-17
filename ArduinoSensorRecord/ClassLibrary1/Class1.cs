using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace ArduinoSensorRecord
{
    public class Class1 : Form
    {
        public static SerialPort port;
        public static List<List<int>> series = new List<List<int>>();
        private static List<string> commandAccepted = new List<string> { "Two sensor setted\r" , "One sensor setted\r" , "Command Unknown\r"};
        

        public static void InitSerialConnection(string portName, int baudRate)
        {
            port = new SerialPort(portName, baudRate)
            {
                ReadTimeout = 5000,
                DtrEnable = true
            };
        }

        public static int[] Decode(string command, Chart series)  //per ogni elemente ci deve essere un valore per una serie di dati
        {
            int[] values = null;
            command = command.Remove(command.Length-1);
            if (!commandAccepted.Contains(command))
            {
                string[] seg = command.Split(';');
                values = new int[series.Series.Count];
                for (int i = 0; i < series.Series.Count; i++)
                {
                    values[i] = Convert.ToInt32(seg[i]);
                }
            }
            return values;
        }

        public static void ExportExcelData(string path, Chart chart1)
        {
            Excel.Application xlApp = new Excel.Application();
            if(xlApp == null)
            {
                MessageBox.Show("Excel is not properly installed");
                return;
            }
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;
            xlWorkBook = xlApp.Workbooks.Add(misValue);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            for(int cols = 0; cols < chart1.Series.Count; cols++)    //i indica il numero della serie di dati (una per ogni sensore)
            {
                xlWorkSheet.Cells[1, cols + 1 ] = chart1.Series[cols].Name;
                for(int row = 0; row < chart1.Series[cols].Points.Count; row++)  //k indica il dato all'interno della serie
                {
                    int a = Convert.ToInt32(series[cols][row]);
                    xlWorkSheet.Cells[row + 2, cols + 1] = a;
                }
            }

            xlWorkBook.SaveAs(path, Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorkSheet);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);

        }

    }
}
