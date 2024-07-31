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
            CapturarHuella_1 capturar = new CapturarHuella_1();
            capturar.OnTemplate += this.OnTemplate;
            capturar.ShowDialog();
        }

        private void frmRegistrar_Load(object sender, EventArgs e)
        {
            DisplayFingerprintData(); // Cargar y mostrar datos al cargar el formulario
            dgvListar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
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
            if (Template != null && !string.IsNullOrEmpty(txtNombre.Text) && !string.IsNullOrEmpty(txtApellidos.Text))
            {
                string name = txtNombre.Text;
                string apellidos = txtApellidos.Text;
                byte[] templateBytes = Template.Bytes;
                string templateString = Convert.ToBase64String(templateBytes);

                if (IsValidBase64String(templateString))
                {
                    var fingerprintData = new FingerprintData
                    {
                        Name = name,
                        Apellidos = apellidos,
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
                            // Escribir encabezados si el archivo está vacío
                            var fileExists = File.Exists(filePath);
                            if (!fileExists)
                            {
                                csv.WriteField("Name");
                                csv.WriteField("Apellidos");
                                csv.WriteField("TemplateString");
                                csv.NextRecord();
                            }

                            csv.WriteRecord(fingerprintData);
                            csv.NextRecord();
                        }

                        MessageBox.Show("Datos guardados correctamente en el archivo CSV.", "Registro de huellas digitales");

                        // Actualizar DataGridView después de guardar
                        DisplayFingerprintData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al guardar los datos: {ex.Message}", "Error de registro");
                    }
                }
                else
                {
                    MessageBox.Show("La plantilla de huellas digitales no es válida.", "Error de registro");
                }
            }
            else
            {
                MessageBox.Show("Por favor, asegúrese de capturar la huella digital y proporcionar un nombre y apellidos.", "Error de registro");
            }
        }

        private bool IsValidBase64String(string base64String)
        {
            // Eliminar espacios en blanco adicionales, si los hay
            base64String = base64String.Trim();

            // Comprobar la longitud de la cadena
            if (base64String.Length % 4 != 0)
            {
                return false;
            }

            // Intentar decodificar la cadena Base64
            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private List<FingerprintData> LoadFingerprintData()
        {
            var fingerprintDataList = new List<FingerprintData>();

            try
            {
                // Verifica si el archivo CSV existe antes de intentar leerlo
                if (File.Exists(filePath))
                {
                    using (var reader = new StreamReader(filePath))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        fingerprintDataList = csv.GetRecords<FingerprintData>().ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos: {ex.Message}", "Error de carga");
            }

            return fingerprintDataList;
        }

        private void DisplayFingerprintData()
        {
            // Cargar datos del archivo CSV
            var data = LoadFingerprintData();

            // Limpiar cualquier dato existente
            dgvListar.Rows.Clear();

            // Configurar el DataGridView
            dgvListar.ColumnCount = 3;
            dgvListar.Columns[0].Name = "Nombre";
            dgvListar.Columns[1].Name = "Apellidos";
            dgvListar.Columns[2].Name = "Huella Digital (Base64)";

            // Añadir filas al DataGridView
            foreach (var item in data)
            {
                dgvListar.Rows.Add(item.Name, item.Apellidos, item.TemplateString);
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void btnBorrar_Click_1(object sender, EventArgs e)
        {
            // Verificar si se ha seleccionado una fila en el DataGridView
            if (dgvListar.SelectedRows.Count > 0)
            {
                // Obtener la fila seleccionada
                var selectedRow = dgvListar.SelectedRows[0];

                // Extraer la información del registro seleccionado
                string nameToDelete = selectedRow.Cells[0].Value.ToString();
                string apellidosToDelete = selectedRow.Cells[1].Value.ToString();

                // Mostrar mensaje de confirmación
                var result = MessageBox.Show("¿Estás seguro de que deseas eliminar este registro?",
                                             "Confirmar eliminación",
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Question);

                // Si el usuario selecciona 'Sí'
                if (result == DialogResult.Yes)
                {
                    // Cargar los datos actuales del archivo CSV
                    var data = LoadFingerprintData();

                    // Encontrar y eliminar el registro correspondiente
                    var recordToDelete = data.FirstOrDefault(d => d.Name == nameToDelete && d.Apellidos == apellidosToDelete);
                    if (recordToDelete != null)
                    {
                        data.Remove(recordToDelete);

                        // Reescribir el archivo CSV sin el registro eliminado
                        try
                        {
                            using (var writer = new StreamWriter(filePath, false))
                            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                            {
                                csv.WriteRecords(data);
                            }

                            // Actualizar el DataGridView
                            DisplayFingerprintData();
                            MessageBox.Show("Registro eliminado correctamente.", "Eliminación exitosa");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error al eliminar el registro: {ex.Message}", "Error de eliminación");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Registro no encontrado.", "Error de eliminación");
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un registro para eliminar.", "Error de eliminación");
            }
        }


    }

    public class FingerprintData
    {
        public string Name { get; set; }
        public string Apellidos { get; set; }
        public string TemplateString { get; set; }
    }
}
