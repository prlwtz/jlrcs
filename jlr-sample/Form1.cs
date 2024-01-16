using System.Dynamic;
using static jlr_sample.JLRCS;
using System.Text.Json.Nodes;
using System.Diagnostics.Eventing.Reader;

namespace jlr_sample
{
    public partial class Form1 : Form
    {

        private string? device_id;
        private JLRCS.Connection? jlr_connection;


        public Form1()
        {
            InitializeComponent();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (this.jlr_connection == null && !string.IsNullOrEmpty(this.textBoxUser.Text) && !string.IsNullOrEmpty(this.textBoxPass.Text))
            {
                try
                {
                    if (!string.IsNullOrEmpty(this.device_id))
                    {
                        this.jlr_connection = new JLRCS.Connection(this.textBoxUser.Text, this.textBoxPass.Text, this.device_id);
                    }
                    else
                    {
                        this.jlr_connection = new JLRCS.Connection(this.textBoxUser.Text, this.textBoxPass.Text);

                        this.device_id = this.jlr_connection!.device_id!;
                        this.textBoxDeviceID.Text = this.device_id;

                    }
                    this.labelNumVehicles.Text = this.jlr_connection.vehicles!.Count.ToString();
                    Settings1.Default.User = this.textBoxUser.Text;
                    Settings1.Default.Pass = this.textBoxPass.Text;
                    Settings1.Default.DeviceID = this.device_id;
                    Settings1.Default.Save();
                    Settings1.Default.Reload();
                    foreach (JLRCS.Vehicle vehicle in this.jlr_connection.vehicles!)
                    {
                        this.listViewVehicles.Items.Add(vehicle.vin);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            this.textBoxDeviceID.Text = string.Empty;
            this.device_id = string.Empty;
            Settings1.Default.DeviceID = this.device_id;
            Settings1.Default.Save();
            Settings1.Default.Reload();
            this.listViewVehicles.Items.Clear();
            this.labelNumVehicles.Text = this.listViewVehicles.Items.Count.ToString();
            this.listViewVehicleStatus.Items.Clear();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.textBoxUser.Text = Settings1.Default.User;
            this.textBoxPass.Text = Settings1.Default.Pass;
            this.textBoxDeviceID.Text = Settings1.Default.DeviceID;
            this.device_id = this.textBoxDeviceID.Text;
            this.listViewVehicles.Items.Clear();
            this.labelNumVehicles.Text = this.listViewVehicles.Items.Count.ToString();
            this.listViewVehicleStatus.Items.Clear();
        }

        private void listViewVehicles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listViewVehicles.SelectedItems.Count > 0)
            {
                JLRCS.Vehicle vehicle = this.jlr_connection!.vehicles![this.listViewVehicles.SelectedIndices[0]];
                this.RefreshVehicleStatus(vehicle);

            }
            else
            {
                this.textBoxVehicleLastUpdated.Text = string.Empty;
                this.listViewVehicleStatus.Items.Clear();
            }

        }

        private void RefreshVehicleStatus(JLRCS.Vehicle vehicle)
        {
            this.listViewVehicleStatus.Items.Clear();
            JsonNode? vehicle_status = vehicle.GetStatus();
            JsonArray? core_status = vehicle_status!["vehicleStatus"]!["coreStatus"] as JsonArray;
            JsonArray? ev_status = vehicle_status!["vehicleStatus"]!["evStatus"] as JsonArray;

            string lastUpdated = vehicle_status!["lastUpdatedTime"]!.ToString();
            this.textBoxVehicleLastUpdated.Text = lastUpdated.Replace("T", " ");

            foreach (JsonNode? node in core_status!)
            {
                JsonObject node_object = node!.AsObject();

                string node_name = node_object["key"]!.ToString();
                string node_value = node_object["value"]!.ToString();

                this.listViewVehicleStatus.Items.Add(new ListViewItem(new string[] { node_name, node_value }));
            }

            foreach (JsonNode? node in ev_status!)
            {
                JsonObject node_object = node!.AsObject();

                string node_name = node_object["key"]!.ToString();
                string node_value = node_object["value"]!.ToString();

                this.listViewVehicleStatus.Items.Add(new ListViewItem(new string[] { node_name, node_value }));

            }
        }


        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (this.listViewVehicles.SelectedItems.Count > 0)
            {
                JLRCS.Vehicle vehicle = this.jlr_connection!.vehicles![this.listViewVehicles.SelectedIndices[0]];
                JsonNode? vehicle_health_status = vehicle.GetHealthStatus();
                this.RefreshVehicleStatus(vehicle);
            }
            }
    }
}
