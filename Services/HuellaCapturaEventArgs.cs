using System;

namespace Services;

public class HuellaCapturaEventArgs : EventArgs
{
	public byte[] Template { get; set; } = Array.Empty<byte>();

	public int TemplateSize { get; set; }

	public byte[]? ImagenHuella { get; set; }

	public int ImagenAncho { get; set; }

	public int ImagenAlto { get; set; }
}
