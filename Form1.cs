
using DigitalPerson4500;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace DititalPerson4500
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            frmRegistrar registrar = new frmRegistrar();
            registrar.ShowDialog();
        }

        private void btnVerificar_Click(object sender, EventArgs e)
        {
            frmVerificar verificar = new frmVerificar();
            verificar.ShowDialog();
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }
    }
}
