using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsvHelper;
using System.IO;
using System.Globalization;
using DititalPerson4500;

namespace DigitalPerson4500
{
    public partial class frmRegistrar : Form
    {
        private DPFP.Template Template;
        // Ruta estática donde se guardará el archivo CSV
        private readonly string directoryPath = @"C:\Fingerprint Registers";
        private readonly string filePath;

        public frmRegistrar()
        {
            InitializeComponent();
            filePath = Path.Combine(directoryPath, "fingerprint_data.csv");
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Event handler for label1 click event
        }

        private void label2_Click(object sender, EventArgs e)
        {
            // Event handler for label2 click event
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            CapturarHuella capturar = new CapturarHuella();
            capturar.OnTemplate += this.OnTemplate;
            capturar.ShowDialog();
        }

        private void frmRegistrar_Load(object sender, EventArgs e)
        {
            // Form load event handler
        }

        private void OnTemplate(DPFP.Template template)
        {
            this.Invoke(new Action(delegate ()
            {
                Template = template;
                btnAgregar.Enabled = (Template != null);
                if (Template != null)
                {
                    MessageBox.Show("La plantilla de huellas digitales está lista para la verificación.", "Registro de huellas digitales");
                    txtHuella.Text = "Huella capturada correctamente";
                }
                else
                {
                    MessageBox.Show("La plantilla de huellas digitales no es válida. Repita el registro de huellas digitales.", "Registro de huellas digitales");
                }
            }));
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (Template != null && !string.IsNullOrEmpty(txtNombre.Text))
            {
                string name = txtNombre.Text;
                byte[] templateBytes = Template.Bytes;
                string templateString = Convert.ToBase64String(templateBytes);

                var fingerprintData = new FingerprintData
                {
                    Name = name,
                    TemplateString = templateString
                };

                try
                {
                    // Crear el directorio si no existe
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    using (var writer = new StreamWriter(filePath, append: true))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecord(fingerprintData);
                        csv.NextRecord();
                    }

                    MessageBox.Show("Datos guardados correctamente en el archivo CSV.", "Registro de huellas digitales");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar los datos: {ex.Message}", "Error de registro");
                }
            }
            else
            {
                MessageBox.Show("Por favor, asegúrese de capturar la huella digital y proporcionar un nombre.", "Error de registro");
            }
        }

       
    }

    public class FingerprintData
    {
        public string Name { get; set; }
        public string TemplateString { get; set; }
    }
}
