using System.Windows;
using Models;

namespace SIVRB.Views
{
    public partial class EditPersonaWindow : Window
    {
        public Persona PersonaEditada { get; private set; }

        public EditPersonaWindow(Persona persona)
        {
            InitializeComponent();

            PersonaEditada = new Persona
            {
                Id = persona.Id,
                DNI = persona.DNI,
                NombreCompleto = persona.NombreCompleto,
                Direccion = persona.Direccion,
                Telefono = persona.Telefono,
                MotivoRegistro = persona.MotivoRegistro,
                Observaciones = persona.Observaciones,
                HuellaTemplate = persona.HuellaTemplate,
                HuellaTemplateSize = persona.HuellaTemplateSize,
                FechaRegistro = persona.FechaRegistro
            };

            // Llenar campos
            TxtDNI.Text = persona.DNI;
            TxtNombre.Text = persona.NombreCompleto;
            TxtDireccion.Text = persona.Direccion;
            TxtTelefono.Text = persona.Telefono;
            TxtMotivo.Text = persona.MotivoRegistro;
            TxtObservaciones.Text = persona.Observaciones;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtDNI.Text) || TxtDNI.Text.Length != 8)
            {
                MessageBox.Show("El DNI debe tener 8 digitos.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtNombre.Text))
            {
                MessageBox.Show("Ingrese el nombre completo.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PersonaEditada.DNI = TxtDNI.Text.Trim();
            PersonaEditada.NombreCompleto = TxtNombre.Text.Trim();
            PersonaEditada.Direccion = TxtDireccion.Text?.Trim() ?? string.Empty;
            PersonaEditada.Telefono = TxtTelefono.Text?.Trim() ?? string.Empty;
            PersonaEditada.MotivoRegistro = TxtMotivo.Text?.Trim() ?? string.Empty;
            PersonaEditada.Observaciones = TxtObservaciones.Text?.Trim() ?? string.Empty;

            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}