using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Models;

namespace SIVRB.Views
{
    public class PersonaComboItem
    {
        public int Id { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }

    public partial class RequisitoriaWindow : Window
    {
        public Requisitoria RequisitoriaNueva { get; private set; }
        private readonly bool _esEdicion;

        // Constructor para nueva requisitoria
        public RequisitoriaWindow(List<Persona> personas)
        {
            InitializeComponent();
            _esEdicion = false;
            TxtTitulo.Text = "Nueva Requisitoria";
            RequisitoriaNueva = new Requisitoria();
            DpFechaEmision.SelectedDate = DateTime.Today;

            CargarPersonas(personas);
        }

        // Constructor para editar requisitoria existente
        public RequisitoriaWindow(List<Persona> personas, Requisitoria requisitoria)
        {
            InitializeComponent();
            _esEdicion = true;
            TxtTitulo.Text = "Editar Requisitoria";

            RequisitoriaNueva = new Requisitoria
            {
                Id = requisitoria.Id,
                PersonaId = requisitoria.PersonaId,
                Delito = requisitoria.Delito,
                Juzgado = requisitoria.Juzgado,
                NumeroExpediente = requisitoria.NumeroExpediente,
                FechaEmision = requisitoria.FechaEmision,
                Activa = requisitoria.Activa,
                Descripcion = requisitoria.Descripcion
            };

            CargarPersonas(personas);

            // Seleccionar persona actual
            var items = CmbPersona.ItemsSource as List<PersonaComboItem>;
            if (items != null)
            {
                CmbPersona.SelectedItem = items.FirstOrDefault(i => i.Id == requisitoria.PersonaId);
            }

            // Llenar campos
            TxtDelito.Text = requisitoria.Delito;
            TxtJuzgado.Text = requisitoria.Juzgado;
            TxtExpediente.Text = requisitoria.NumeroExpediente;
            DpFechaEmision.SelectedDate = requisitoria.FechaEmision;
            TxtDescripcion.Text = requisitoria.Descripcion;
            ChkActiva.IsChecked = requisitoria.Activa;

            // No permitir cambiar persona en edicion
            CmbPersona.IsEnabled = false;
        }

        private void CargarPersonas(List<Persona> personas)
        {
            var items = personas.Select(p => new PersonaComboItem
            {
                Id = p.Id,
                DisplayText = $"{p.DNI} — {p.NombreCompleto}"
            }).ToList();

            CmbPersona.ItemsSource = items;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var personaSeleccionada = CmbPersona.SelectedItem as PersonaComboItem;
            if (personaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una persona.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtDelito.Text))
            {
                MessageBox.Show("Ingrese el delito.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtJuzgado.Text))
            {
                MessageBox.Show("Ingrese el juzgado.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RequisitoriaNueva.PersonaId = personaSeleccionada.Id;
            RequisitoriaNueva.Delito = TxtDelito.Text.Trim();
            RequisitoriaNueva.Juzgado = TxtJuzgado.Text.Trim();
            RequisitoriaNueva.NumeroExpediente = TxtExpediente.Text?.Trim() ?? string.Empty;
            RequisitoriaNueva.FechaEmision = DpFechaEmision.SelectedDate ?? DateTime.Today;
            RequisitoriaNueva.Descripcion = TxtDescripcion.Text?.Trim() ?? string.Empty;
            RequisitoriaNueva.Activa = ChkActiva.IsChecked ?? true;

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