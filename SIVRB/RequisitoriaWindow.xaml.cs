using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Models;

namespace SIVRB.Views;

public partial class RequisitoriaWindow : Window
{
	private readonly bool _esEdicion;

	public Requisitoria RequisitoriaNueva { get; private set; }

	public RequisitoriaWindow(List<Persona> personas)
	{
		InitializeComponent();
		_esEdicion = false;
		TxtTitulo.Text = "Nueva Requisitoria";
		RequisitoriaNueva = new Requisitoria();
		DpFechaEmision.SelectedDate = DateTime.Today;
		CargarPersonas(personas);
	}

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
		if (CmbPersona.ItemsSource is List<PersonaComboItem> source)
		{
			CmbPersona.SelectedItem = source.FirstOrDefault((PersonaComboItem i) => i.Id == requisitoria.PersonaId);
		}
		TxtDelito.Text = requisitoria.Delito;
		TxtJuzgado.Text = requisitoria.Juzgado;
		TxtExpediente.Text = requisitoria.NumeroExpediente;
		DpFechaEmision.SelectedDate = requisitoria.FechaEmision;
		TxtDescripcion.Text = requisitoria.Descripcion;
		ChkActiva.IsChecked = requisitoria.Activa;
		CmbPersona.IsEnabled = false;
	}

	private void CargarPersonas(List<Persona> personas)
	{
		List<PersonaComboItem> itemsSource = personas.Select((Persona p) => new PersonaComboItem
		{
			Id = p.Id,
			DisplayText = p.DNI + " — " + p.NombreCompleto
		}).ToList();
		CmbPersona.ItemsSource = itemsSource;
	}

	private void BtnGuardar_Click(object sender, RoutedEventArgs e)
	{
		if (!(CmbPersona.SelectedItem is PersonaComboItem personaComboItem))
		{
			MessageBox.Show("Seleccione una persona.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		if (string.IsNullOrWhiteSpace(TxtDelito.Text))
		{
			MessageBox.Show("Ingrese el delito.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		if (string.IsNullOrWhiteSpace(TxtJuzgado.Text))
		{
			MessageBox.Show("Ingrese el juzgado.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		RequisitoriaNueva.PersonaId = personaComboItem.Id;
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
