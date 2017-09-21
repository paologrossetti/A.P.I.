using System;
using System.IO;
using System.Globalization;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Eto.Forms;
using Eto.Drawing;
using Eto.WinForms.Forms;
using Eto.Wpf.Forms;
using Eto.GtkSharp.Forms;

using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace pedopornanalyzer
{
	public class MyTabGallery:TabPage
	{
		public static int index = 1;  					//Indice utilizzato per recuperare il path delle immagini positive di Classifier.PositiveImages. NB:E' inizializzato ad 1!

		public static ImageView gallery;

		public static TextBox numberimage;
		public static TextBox pathimage;

		public static Bitmap defaultimage = new Bitmap( //immagine di default che viene visualizzata nel tab Galleria
			System.Reflection.Assembly.GetEntryAssembly().
			GetManifestResourceStream("pedopornanalyzer.Properties.Resources.noimage2.png"));

		public static bool check = false; 				// variabile utilizzata nella rimozione di un immagine: è responsabile di mostrare/nascondere il popup di warning. True -> :(l'ultima volta) si è spuntato "Non mostrare più"; False -> :(l'ultima volta) non si è spuntato

		public MyTabGallery ()
		{
			Text = "Galleria";

			Button first;
			Button next;
			Button previous;
			Button last;
			Button createpdf;
			Button casualimage;
			Button goimagenumb;
			Button removeimage;

			Content = new Scrollable {
				Content = new TableLayout { 
					BackgroundColor = new Color (0 / 255, 0 / 255, 255 / 255, 1),
					Padding = new Padding (10, 10, 10, 10),
					Spacing = new Size (40, 20),
					Rows = {
						new TableRow (new TableCell (new Label{ Text = "      " }), new TableCell (new Label{ Text = "     " }), new TableCell (new Label{ Text = "     " })),
						new TableRow (new TableCell (new Label {
							Text = "Immagine",
							TextAlignment = TextAlignment.Right,
							TextColor = new Color (255 / 255, 255 / 255, 255 / 255, 1),
							Font = Fonts.Monospace (20f, 0, 0)
						}, false), new TableCell (pathimage = new TextBox {
							Enabled = true,
							ReadOnly = true
						}, false), new TableCell (numberimage = new TextBox {
							Enabled = true,
							ReadOnly = true
						}, false)),
						new TableRow (new TableCell (new Label{ Text = "" }, false), new TableCell (gallery = new ImageView {
							Image = defaultimage,
							Size = new Size (550, 550),
							Cursor = Cursors.Pointer,
							ToolTip = "Doppio click per visualizzare l'immagine nelle dimensioni originali."
						}), new TableCell (new Label{ Text = "" }, false)),
						new TableRow (new TableCell (new Label{ Text = "" }, false), new TableCell (removeimage = new Button {
							Text = "X Escludi l'immagine corrente da quelle positive",
							BackgroundColor = new Color (192 / 155, 192 / 155, 192 / 155, 0.6f),
							TextColor = new Color (0, 0, 0, 1)
						}), new TableCell (new Label{ Text = "" }, false)),  
						new TableRow (new TableCell (goimagenumb = new Button {
							Text = "Vai all'immagine numero...",
							BackgroundColor = new Color (192 / 155, 192 / 155, 192 / 155, 0.6f),
							TextColor = new Color (0, 0, 0, 1)
						}, false), new TableCell (new Label{ Text = "" }), new TableCell (casualimage = new Button {
							Text = "Visualizzazione casuale",
							ToolTip = "Clicca per visualizzare le immagini in ordine casuale",
							BackgroundColor = new Color (192 / 155, 192 / 155, 192 / 155, 0.6f),
							TextColor = new Color (0, 0, 0, 1)
						}, false)),
						new TableRow (new TableCell (first = new Button {
							Text = "|<- First",
							BackgroundColor = new Color (192 / 155, 192 / 155, 192 / 155, 0.6f),
							TextColor = new Color (0, 0, 0, 1)
						}, false), new TableCell (new Label{ Text = "" }), new TableCell (last = new Button {
							Text = "->| Last",
							BackgroundColor = new Color (192 / 155, 192 / 155, 192 / 155, 0.6f),
							TextColor = new Color (0, 0, 0, 1)
						}, false)),
						new TableRow (new TableCell (previous = new Button {
							Text = "< Previous",
							BackgroundColor = new Color (192 / 155, 192 / 155, 192 / 155, 0.6f),
							TextColor = new Color (0, 0, 0, 1)
						}, true), new TableCell (new Label{ Text = "     " }, true), new TableCell (next = new Button {
							Text = "> Next",
							BackgroundColor = new Color (192 / 155, 192 / 155, 192 / 155, 0.6f),
							TextColor = new Color (0, 0, 0, 1)
						}, true)),
						new TableRow (new TableCell (new Label{ Text = "" }), new TableCell (createpdf = new Button {
							Text = "Produci verbale in formato PDF",
							BackgroundColor = new Color (192 / 155, 192 / 155, 192 / 155, 0.6f),
							TextColor = new Color (0, 0, 0, 1)
						}), new TableCell (new Label{ Text = "" })),
						new TableRow (new TableCell (new Label{ Text = "" }), new TableCell (new Label{ Text = "" }), new TableCell (new Label{ Text = "" })),
					}

				}
			};	

			
					

			first.Click += (object sender, EventArgs e) =>            //Evento al click del bottone First
			{
				ResetElementsGallery();
				ShowImage(1);										  //Visualizzerò l'immagine in testa alla coda
			};
				
			previous.Click += (object sender, EventArgs e) => 		//Evento al click del bottone Previous
			{
				if(Classifier.PositiveImages != null)
				{
					if(index==1){									//Se clicco "indietro" mentre visualizzo l'immagine di testa della coda...
						ResetElementsGallery();
						ShowImage(Classifier.PositiveImages.Count);	//...Visualizzerò l'immagine di coda
					}
					else
					{
						ResetElementsGallery();;
						ShowImage(index-1);
					}
				}
			};

			next.Click += (object sender, EventArgs e) => 			//Evento al click del bottone Next
			{
				GoToNextImage();
			};

			last.Click += (object sender, EventArgs e) => 			//Evento al click del bottone Last
			{
				if(Classifier.PositiveImages != null)
				{
					ResetElementsGallery();
					ShowImage(Classifier.PositiveImages.Count);
				}

			};

			createpdf.Click += (object sender, EventArgs e) =>  	//Evento al click del bottone ProduciPDF
			{
				if(Classifier.PositiveImages != null && Classifier.isStop == false){           //Se PositiveImages è stato creato e se il classificatore non è stato stoppato mostro l'allert
					/*DialogResult allert = MessageBox.Show("Prima di procedere alla creazione del verbale è necessario mettere in pausa l'analizzatore:\n"+
						"potrebbe esserci ulteriori falsi positivi da escludere.\n\n"+
						"Ricontrollare le immagini che si vogliono contestare.","Attenzione!",MessageBoxButtons.OK);*/
					Dialog warning = new Dialog{Title = "Attenzione!",ClientSize = new Size(450,180),BackgroundColor = new Color(0,0,1,1)};  	//Mostro un popup(dialog) per contenere textbox e bottone
					Button okbutton;
					warning.Content = new TableLayout { Rows = 
						{
							new TableRow (new TableCell( new Label())),
							new TableRow ( new TableCell(new Label{ Text = "Prima di procedere alla creazione del verbale è necessario mettere in pausa l'analizzatore:\n"+
									"potrebbe esserci ulteriori falsi positivi da escludere.\n\n"+
									"Ricontrollare le immagini che si vogliono contestare.",TextColor = new Color(255/255,255/255,255/255,1),TextAlignment = TextAlignment.Center})),
							new TableRow (new TableCell( new Label())),
							new TableRow (new TableCell( okbutton = new Button{Text = "Ok",TextColor = new Color(0,0,0,1),BackgroundColor = new Color(192/155,192/155,192/155,0.6f)})),
							new TableRow (new TableCell( new Label()))},
					};
					warning.ShowModalAsync();

					okbutton.Click += (object sender2, EventArgs e2) =>     		//Evento al click del bottone OK: rimuovo l'immagine e chiudo la finestra di warning
					{
						warning.Close();
					};
				}
				else CreateVerbalePdfAsync();
			};

			goimagenumb.Click += (object sender, EventArgs e) =>      	//Evento al click del bottone Vai all'immagine numero...
			{
				Dialog a = new Dialog{Title = "Inserisci il numero dell'immagine positiva che vuoi visualizzare:",ClientSize = new Size(520,40),BackgroundColor = new Color(0,0,1,1)};  	//Mostro un popup(dialog) per contenere textbox e bottone
				TextBox textbox;   		 	//Textbox in cui si dovrà inserire il numero dell'immagine da visualizzare
				Button gotoimage;	   		//Bottone a cui è associato l'azione di mostrare il numero di immagine scelta
				a.Content = new TableLayout { Rows = 
					{
						new TableRow ( new TableCell(textbox = new TextBox()),new TableCell(gotoimage = new Button{Text = "Visualizza Immagine",TextColor = new Color(0,0,0,1),BackgroundColor = new Color(192/155,192/155,192/155,0.6f)}))},
				};
				gotoimage.Click += (object send, EventArgs ee) => { //Evento al click del bottone gotoimage
					int numb;
					if((Classifier.PositiveImages != null) && (int.TryParse(textbox.Text,out numb)) && (numb>1 && numb<=Classifier.PositiveImages.Count)){ // Se Classifier.PositiveImages non è nullo && Se il numero inserito è int && Se il numero inserito è compreso nel range del numero delle immagini positive...
						index = numb;
						ShowImage(index);
					}
					a.Close();
				};					
				a.ShowModalAsync();
			};

			casualimage.Click += (object sender, EventArgs e) =>     	//Evento al click del bottone Visualizzazione casuale
			{
				if(Classifier.PositiveImages != null)
				{
					Random r = new Random();
					int rInt = r.Next(1,Classifier.PositiveImages.Count+1);
					index = rInt;
					ShowImage(index);
				}
			};

			gallery.MouseDoubleClick += (object sender, MouseEventArgs e) =>  //Evento al doppioclick sull'immagine da visualizzare
			{
				if((gallery.Size.Height != gallery.Image.Size.Height) && ((gallery.Size.Width) != gallery.Image.Size.Width)) //Se l'immagine visualizzata  è nelle dimensioni originali...
				{
				gallery.Size = gallery.Image.Size;																			//Mostro l'immagine nelle dimensioni originali
					gallery.ToolTip = "Doppio click per tornare alla visualizzazione standard";
				}
				else 
				{
					ResetElementsGallery();																					//...Altrimenti resetto le dimensioni dell'immagine	
				}
			};

			removeimage.Click += (object sender, EventArgs e) =>      //Evento al click del bottone escludi immagine
			{
				//Costruisco la finestra personalizzata di warning di eliminazione dell'immagine da quelle considerate positive 
				Dialog a = new Dialog{Title = "Warning",ClientSize = new Size(450,180),BackgroundColor = new Color(0,0,1,1)};  	//Mostro un popup(dialog) per contenere textbox e bottone
				Button okbutton;	 //bottone OK
				Button cancelbutton; //bottone Annulla
				CheckBox noshowagain;//checkbox Non mostrare più
				a.Content = new TableLayout { Rows = 
					{
						new TableRow (new TableCell( new Label())),
						new TableRow ( new TableCell(new Label{ Text = "Sei sicuro di voler escludere l'immagine corrente ? ",TextColor = new Color(255/255,255/255,255/255,1),TextAlignment = TextAlignment.Center})),
						new TableRow (new TableCell( new Label())),
						new TableRow (new TableCell( okbutton = new Button{Text = "Ok",TextColor = new Color(0,0,0,1),BackgroundColor = new Color(192/155,192/155,192/155,0.6f)})),
						new TableRow (new TableCell( cancelbutton = new Button{Text = "Annulla",TextColor = new Color(0,0,0,1),BackgroundColor = new Color(192/155,192/155,192/155,0.6f)})),
						new TableRow (new TableCell( new Label())),
						new TableRow (new TableCell( noshowagain = new CheckBox{Text = "Non mostrare più",TextColor = new Color(255/255,255/255,255/255,1)}) )},
				};

				okbutton.Click += (object sender2, EventArgs e2) =>     		//Evento al click del bottone OK: rimuovo l'immagine e chiudo la finestra di warning
				{
					
					RemoveImageFromPositiveImages();
					a.Close();

				};

				cancelbutton.Click += (object sender3, EventArgs e3) => 		//Evento al click del bottone Annulla: resetto a falso il booleano e chiudo la finestra di warning
				{
					check = false;
					a.Close();
				};

				noshowagain.CheckedChanged += (object sender4, EventArgs e4) => //Evento al cambiamento della spunta di checkbox: modifica la variabile booleana
				{
					if(!check) check = true;
					else check = false;
				};
				//fine costruzione finestra warning con i relativi eventi

				if(check!=true){ 						//Se "dall'ultima volta" la checkbox non è stata spuntata...
					a.ShowModalAsync();					//...Apro la finestra di warning con i relativi eventi
				}
				else{									//Altrimenti non mostro la finestra di warning e rimuovo direttamente l'immagine
					RemoveImageFromPositiveImages();    
				}												
			};
		}
			
		public void ShowImage(int pos){     					//Funzione che permette di mostrare l'immagine.Prendo come parametro l'indice che sarà utilizzato per recuperare il path dell'immagine da visualizzare			

			if (Classifier.PositiveImages != null && Classifier.PositiveImages.Count > 0) { //Condizione necessaria: l'analisi deve essere già stata fatta o è in corso(cioè Classifier!=null) e devo avere immagini da visualizzare(Positives.Count!=0)
					String imageName = Classifier.PositiveImages [pos-1]; 					//***NB : il pos-esimo elemento ha posizione pos-1 nell'arraylist ! ***
					Bitmap immagine = new Bitmap (imageName);
					gallery.Image = immagine;
					index = pos;
					numberimage.Text = pos.ToString ();
					pathimage.Text = imageName;
			}
		}

		private void ResetElementsGallery(){					//Funzione che permette di riportare la dimensione della galleria in quella di default
			gallery.Size = new Size(550,550);
			gallery.ToolTip = "Doppio click per visualizzare l'immagine nelle dimensioni originali.";
		}

		private void GoToNextImage(){							//Funzione che permette di mostrare l'immagine successiva
			if(Classifier.PositiveImages != null)
			{
				if(index==Classifier.PositiveImages.Count)   	//Se sono arrivato alla fine delle immagini...
				{
					ResetElementsGallery();
					ShowImage(1);                          		//Ti mostro la prima
				}
				else
				{
					ResetElementsGallery();
					ShowImage(index+1);							//Altrimenti ti mostro la successiva
				}
			}
		}

		private void RemoveImageFromPositiveImages(){		   //Funzione che si attiva quando si vuole escludere l'immagine corrente da quelle considerate positive
			if(Classifier.PositiveImages == null || Classifier.PositiveImages.Count == 0) //Caso base: non ho ancora attivato l'analizzatore o non ho trovato immagini positive...
			{
				ResetDefaultGallery();													  //...riporto la galleria nelle condizioni di default.
			}
			else if(Classifier.PositiveImages.Count == 1)								  // Caso in cui voglio escludere l'unica immagine positive...
			{
				RemoveImage();															  //Eseguo le operazioni comuni e..
				ResetDefaultGallery();													  //...riporto la galleria nelle condizioni di default.
			}
			else                                                                          //Caso generale: voglio scartare una immagine di un gruppo di immagini
			{
				RemoveImage();															  //Eseguo le operazioni comuni e..
				index--;																  //...decremento il valore di index dato che ho appena scartato un'immagine
				GoToNextImage();														  //..mostro l'immagine successiva a quella scartata
			}
		}

		private void RemoveImage(){													  		//Funzione che,nell'operazione di esclusione di un'immagine,svolge le operazioni comuni indipendentemente dal caso base/generale.
			int IndiceElementoDaEliminare = index - 1;								  		//L'indice dell'elemento da eliminare nella lista è "uno di meno" rispetto ad index (l'indice della lista parte da 0 mentre index parte da 1)
			string pathdaeliminare = Classifier.PositiveImages[IndiceElementoDaEliminare]; 	//Recupero la stringa che rappresenta il path da scartare nella textarea del MYTabAnalysis
			Classifier.PositiveImages.RemoveAt(IndiceElementoDaEliminare);            		//Rimuovo l'elemento i-esimo dalla lista e la riordino
			Classifier.Positives--;															//Decremento il numero di Classifier.Positives
			Classifier.Negatives++;															//Incremento il numero di Classifier.Negatives
			MyTabAnalysis.positive.Text = Classifier.Positives.ToString ();					//Aggiorno il numero de positivi in MyTabAnalysis
			MyTabAnalysis.negative.Text = Classifier.Negatives.ToString ();					//Aggiorno il numero de positivi in MyTabAnalysis
			MyTabAnalysis.pathPositive.Text = MyTabAnalysis.pathPositive.Text.Replace (pathdaeliminare, "- - - - - - - - - - D E L E T E D - - - - - - - - - - "); // Nella textarea di MyTabAnalysis, sostituisco il path da eliminare con la scritta "Deleted";
		}

		private string GenerateSHA1(string path){ 											//Funzione che genera lo SHA1 dell'immagine passata come parametro

			using (FileStream stream = File.OpenRead(path))
			{
				using (SHA1Managed sha = new SHA1Managed())
				{
					byte[] checksum = sha.ComputeHash(stream);								//un'immagine non è altro che un insieme di bit ( e quindi insieme di byte)
					string sendCheckSum = BitConverter.ToString(checksum).Replace("-", string.Empty);
					return sendCheckSum;
				}
			}
		}

		private string GenerateMD5(string path){											//Funzione che genera MD5 dell'immagine passata come parametro

			using (var md5 = MD5.Create())  
			{
				using (var stream = File.OpenRead(path))
				{
					return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-",string.Empty).ToLower();
				}
			}
		}

		private async void CreateVerbalePdfAsync(){                                          //Funzione che avvia l'iter per produrre il verbale in formato pdf
			var dlg = new SelectFolderDialog();
			dlg.Title = "Indica dove vuoi salvare il verbale";
			DialogResult folder = dlg.ShowDialog(null);
			if(folder == DialogResult.Ok)
			{
				{
					Document doc = new Document(iTextSharp.text.PageSize.LETTER,10,10,42,35);
					string date_time = DateTime.Now.ToString("yyyyMMdd_HH:mm:ss");
					PdfWriter verbale = PdfWriter.GetInstance(doc,new FileStream(date_time+".pdf",FileMode.Create));
					doc.Open();
					iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("pedopornanalyzer.Properties.Resources.polizia.png"));
					logo.ScaleAbsolute(160f,160f);
					logo.Alignment = 3;
					doc.Add(logo);
					Paragraph par1 = new Paragraph("POLIZIA POSTALE\n\n");
					par1.Alignment = Element.ALIGN_CENTER;
					par1.Font = FontFactory.GetFont("Courier", 30f, BaseColor.BLACK);
					doc.Add (par1);

					Paragraph par2 = new Paragraph ("Alle ore " + DateTime.Now.ToString ("HH:mm:ss") + " del giorno " + DateTime.Now.ToString ("dd/MM/yyyy") + " è stato analizzato il seguente dispositivo:\n"
						+ Classifier.DriveToClassify + "\n\n" +
						"Si contestano le seguenti " + Classifier.Positives + " immagini pedopornografiche:\n");
					par2.Font = FontFactory.GetFont ("Courier", 15f, BaseColor.BLACK);
					doc.Add (par2);

					if(Classifier.PositiveImages != null){

						string pathSha1Md5 = "";

						await Task.Run (() => {

							Dialog waiting = new Dialog{Title = "",Size = new Eto.Drawing.Size (750, 128),Content = new TableLayout{Rows = {new TableRow(new TableCell(new ImageView{Image = new Eto.Drawing.Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("pedopornanalyzer.Properties.Resources.warning.png"))}),new TableCell(new Label{Text = "Attendere prego: creazione del verbale in corso...",VerticalAlignment = VerticalAlignment.Center,TextAlignment = TextAlignment.Center,TextColor = new Eto.Drawing.Color(1,1,1,1),BackgroundColor = new Eto.Drawing.Color (0, 0,1,1),Font = Fonts.Monospace(15f,0,0)}))}}};

							waiting.ShowModalAsync ();

							pathSha1Md5 = StringPathSha1Md5();

							Paragraph par3 = new Paragraph(pathSha1Md5);

							doc.Add(par3);

							waiting.Close();

						});
					}
					doc.Close();
					MessageBox.Show("Verbale prodotto con successo !\nIl nome del verbale è dato dalla ora e data di creazione.",0);
				}
			};
		}

		private string StringPathSha1Md5(){
			
			string pathmd5sha1 = "";
			int i = 1;
			foreach(string str in Classifier.PositiveImages){
				pathmd5sha1 += i+"): "+str+"\n"+"SHA1: "+GenerateSHA1(str)+"\n"+"MD5: "+GenerateMD5(str)+"\n\n";
				i++;
			}
			return pathmd5sha1;
		}

		public static void ResetDefaultGallery(){
			index = 1;
			numberimage.Text = "";
			pathimage.Text = "";
			gallery.Image = defaultimage;
		}
	}		
}