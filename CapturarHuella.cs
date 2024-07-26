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
    public partial class CapturarHuella : Form
    {
        public delegate void OnTemplateEventHandler(DPFP.Template template);

        public event OnTemplateEventHandler OnTemplate;

        private DPFP.Processing.Enrollment Enroller;
        public CapturarHuella()
        {
            InitializeComponent();
        }

        private void CapturarHuella_Load(object sender, EventArgs e)
        {

        }
    }
}
