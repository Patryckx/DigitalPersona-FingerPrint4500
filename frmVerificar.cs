using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using DPFP; // Asegúrate de que esta biblioteca esté referenciada en tu proyecto

namespace DititalPerson4500
{
    public partial class frmVerificar : CaptureForm
    {
        private DPFP.Template Template;
        private DPFP.Verification.Verification Verificator;
        private readonly string iniFilePath;

        public frmVerificar()
        {
            InitializeComponent();
            Verificator = new DPFP.Verification.Verification(); // Crear un verificador de plantillas de huella digital
            iniFilePath = Path.Combine(@"C:\Fingerprint Registers", "verification_flag.ini");
            this.FormClosing += new FormClosingEventHandler(frmVerificar_FormClosing); // Suscribir al evento FormClosing
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
            ResetVerificationFlag(); // Resetear la bandera al iniciar
        }

        private void UpdateStatus(int FAR)
        {
            // Mostrar el valor "False accept rate" (FAR)
            SetStatus(String.Format("False Accept Rate (FAR) = {0}", FAR));
        }

        protected override void Process(DPFP.Sample Sample)
        {
            base.Process(Sample);

            // Procesar la muestra y crear un conjunto de características para el propósito de verificación.
            DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Verification);

            // Comprobar la calidad de la muestra y empezar la verificación si es buena
            if (features != null)
            {
                // Comparar el conjunto de características con nuestras plantillas almacenadas
                DPFP.Verification.Verification.Result result = new DPFP.Verification.Verification.Result();

                // Cargar huellas digitales desde el archivo CSV
                var fingerprintDataList = LoadFingerprintDataFromCsv();

                foreach (var fingerprintData in fingerprintDataList)
                {
                    // Convertir la cadena Base64 a bytes y crear una plantilla de huella digital
                    byte[] templateBytes = Convert.FromBase64String(fingerprintData.TemplateString);
                    using (var stream = new MemoryStream(templateBytes))
                    {
                        DPFP.Template template = new DPFP.Template(stream);

                        // Verificar la muestra capturada contra la plantilla cargada
                        Verificator.Verify(features, template, ref result);
                        UpdateStatus(result.FARAchieved);

                        if (result.Verified)
                        {
                            MakeReport("La huella digital fue VERIFICADA. " + fingerprintData.Name + " " + fingerprintData.Apellidos);
                            SetVerificationFlag(true); // Establecer la bandera
                            return; // Si se verifica, salir del bucle
                        }
                    }
                }

                // Si no se verifica ninguna huella
                MakeReport("La huella digital NO fue verificada.");
                SetVerificationFlag(false); // Establecer la bandera si no se verifica
            }
        }

        private List<FingerprintData> LoadFingerprintDataFromCsv()
        {
            var fingerprintDataList = new List<FingerprintData>();
            var filePath = Path.Combine(@"C:\Fingerprint Registers", "fingerprint_data.csv");

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

        private void SetVerificationFlag(bool isValidated)
        {
            if (!File.Exists(iniFilePath))
            {
                using (var writer = new StreamWriter(iniFilePath))
                {
                    writer.WriteLine("[Fingerprint]");
                    writer.WriteLine($"isvalidated={(isValidated ? "true" : "none")}");
                }
            }
            else
            {
                var iniFile = new Class1.IniFile(iniFilePath);
                iniFile.WriteValue("Fingerprint", "isvalidated", isValidated ? "true" : "none");
            }
        }

        private void ResetVerificationFlag()
        {
            if (File.Exists(iniFilePath))
            {
                SetVerificationFlag(false);
            }
        }

        private void frmVerificar_Load(object sender, EventArgs e)
        {
            // Implementa cualquier lógica adicional si es necesario
        }

        private void frmVerificar_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Restaurar la bandera a "none" cuando se cierre el formulario
            SetVerificationFlag(false);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            // Restaurar la bandera a "none" cuando se cierra el formulario mediante el botón de cerrar
            SetVerificationFlag(false);
            this.Close(); // Cerrar el formulario
        }
    }

    public class FingerprintData
    {
        public string Name { get; set; }
        public string Apellidos { get; set; }
        public string TemplateString { get; set; } // Esta propiedad almacena la huella dactilar codificada en Base64
    }
}
