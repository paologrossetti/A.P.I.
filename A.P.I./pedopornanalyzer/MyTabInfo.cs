using System;
using Eto.Forms;
using Eto.Drawing;
using Eto.WinForms.Forms;
using Eto.Wpf.Forms;
using Eto.GtkSharp.Forms;

namespace pedopornanalyzer
{
	public class MyTabInfo:TabPage
	{
		public MyTabInfo ()
		{
			Text = "Read Me";

			Content = new Scrollable 
			{ 
				Content = new TableLayout {

					BackgroundColor = new Color (0/255, 0/255, 255/255, 1),
					Padding = new Padding (10, 10, 10, 10),
					Spacing = new Size (40, 20),
					Rows = {
						new TableRow (new TableCell (new Label {
							Text = "Analyzer Pedoporngraphy Images",
							Font = Fonts.Monospace (24f, 0, 0),
							TextColor = new Color(255/255,255/255,255/255,1)
						}, false)),
						new TableRow (new TableCell (new Label {
							Text = "API è un programma che permette di estrarre da un dispositivo rimovibile possibili immagini ritenute pedopornografiche.\n" +
							"I dispositivi supportati all'analisi sono tutti quelli dotati di una memoria di massa che possono essere collegati tramite interfaccia USB.\n" +
							"Le immagini recuperate possono avere esclusivamente solo uno di questi formati: .jpg, .bmp, .png.\n" +
							"Non si analizzano immagini troppo piccole (<18Kb) o troppo grandi (>30Mb).",
							Font = Fonts.Monospace(12f,0,0),
							TextColor = new Color(255/255,255/255,255/255,1)
						}, false)),
						new TableRow (new TableCell (new Label {
							Text = "Analizza",
							Font = Fonts.Monospace (24f, 0, 0),
							TextColor = new Color(255/255,255/255,255/255,1)
						}, false)),
						new TableRow (new TableCell (new Label {
							Text = "La scheda Analizza contiene le principali funzionalità per cui è stata pensata questa applicazione:\n" +
							"dato un dispositivo, il programma analizzerà tutte le cartelle e sottocartelle per estrarre immagini ritenute positive ovvero pedopornografiche.\n\n" +
							"Da questa scheda sarà possibile scegliere per l'analisi il classificatore standard oppure un classificatore " +
							"addestrato al momento che imparerà a riconoscere immagini simili a quelle fornitegli come esempio.\n\n" +
							"La maggior parte delle periferiche collegate tramite interfaccia USB,nel sistema operativo Ubuntu, si trovano in /media/user/ in cui si indica con  \"/\"  la directory radice.\n" +
							"Fanno parte di questa categoria dispositivi quali flashdrive USB, Hard Disk esterni.\n\n" +
							"Alcune periferiche collegabili tramite interfaccia USB possono non trovarsi in /media/user in caso di particolari protocolli utilizzati dalla periferica.\n" +
							"Fanno parte di questa categoria dispositivi quali smartphone e tablet con sistema operativo Android in versione 4.0 o superiore.",
							Font = Fonts.Monospace(12f,0,0),
							TextColor = new Color(255/255,255/255,255/255,1)
						}, false)),
						new TableRow (new TableCell (new Label {
							Text = "Galleria",
							Font = Fonts.Monospace (24f, 0, 0),
							TextColor = new Color(255/255,255/255,255/255,1)
						}, false)),
						new TableRow (new TableCell (new Label {
							Text = "La scheda Galleria contiene un semplice visualizzatore delle probabili immagini ritenute pedopornografiche.\n\n" +
							"Bisogna specificare che la classificazione di immagini pedopornografiche in realtà consiste nella ricerca di immagini pornografiche:\n" +
							"il controllo delle immagini in modo manuale attraverso la galleria risulta fondamentale per poter discriminare effettivamente i possibili contenuti illeciti da quelli leciti.\n\n" +
							"---> visualizzazione dell'immagine nelle sue dimensioni originali facendo doppio-click sull'immagine stessa.\n\n" +
							"---> possibilità di visionare le immagini ritenute positive in ordine casuale.\n\n"+
							"---> possibilità di scartare eventuali falsi positivi.\n\n"+
							"---> produzionde del verbale di analisi del dispositivo in formato PDF con la lista delle immagini positive.\n" +
							"     Per ogni immagine positiva: path e hashing doppio(SHA1 e MD5) come verifica di integrità.",
							Font = Fonts.Monospace(12f,0,0),
							TextColor = new Color(255/255,255/255,255/255,1)
						}, false)),
						new TableRow (new TableCell (new Label {
							Text = "Nota",
							Font = Fonts.Monospace (24f, 0, 0),
							TextColor = new Color(255/255,255/255,255/255,1)
						}, false)),
						new TableRow (new TableCell (new Label {
							Text = "Nel caso in cui il dispositivo da analizzare abbia una struttura molto complessa e/o contenga numerosissimi file,\n" +
									"il programma potrebbe avere bisogno di alcuni secondi prima di mostrarne i risultati:\n" +
									"in tal caso, si prega di attendere.",
							Font = Fonts.Monospace(12f,0,0),
							TextColor = new Color(255/255,255/255,255/255,1)
						}, false)),
					}
				}
			};
		}
	}
}