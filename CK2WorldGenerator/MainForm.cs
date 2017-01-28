using CK2WorldGenerator.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CK2WorldGenerator
{
    public partial class MainForm : Form, IGeneratorFrontend
    {
        private delegate void LogEntryDelegate(string text);

        private WorldGenerator Generator;
        
        public MainForm()
        {
            InitializeComponent();
            Generator = new WorldGenerator(this);
            TopologyMap.Image = Generator.TopologyMap;
            TerrainMap.Image = Generator.TerrainMap;
            RiversMap.Image = Generator.RiversMap;
            ProvincesMap.Image = Generator.ProvincesMap;
            CulturesMap.Image = Generator.CulturesMap;
        }

        public void AddLogEntry(string text)
        {
            if (LogTextBox.InvokeRequired)
            {
                LogTextBox.Invoke(new LogEntryDelegate(AddLogEntry), text);
            }
            else
            {
                LogTextBox.AppendText(text);
            }
        }

        private async void GenerateWorldButton_Click(object sender, EventArgs e)
        {
            GenerateWorldButton.Enabled = false;
            SaveButton.Enabled = false;
            MainTabControl.SelectedTab = LogTab;
#if DEBUG
            Generator.GenerateWorld();
#else
            await Task.Run(() => Generator.GenerateWorld());
#endif
            GenerateWorldButton.Enabled = true;
            SaveButton.Enabled = true;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void ProvincesMinControl_ValueChanged(object sender, EventArgs e)
        {
        }

        private void ProvincesMaxControl_ValueChanged(object sender, EventArgs e)
        {
        }

        private void SeaLevelControl_ValueChanged(object sender, EventArgs e)
        {
            Generator.Settings.SeaLevel = (byte)SeaLevelControl.Value;
        }

        private void BaseHeightControl_ValueChanged(object sender, EventArgs e)
        {
            Generator.Settings.BaseHeight = (byte)BaseHeightControl.Value;
        }

        private void HeightScaleControl_ValueChanged(object sender, EventArgs e)
        {
            Generator.Settings.HeightScale = (byte)HeightScaleControl.Value;
        }

        private void RoughnessControl_ValueChanged(object sender, EventArgs e)
        {
            Generator.Settings.Roughness = (double)RoughnessControl.Value;
        }

        private void NoiseControl_ValueChanged(object sender, EventArgs e)
        {
            Generator.Settings.Noise = (double)NoiseControl.Value;
        }

        private void LandProvincesMinControl_ValueChanged(object sender, EventArgs e)
        {
            Generator.Settings.LandProvincesMin = (int)LandProvincesMinControl.Value;
            if (LandProvincesMinControl.Value > LandProvincesMaxControl.Value)
                LandProvincesMaxControl.Value = LandProvincesMinControl.Value;
        }

        private void LandProvincesMaxControl_ValueChanged(object sender, EventArgs e)
        {
            Generator.Settings.LandProvincesMax = (int)LandProvincesMaxControl.Value;
            if (LandProvincesMaxControl.Value < LandProvincesMinControl.Value)
                LandProvincesMinControl.Value = LandProvincesMaxControl.Value;
        }

        private void SeaProvincesMinControl_ValueChanged(object sender, EventArgs e)
        {
            Generator.Settings.SeaProvincesMin = (int)SeaProvincesMinControl.Value;
            if (SeaProvincesMinControl.Value > SeaProvincesMaxControl.Value)
                SeaProvincesMaxControl.Value = SeaProvincesMinControl.Value;
        }

        private void SeaProvincesMaxControl_ValueChanged(object sender, EventArgs e)
        {
            Generator.Settings.SeaProvincesMax = (int)SeaProvincesMaxControl.Value;
            if (SeaProvincesMaxControl.Value < SeaProvincesMinControl.Value)
                SeaProvincesMinControl.Value = SeaProvincesMaxControl.Value;
        }

        private void WorldNameControl_TextChanged(object sender, EventArgs e)
        {

        }

        private void RandomSeedControl_TextChanged(object sender, EventArgs e)
        {
            Generator.Settings.RandomSeed = RandomSeedControl.Text;
        }

        private void CultureGroupsMinControl_ValueChanged(object sender, EventArgs e)
        {
            Generator.Settings.CultureGroupsMin = (int)CultureGroupsMinControl.Value;
            if (CultureGroupsMinControl.Value > CultureGroupsMaxControl.Value)
                CultureGroupsMaxControl.Value = CultureGroupsMinControl.Value;
            if (CultureGroupsMinControl.Value > CulturesMinControl.Value)
                CulturesMinControl.Value = CultureGroupsMinControl.Value;
            if (CultureGroupsMinControl.Value > CulturesMaxControl.Value)
                CulturesMaxControl.Value = CultureGroupsMinControl.Value;
        }

        private void CultureGroupsMaxControl_ValueChanged(object sender, EventArgs e)
        {
            Generator.Settings.CultureGroupsMax = (int)CultureGroupsMaxControl.Value;
            if (CultureGroupsMaxControl.Value < CultureGroupsMinControl.Value)
                CultureGroupsMinControl.Value = CultureGroupsMaxControl.Value;
            if (CultureGroupsMaxControl.Value > CulturesMaxControl.Value)
                CulturesMaxControl.Value = CultureGroupsMaxControl.Value;
        }

        private void CulturesMinControl_ValueChanged(object sender, EventArgs e)
        {
            Generator.Settings.CulturesMin = (int)CulturesMinControl.Value;
            if (CulturesMinControl.Value > CulturesMaxControl.Value)
                CulturesMaxControl.Value = CulturesMinControl.Value;
            if (CulturesMinControl.Value < CultureGroupsMinControl.Value)
                CultureGroupsMinControl.Value = CulturesMinControl.Value;
        }

        private void CulturesMaxControl_ValueChanged(object sender, EventArgs e)
        {
            Generator.Settings.CulturesMax = (int)CulturesMaxControl.Value;
            if (CulturesMaxControl.Value < CulturesMinControl.Value)
                CulturesMinControl.Value = CulturesMaxControl.Value;
            if (CulturesMaxControl.Value < CultureGroupsMaxControl.Value)
                CultureGroupsMaxControl.Value = CulturesMaxControl.Value;
        }

    }
}
