using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using Eto.Drawing;
using Eto.WinForms.Forms;
using Eto.Wpf.Forms;
using Eto.GtkSharp.Forms;


namespace pedopornanalyzer
{
	public class MyForm: Form 
	{
		public MyForm()
		{
			// Title to show in the title bar
			Title = "Analyzer Pedoporngraphy Images";
	
			BackgroundColor = new Color (0/255, 0/255, 255/255, 1); //Scelto il colore RGB(x,y,z), per impostarlo correttamente si scrive nella forma (x/255,y/255,z/255,1)

			ClientSize = new Size (1450, 930);

			Button start;

			TabControl tab = new TabControl ();

			MyTabInfo tabinfo = new MyTabInfo ();

			MyTabAnalysis tabanalysis = new MyTabAnalysis ();
		
			MyTabGallery tabgallery = new MyTabGallery ();

			Bitmap pplogo = new Bitmap(                      //immagine del logo della polizia postale:lo recupero dalla cartella Resources
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream("pedopornanalyzer.Properties.Resources.polizia.png"));

			Bitmap univpmlogo = new Bitmap(					//immagine del logo della UNIVPM:lo recupero dalla cartella Resources
				System.Reflection.Assembly.GetEntryAssembly().
				GetManifestResourceStream("pedopornanalyzer.Properties.Resources.univpm2.png"));

			Content = new Scrollable {

				Content = new TableLayout {

					Padding = new Padding (50, 50, 50, 50),

					Rows = {
						new TableRow (new TableCell (new Label { Text = "" }, false), new TableCell (new Label {
							Text = "Analyzer Pedoporngraphy Images",
							TextAlignment = TextAlignment.Center,
							VerticalAlignment = VerticalAlignment.Center,
							TextColor = new Color (255 / 255, 255 / 255, 255 / 255, 1),
							Font = Fonts.Monospace (30f, 0, 0)
						}, false), new TableCell (new Label{ Text = "" }, false)),
						new TableRow (new TableCell (new Label{ Size = new Size (100, 100) })),
						new TableRow (new TableCell (new ImageView{ Image = pplogo, Size = new Size (300, 400) }, false), new TableCell (new Label{ Text = "" }, false), new TableCell (new ImageView {
							Image = univpmlogo,
							Size = new Size (300, 400)
						}, false)),
						new TableRow (new TableCell (new Panel{ Size = new Size (100, 100) })),
						new TableRow (new TableCell (new Label{ Text = "        " })),
						new TableRow (new TableCell (new Label { Text = "" }, false), new TableCell (start = new Button {
							Text = "Start !",
							Height = 60,
							Font = Fonts.Sans (25f, 0, 0),
							BackgroundColor = new Color (192 / 155, 192 / 155, 192 / 155, 0.6f),
							TextColor = new Color (0, 0, 0, 1)
						}, true), new TableCell (new Label { Text = "" }, false)),
						new TableRow (new TableCell (new Label (), false), new TableCell (new Label (), false), new TableCell (new Label (), false))
					}
				}
			};
				
			start.Click += (object sender, EventArgs e) =>     				 //Evento al click del bottone Start nella homepage del programma
			{
				
				tab.Pages.Add (tabinfo);

				tab.Pages.Add (tabanalysis);
			
				tab.Pages.Add (tabgallery);

				Content = tab;

				BackgroundColor = new Color (255/255, 255/255, 255/255, 1);
			};

			tab.SelectedIndexChanged += (object sender, EventArgs e) => 	 
			{
				if(tab.SelectedIndex == 2)									//Evento che accade quando da un diverso tab, entro nel tab Galleria...
				{
					tabgallery.ShowImage(MyTabGallery.index);				//...mostro l'immagine dato l'indice.
				}
			};

		}
	}
		
	class Program															//PUNTO D'INGRESSO DEL PROGRAMMA
	{

		[STAThread]
		static void Main(string[] args)
		{
			new Application().Run(new MyForm());
		}
	}
}
