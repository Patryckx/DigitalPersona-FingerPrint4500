using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CsvHelper;
using CsvHelper.Configuration;
using DPFP;
using DPFP.Capture;

namespace DititalPerson4500
{
    public partial class frmVerificar : CaptureForm
    {
        private DPFP.Template Template;
        private DPFP.Verification.Verification Verificator;
        private readonly string iniFilePath;
        private readonly string folderPath = @"C:\Fingerprint Registers";

        public frmVerificar()
        {
            InitializeComponent();
            Verificator = new DPFP.Verification.Verification();
            iniFilePath = Path.Combine(folderPath, "verification_flag.ini");
            this.FormClosing += new FormClosingEventHandler(frmVerificar_FormClosing);
        }

        public void Verify(DPFP.Template template)
        {
            Template = template;
            ShowDialog();
        }

        protected override void Init()
        {
            base.Init();
            base.Text = "Verificación de Huella Digital";
            Verificator = new DPFP.Verification.Verification();
            UpdateStatus(0);
            ResetVerificationFlag();
        }

        private void UpdateStatus(int FAR)
        {
            SetStatus(String.Format("False Accept Rate (FAR) = {0}", FAR));
        }

        protected override void Process(DPFP.Sample Sample)
        {
            base.Process(Sample);

            DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Verification);

            if (features != null)
            {
                DPFP.Verification.Verification.Result result = new DPFP.Verification.Verification.Result();

                var fingerprintDataList = LoadFingerprintDataFromCsv();

                foreach (var fingerprintData in fingerprintDataList)
                {
                    byte[] templateBytes = Convert.FromBase64String(fingerprintData.TemplateString);
                    using (var stream = new MemoryStream(templateBytes))
                    {
                        DPFP.Template template = new DPFP.Template(stream);

                        Verificator.Verify(features, template, ref result);
                        UpdateStatus(result.FARAchieved);

                        if (result.Verified)
                        {
                            MakeReport("La huella digital fue VERIFICADA. " + fingerprintData.Name + " " + fingerprintData.Apellidos);

                            // Escritura del archivo INI antes de iniciar la espera
                            SetVerificationFlag(true, fingerprintData.Name, fingerprintData.Apellidos);

                            // Espera de 7 segundos antes de cerrar
                            Timer timer = new Timer();
                            timer.Interval = 7000; // 7000 ms = 7 s
                            timer.Tick += (s, e) =>
                            {
                                timer.Stop();
                                this.Close(); // Cierra la ventana después de la espera
                            };
                            timer.Start();

                            return;
                        }
                    }
                }

                MakeReport("La huella digital NO fue verificada.");
                SetVerificationFlag(false, null, null);
            }
        }

        private List<FingerprintData> LoadFingerprintDataFromCsv()
        {
            var fingerprintDataList = new List<FingerprintData>();
            var filePath = Path.Combine(folderPath, "fingerprint_data.csv");

            try
            {
                if (File.Exists(filePath))
                {
                    using (var reader = new StreamReader(filePath))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        fingerprintDataList = csv.GetRecords<FingerprintData>().ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos del CSV: {ex.Message}", "Error de carga");
            }

            return fingerprintDataList;
        }

        private void SetVerificationFlag(bool isValidated, string name, string apellidos)
        {
            EnsureDirectoryExists();

            if (!File.Exists(iniFilePath))
            {
                using (var writer = new StreamWriter(iniFilePath))
                {
                    writer.WriteLine("[Fingerprint]");
                }
            }

            var iniFile = new IniFile(iniFilePath);
            iniFile.WriteValue("Fingerprint", "isvalidated", isValidated ? "true" : "none");
            iniFile.WriteValue("Fingerprint", "nombre", isValidated ? name : "none");
            iniFile.WriteValue("Fingerprint", "apellidos", isValidated ? apellidos : "none");
        }

        private void ResetVerificationFlag()
        {
            EnsureDirectoryExists();

            if (File.Exists(iniFilePath))
            {
                var iniFile = new IniFile(iniFilePath);
                iniFile.WriteValue("Fingerprint", "isvalidated", "none");
                iniFile.WriteValue("Fingerprint", "nombre", "none");
                iniFile.WriteValue("Fingerprint", "apellidos", "none");
            }
        }

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        private void frmVerificar_Load(object sender, EventArgs e)
        {
            // Implementa cualquier lógica adicional si es necesario
        }

        private void frmVerificar_FormClosing(object sender, FormClosingEventArgs e)
        {
            SetVerificationFlag(false, null, null);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            SetVerificationFlag(false, null, null);
            this.Close();
        }
    }

    public class FingerprintData
    {
        public string Name { get; set; }
        public string Apellidos { get; set; }
        public string TemplateString { get; set; }
    }

    public class IniFile
    {
        private readonly string _path;

        public IniFile(string path)
        {
            _path = path;
        }

        public void WriteValue(string section, string key, string value)
        {
            // Implementación para escribir un valor en el archivo INI
            WritePrivateProfileString(section, key, value, _path);
        }

        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
    }
}
