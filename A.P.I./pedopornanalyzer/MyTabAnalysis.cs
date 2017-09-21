using System;
using Eto.Forms;
using Eto.Drawing;
using Eto.WinForms.Forms;
using Eto.Wpf.Forms;
using Eto.GtkSharp.Forms;

namespace pedopornanalyzer
{
	public class MyTabAnalysis:TabPage
	{
		public static TextBox positive;     //TextBox che dovrà contenere il numero di immagini positive
		public static TextBox negative;	 	//TextBox che dovrà contenere il numero di immagini negative
		public TextBox images;		 		//TextBox che dovrà contenere il numero di immagini analizzate

		public static TextArea pathPositive;//TextArea che dovrà contene i path delle immagini positive

		public ProgressBar progress; 		//ProgressBar che rappresenta l'avanzamento nell'elaborazione dei risultati delle immagini recuperate

		public Button PauseResume;			//Button che permette di stoppare/riprendere l'elaborazione dei risultati

		public Button cartella;				//Button che permette di scegliere il dispositivo da analizzare

		public Label percentuage;			//Label che indica la percentuale di avanzamento della progress bar

		Classifier classif;         		//"Prenoto" un oggetto della classe Classifier

		public ImageView loading;
	
		public MyTabAnalysis ()
		{
			Button vai;

			TextBox path;

			Text = "Analizza";

			Content = new Scrollable {
		
				Content = new TableLayout {
					BackgroundColor = new Color (0 / 255, 0 / 255, 255 / 255, 1),
					Padding = new Padding (10, 10, 10, 10),
					Spacing = new Size (40, 20),
					Rows = {
						new TableRow (new TableCell (path = new TextBox {
							Text = "Path del dispositivo...",
							TextColor = new Color (0, 0, 0, 1),
							Enabled = true,
							ReadOnly = true
						}, true), new TableCell (cartella = new Button {
							Text = "Scegli Cartella",
							BackgroundColor = new Color (1, 1, 1, 1)
						})),
						new TableRow (new TableCell (vai = new Button {
							Text = "Analizza la cartella selezionata",
							Enabled = false,
							BackgroundColor = new Color (1, 1, 1, 1)
						})),
						new TableRow (new TableCell (loading = new ImageView{ }, false)),
						new TableRow (new TableCell (new TableLayout {
							Spacing = new Size (40, 20),
							Padding = new Padding (1, 1, 1, 1),
							Rows = {
								new TableRow (new TableCell (new Label {
									Text = "N.Immagini analizzate",
									TextColor = new Color (255 / 255, 255 / 255, 255 / 255, 1),
									Font = Fonts.Sans (13f, 0, 0)
								}), new TableCell (images = new TextBox {
									Enabled = true,
									ReadOnly = true
								}, false)),
								new TableRow (new TableCell (new Label {
									Text = "N.Positivi",
									TextColor = new Color (255 / 255, 255 / 255, 255 / 255, 1),
									Font = Fonts.Sans (13f, 0, 0)
								}), new TableCell (positive = new TextBox {
									Enabled = true,
									ReadOnly = true
								}, false)),
								new TableRow (new TableCell (new Label {
									Text = "N.Negativi",
									TextColor = new Color (255 / 255, 255 / 255, 255 / 255, 1),
									Font = Fonts.Sans (13f, 0, 0)
								}), new TableCell (negative = new TextBox {
									Enabled = true,
									ReadOnly = true
								}, false)),
								new TableRow (new TableCell (new Label ())),
								new TableRow (new TableCell (progress = new ProgressBar { MinValue = 0 }, false), new TableCell (percentuage = new Label {
									Text = "",
									TextColor = new Color (1, 1, 1, 1),
									Font = Fonts.Sans (13f, 0, 0)
								}, false)),
								new TableRow (new TableCell (new Label ())),
								new TableRow (new TableCell (new Label {
									Text = "Path delle immagini positive:",
									TextColor = new Color (255 / 255, 255 / 255, 255 / 255, 1),
									Font = Fonts.Sans (13f, 0, 0)
								}), new TableCell (PauseResume = new Button {
									Text = "Pausa",
									BackgroundColor = new Color (1, 1, 1, 1),
									Enabled = false
								})),
								new TableRow (new TableCell (pathPositive = new TextArea { Enabled = true, ReadOnly = true }, true))
							}
						},
							false)),
					}

				}
			};

			cartella.Click += (sender, e) =>                  //Evento al click del bottone cartella
			{
				var dlg = new SelectFolderDialog();
				dlg.Title = "Seleziona il dispositivo da analizzare";
				DialogResult folder = dlg.ShowDialog(null);
				if(folder == DialogResult.Ok)
				{
					{
						string pathDevice = dlg.Directory.ToString();
						Classifier.DriveToClassify = pathDevice;
						path.Text =  pathDevice;
						vai.Enabled = true;
					}
				};
			};

			vai.Click += (sender, e) =>						//Evento al click del bottone vai
			{
				AzzeraCampi();								//Resetto quei campi che devono essere modificati dall'analisi che si appresta a fare
				vai.Enabled = false;
				classif = new Classifier();   			    //Creo un'instanza della classe Classifier
				classif.ClassifiersAsync(this);
				vai.Enabled = false;						//Disabilito il bottone vai per evitare che i task si impiastricciano nel caso si ripremi ancora "vai"
				cartella.Enabled = false;					//Disabilito il bottone cartella per evitare che i task si impiastricciano nel caso si ripremi scelta un altro dispositivo mentre è in corso l'analisi di quello attuale
			};

			PauseResume.Click += (sender, e) => 			//Evento al click del bottone PauseResume
			{			
				if(PauseResume.Text == "Pausa")				//Se ho premuto al "bottone Pausa"...
				{
					Classifier.isStop = true;				//...il flag di stop è true
					PauseResume.Text = "Riprendi";			//...diventa il "bottone Riprendi"
					cartella.Enabled = true;				//Se nel bottone c'è scritto "Riprendi" significa che sono in pausa e posso scegliere un nuovo dispositivo: abilito cartella
				}
				else{										//Se ho premuto al "bottone Riprendi"...
					Classifier.isStop = false;				//...il flag di stop è falso
					PauseResume.Text = "Pausa";				//...diventa il "bottone Pausa"
					classif.AnalyzerAsync(this);            //...l'analisi riprende
					cartella.Enabled = false; 				//Se nel bottone c'è scritto "Pausa" significa che l'analisi è in corso e non si deve scegliere un nuovo dispositivo: disabilito cartella
				}
			};
		}


			
		private void AzzeraCampi(){

			positive.Text = "";
			negative.Text = "";
			images.Text = "";
			pathPositive.Text = "";
			progress.Value = 0;
			Classifier.isStop = false;
			PauseResume.Enabled = false;
			PauseResume.Text = "Pausa";
			percentuage.Text = "";
			MyTabGallery.ResetDefaultGallery ();
			MyTabGallery.check = false;
		}
	}
}

