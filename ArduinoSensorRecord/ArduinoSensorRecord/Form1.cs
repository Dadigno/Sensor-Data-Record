using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SensorDataRecord
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            GetAvailablePorts();
        }

        private delegate void connectionHandler();
        private connectionHandler delegateConnection;
        string sender = "Sent: ";
        string received = "Received: ";
        String[] ports;

        public void GetAvailablePorts()
        {
            ports = SerialPort.GetPortNames();
            comboBoxPorts.Items.Clear();
            comboBoxPorts.Items.AddRange(ports);
            delegateConnection = OpenConnection;
        }

        public void DrawGraph(int[] values)
        {
            if (values != null)
            {
                for (int i = 0; i < chart1.Series.Count; i++)
                {
                    chart1.Series[i].Points.AddY(values[i]);
                    Class1.series[i].Add(values[i]);
                }
            }
        }

        private void OpenConnection()
        {
            string selectedPort = comboBoxPorts.GetItemText(comboBoxPorts.SelectedItem);
            int baudRate = Convert.ToInt32(comboBoxBaudRate.Text);

            Class1.InitSerialConnection(selectedPort, baudRate);

            if (!Class1.port.IsOpen)
            {
                listMessages.Items.Add("Tentativo di connessione");
                Class1.port.Open();
                string response = Class1.port.ReadLine();
                if (response == "start\r")
                {
                    listMessages.Items.Add("Connesso");
                    delegateConnection = CloseConnection;
                    comboBoxPorts.Enabled = false;
                    comboBoxBaudRate.Enabled = false;
                    buttonSend.Enabled = true;
                    button1.Text = "Disconnect";
                    button5.Enabled = false;
                    button4.Enabled = false;
                    Class1.port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(SerialPort1_DataReceived);
                }
                else
                {
                    listMessages.Items.Add("Connection error: " + response);
                    Class1.port.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(SerialPort1_DataReceived);
                    CloseConnection();
                }
            }

        }

        private void CloseConnection()
        {
            Class1.port.Close();
            if (!Class1.port.IsOpen)
            {
                listMessages.Items.Add("Disconnesso");
                delegateConnection = OpenConnection;
                comboBoxPorts.Enabled = true;
                comboBoxBaudRate.Enabled = true;
                buttonSend.Enabled = false;
                button1.Text = "Connect";
                button5.Enabled = true;
                button4.Enabled = true;
            }
        }

        private void PrintOnMonitor(string msg, bool? dir)       //false=received, true=sent, null=internal command
        {
            if (dir == false)
                listMessages.Items.Add(received + msg);
            else if (dir == true)
                listMessages.Items.Add(sender + msg);
            else
                listMessages.Items.Add(msg);

            if (checkBoxScroll.Checked)
            {
                listMessages.SelectedIndex = listMessages.Items.Count - 1;
                listMessages.SelectedIndex = -1;
            }
        }

        //
        //Events Handlers
        //
        private void DataSerialReceived(object sender, EventArgs e)
        {
            string command = Class1.port.ReadLine();
            PrintOnMonitor(command, false);
            DrawGraph(Class1.Decode(command, chart1));
        }

        private void SerialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            this.Invoke(new EventHandler(DataSerialReceived));
        }

        private void ButtonSend_Click(object sender, EventArgs e)
        {
            string command = textBoxCommand.Text;
            Class1.port.Write(command);
            PrintOnMonitor(command, true);
            textBoxCommand.Text = "";
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (comboBoxBaudRate.Text == "" || comboBoxPorts.Text == "")
            {
                PrintOnMonitor("Devi impostare la porta seriale", null);
            }
            else
            {
                if (chart1.Series.Count == 0)
                {
                    MessageBox.Show("Sorry, You must add a chart first","Ops!!", MessageBoxButtons.OK,MessageBoxIcon.Warning);
                }
                else
                {
                    try
                    {
                        delegateConnection();
                    }
                    catch (System.IO.IOException)
                    {
                        PrintOnMonitor("Il tentativo di connessione non è andato a buon fine", null);
                        CloseConnection();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        PrintOnMonitor("Accesso non autorizzato", null);
                        CloseConnection();
                    }
                    catch (TimeoutException)
                    {
                        PrintOnMonitor("Connection timeout", null);
                        CloseConnection();
                    }
                    catch (ArgumentException)
                    {
                        PrintOnMonitor("Invalid COM port, must start with 'COM' ", null);
                        CloseConnection();
                    }
                }
            }
        }

        private void ComboBoxPorts_DropDown(object sender, EventArgs e)
        {
            GetAvailablePorts();
        }

        private void ButtonClearGraph_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            listMessages.Items.Clear();
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisX.Interval = (double)numericUpDown1.Value;
        }

        private void NumericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisX.IntervalOffset = (double)numericUpDown2.Value;
        }

        private void NumericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisY.Interval = (double)numericUpDown4.Value;
        }

        private void NumericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisY.IntervalOffset = (double)numericUpDown3.Value;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            DialogResult DResult = Save.ShowDialog();
            if (DResult != DialogResult.Cancel && DResult != DialogResult.Abort)
            {
                string filePath = Save.FileName;
                chart1.SaveImage(filePath, ChartImageFormat.Png);
            }
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            if (chart1.Series.Count != 3)
            {
                DialogResult DResult = colorDialog1.ShowDialog();
                if (DResult != DialogResult.Cancel && DResult != DialogResult.Abort)
                {
                    Series newSerie = new Series
                    {
                        ChartType = SeriesChartType.Line,
                        Name = "Sensor " + (chart1.Series.Count + 1),
                        BorderWidth = 3,
                    };
                    newSerie.Color = colorDialog1.Color;
                    chart1.Series.Add(newSerie);
                    Class1.series.Add(new List<int>());
                }
            }
            else
            {
                MessageBox.Show("Sorry, maximun three charts!", "Ops!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            DialogResult DResult = SaveExcel.ShowDialog();
            if (DResult != DialogResult.Cancel && DResult != DialogResult.Abort)
            {
                string filePath = SaveExcel.FileName;
                Class1.ExportExcelData(filePath, chart1);
            }
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Dadigno");
        }
    }
}
