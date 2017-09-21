using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Emgu.CV;
using Emgu.CV.ML;
using Emgu.CV.ML.MlEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using Eto.Forms;
using Eto.Drawing;

namespace pedopornanalyzer
{
	public class Classifier
	{
		public static String xmlFileName = "porn_classifier.xml"; 								//nome del classificatore NB:E'statico!
		public static string DriveToClassify = null;             								//variabile che conterra il path del dispositivo da analizzare NB:E' statico!
		public static string PathClassifierStandard = Path.GetFullPath("porn_classifier.xml");	//rappresenta il path completo del classificatore NB:E'statico!
		public static string[] fileImages;														//array che contiene i path delle immagini da analizzare NB:E'statico!
		public static List<string> PositiveImages; 												//Arraylist che contiene i path delle immagini positive NB:E' statico!
		public static string ClassifierUsedForAnalysis; 										//Rappresenta il path del classificatore usato per l'analisi corrente


		public static int ImageAnalyzed; 														//rappresenta il numero di immagini analizzate fino a questo momento NB:E'statico!
		public static int Positives;															//rappresenta il numero di immagini positive NB:E'statico!
		public static int Negatives;															//rappresenta il numero di immagini negative NB:E'statico!
		public static int populationSize;														//rappresenta il numero di immagina da analizzare NB:E'statico!

		public static bool isStop = false;														//rappresenta il flag che viene settato quando si mette in pausa l'analizzatore NB:E'statico!
		public static bool existClassifier = true;							 					//rappresenta il flag che indica se esiste il classificatore per l'analisi corrente NB:E'statico!

		public async void ClassifiersAsync(MyTabAnalysis mytab2)
		{
			bool cyclebreak = false;

			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

			//Dialog in cui si permette di scegliere il calssificatore all'utente 
			Dialog choose = new Dialog{Title = "Scegli il Classificatore",ClientSize = new Size(450,180),BackgroundColor = new Color(0,0,1,1)};
			Button defaultclassifier;	
			Button newclassifier; 
			choose.Content = new TableLayout { Rows = 
				{
					new TableRow (new TableCell( new Label())),
					new TableRow ( new TableCell(new Label{ Text = "Quale classificatore vuoi utilizzare per la classificazione delle immagini ? ",TextColor = new Color(255/255,255/255,255/255,1),TextAlignment = TextAlignment.Center})),
					new TableRow (new TableCell( new Label())),
					new TableRow (new TableCell( defaultclassifier = new Button{Text = "Voglio utilizzare il classificatore di default",TextColor = new Color(0,0,0,1),BackgroundColor = new Color(192/155,192/155,192/155,0.6f)})),
					new TableRow (new TableCell( new Label())),
					new TableRow (new TableCell( newclassifier = new Button{Text = "Voglio addestrare e creare un nuovo classificatore ",TextColor = new Color(0,0,0,1),BackgroundColor = new Color(192/155,192/155,192/155,0.6f)})),
					new TableRow (new TableCell( new Label()))},
			};

			defaultclassifier.Click += (object sender, EventArgs e) => 	//Caso in cui si utilizzi il classificatore standard
			{
				existClassifier = true;									//Esiste il classificatore per l'analisi corrente
				ClassifierUsedForAnalysis = PathClassifierStandard;		//Il classificatore usato è quello standard
				choose.Close();
			};

			newclassifier.Click += (object sender, EventArgs e) => 		//Caso in cui si vuole creare classificatore
			{
				existClassifier = false;								//Non esiste ancora il classicatore che si vuole usare..occorre crearlo
				choose.Close();
			};

			choose.ShowModal();


			while (!cyclebreak)
			{
				if (!existClassifier)   //Caso in cui il classificatore voglio creare un nuovo classificatore
				{
					//Dialog che spiega come addestrare il classificatore
					Dialog attention = new Dialog{ Title = "Attenzione!", Size = new Eto.Drawing.Size (888, 450) };
					Button okbutton;
					attention.Content = new TableLayout{Rows =
							{
								new TableRow(new TableCell(new ImageView{Image = new Eto.Drawing.Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("pedopornanalyzer.Properties.Resources.warning.png"))}),new TableCell(new Label{Text = "Attenzione:si è scelto di creare un nuovo classificatore!\n\n" +
									"E' possibile addestrare il classificatore fornendogli degli esempi,da cui imparerà a riconoscere immagini simili.\n\n" +
									"Per addestrare il classificatore si dovrà:\n" +
										"- selezionare una cartella che contiene esclusivamente immagini positive (pornografiche/pedopornografiche) in formato .jpg\n" +
										"- selezionare una cartella che contiene esclusivamente immagini negative in formato .jpg\n" +
										"  Le due cartelle devono contenere,all'incirca, lo stesso numero di file.\n\n" +
										"Nota: Non rispondiamo dei risultati prodotti dal nuovo classificatore, se non \n" +
										"si sa come addestrarlo utilizzare quello standard.\n",VerticalAlignment = VerticalAlignment.Center,TextAlignment = TextAlignment.Center,TextColor = new Eto.Drawing.Color(1,1,1,1),BackgroundColor = new Eto.Drawing.Color (0, 0,1,1),Font = Fonts.Monospace(15f,0,0)})),
								new TableRow(new TableCell(),new TableCell(okbutton = new Button{Text = "Ho capito !"}),new TableCell())}
					};

					okbutton.Click += (object sender, EventArgs e) => 
					{
						attention.Close();
					};

					attention.ShowModal();

					/* Training Phase */
					Console.WriteLine("Classifier not detected.");
					Console.WriteLine("Training new classifier from samples...");
					string trainFolderPositive = "";
					bool presscancel = true;
					do{                                                                  //do-while per "costringere" l'utente a scegliere una cartella
						var dlg = new SelectFolderDialog();
						dlg.Title = "Seleziona la cartella che contiene file positive";
						DialogResult folder = dlg.ShowDialog(null);
						if (folder == DialogResult.Ok) {
							presscancel = false;
							string pathDevice = dlg.Directory.ToString ();
							trainFolderPositive = pathDevice;
						} else {
							presscancel = true;
							MessageBox.Show ("Per favore,seleziona una cartella che contiene esclusivamente file positivi in formato .jpg", 0);
						}
					}while (presscancel);
	
					List<string> files = Directory.EnumerateFiles(trainFolderPositive, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".jpg") || s.EndsWith(".JPG") || s.EndsWith(".jpeg") || s.EndsWith(".JPEG")).ToList();
					int numbpositiveimages = files.Count ();
					MessageBox.Show("Cartella selezionata correttamente",0);


					string trainFolderNegative = "";
					presscancel = true;
					do{																	//do-while per "costringere" l'utente a scegliere una cartella
						var dlg2 = new SelectFolderDialog();
						dlg2.Title = "Seleziona la cartella che contiene file negativi";
						DialogResult folder2 = dlg2.ShowDialog(null);
						if (folder2 == DialogResult.Ok) {
							presscancel = false;
							string pathDevice = dlg2.Directory.ToString ();
							trainFolderNegative = pathDevice;
						} else {
							presscancel = true;
							MessageBox.Show ("Per favore,seleziona una cartella che contiene esclusivamente file negativi in formato .jpg", 0);
						}
					}while (presscancel);
						
					files.AddRange(Directory.EnumerateFiles(trainFolderNegative, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".jpg") || s.EndsWith(".JPG") || s.EndsWith(".jpeg") || s.EndsWith(".JPEG")).ToList());
					populationSize = files.Count ();
					int numbnegativeimages = populationSize - numbpositiveimages;
					MessageBox.Show("Cartella selezionata correttamente",0);

					int[] labels = new int[populationSize];
					Matrix<float> trainingMat = new Matrix<float>(populationSize, 10);

					Dialog waiting = new Dialog{Title = "Addestramento in corso",Size = new Eto.Drawing.Size (750, 128),Content = new TableLayout{Rows = {new TableRow(new TableCell(new ImageView{Image = new Eto.Drawing.Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("pedopornanalyzer.Properties.Resources.warning.png"))}),new TableCell(new Label{Text = "Classificatore non individuato.\n" +
							"Addestramento del classificatore dagli esempi in corso...\n",VerticalAlignment = VerticalAlignment.Center,TextAlignment = TextAlignment.Center,TextColor = new Eto.Drawing.Color(1,1,1,1),BackgroundColor = new Eto.Drawing.Color (0, 0,1,1),Font = Fonts.Monospace(15f,0,0)}))}}};
					waiting.ShowModalAsync (); // il dialog di avviso viene mostrato in modo asincrono rispetto all'altra interfaccia

					try
					{
						using (SVM svm = new SVM())
						{
							svm.Type = SVM.SvmType.CSvc;
							svm.SetKernel(SVM.SvmKernelType.Linear);
							svm.TermCriteria = new MCvTermCriteria((int)1e6, 1e-6);
							await Task.Run(() =>
								{

									for (int i = 0; i < populationSize; i++)
									{
										SkinDetector mySkinDetector = new SkinDetector();

										Matrix<float> samples = mySkinDetector.getFeatures(files[i]);

										for(int j = 0; j < samples.Cols; j++)
										{
											trainingMat[i, j] = samples[0, j];
										}
										if (i<numbpositiveimages) labels[i] = 1;
										else labels[i] = 0;

									}
								});

							Matrix<int> labelsMat = new Matrix<int>(labels);

							TrainData td = new TrainData(trainingMat, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, labelsMat);

							if (svm.TrainAuto(td))
							{
								waiting.Close();

								SaveFileDialog SaveClassifierDialog = new SaveFileDialog{Title="Indica il nome del classificatore"};

								SaveClassifierDialog.Filters.Add(new FileDialogFilter("XML Document", ".xml"));
								SaveClassifierDialog.CurrentFilter = SaveClassifierDialog.Filters[0];
								SaveClassifierDialog.CurrentFilterIndex = 0;

								bool ExistSameClassifier = true;

								while(ExistSameClassifier){

									if (SaveClassifierDialog.ShowDialog(null) == DialogResult.Ok)
									{
										if(File.Exists(SaveClassifierDialog.FileName+".xml")) 	
										{										
											ExistSameClassifier = true; 
											MessageBox.Show("Esiste già un classificatore con questo nome",0);
										}
										else
										{
											ExistSameClassifier = false;
											string savefile = SaveClassifierDialog.FileName+".xml";
											svm.Save(savefile);
											ClassifierUsedForAnalysis = savefile;
										}
									}
								}
								MessageBox.Show("Addestramento completato. Il classificatore è pronto!",0);
							}
							else{
								MessageBox.Show("Ooops !\nQualcosa è andato storto,verrà utilizzato il classificatore standard");
								ClassifierUsedForAnalysis = PathClassifierStandard;
							}
						}
					}
					catch (Exception e)
					{ Console.WriteLine(e.ToString()); }
					existClassifier = true;
				}
				else  				   //Caso in cui il classificatore esiste
				{
					mytab2.ToolTip = "Attendere prego: stiamo elaborando i risultati...";
					Dialog warning = new Dialog{Title = "",Size = new Eto.Drawing.Size (750, 128),Content = new TableLayout{Rows = {new TableRow(new TableCell(new ImageView{Image = new Eto.Drawing.Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("pedopornanalyzer.Properties.Resources.warning.png"))}),new TableCell(new Label{Text = "Attendere prego: stiamo elaborando i risultati...",VerticalAlignment = VerticalAlignment.Center,TextAlignment = TextAlignment.Center,TextColor = new Eto.Drawing.Color(1,1,1,1),BackgroundColor = new Eto.Drawing.Color (0, 0,1,1),Font = Fonts.Monospace(15f,0,0)}))}}};
					warning.ShowModalAsync (); // il dialog di avviso viene mostrato in modo asincrono rispetto all'altra interfaccia
					// Il recupero di tutte le immagini dal dispositivo lo faccio fare ad un altro Task in modo da mantere la UI "viva"
					await Task.Run(() =>
						{
							DiskUtils.GetImagesFromExtension (DriveToClassify);	//le immagini vengono recuperate dal lavoro di estrazione di Davide
							fileImages = DiskUtils.fileimmagine.ToArray ();		//le immagini recuperate(il loro path in realtà) vengono inserite in un array 
						});
					warning.Close (); 						// ora che i risultati sono pronti chiudo il dialog di elaborazione dei risultati...
					mytab2.ToolTip = "";					// ...resetto a "vuoto" il tooltip della pagine
					mytab2.PauseResume.Enabled = true; 		//...abilito il pulsante Pausa/Riprendi.
					ImageAnalyzed = 0; 						//Inizialmente il numero di immagini analizzate è zero
					Positives = 0;							//Inizialmente il numero di immagini positive è zero
					Negatives = 0;							//Inizialmente il numero di immagini negative è zero
					populationSize = 0;						//Inizialmente il numero delle immagini da analizzare è zero
					populationSize = fileImages.Length;     //Setto il valore del numero delle immagini da analizzare: è pari al numero di elementi dell'array

					mytab2.progress.MaxValue = populationSize;	//Setto il valore massimo che la progress bar può assumere:è pari al numero di immagini da analizzare
					PositiveImages = new List<string>();   		//Inizializzo l'array che dovrà contenere il path delle immagini positive
					cyclebreak = true;

					AnalyzerAsync (mytab2);						//Lancio la funzione principale

				}
			}
		}

		public async void AnalyzerAsync(MyTabAnalysis tabAnalysis)   //Funzione principale che permette di elaborare i risultati riguardo le immagini recuperate.Sia caso iniziale che per Pausa/riprendi
		{

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			using (SVM svm = new SVM())
			{
				
				FileStorage fs = new FileStorage(ClassifierUsedForAnalysis,FileStorage.Mode.Read);
				svm.Read(fs.GetFirstTopLevelNode());

				for (int i = ImageAnalyzed; i < populationSize && !isStop; i++) 	// Inizio:pari al numero di immagini analizzate( 0 nel caso iniziale, !=0 in caso di pausa).Condizione di fine: ImDaAnalizzare=ImAnalizzate || PremutoPausa;
				{

					SkinDetector mySkinDetector = new SkinDetector();

					Matrix<float> sample = new Matrix<float>(1, 10);

					sample = mySkinDetector.getFeatures(fileImages[i]);

					int response = (int)svm.Predict(sample);

					ImageAnalyzed++;                                   			 	//Incremento il numero di immagini analizzte      	

					tabAnalysis.images.Text = ImageAnalyzed.ToString ();			//Stampo il numero di immagini analizzate nella form

					if (response == 1)												//Caso immagine positiva
					{
						Positives++;												//Incremento il numero di immagini positive
						MyTabAnalysis.positive.Text = Positives.ToString ();		//Stampo il numero di immagini positive nella form

						PositiveImages.Add (fileImages [i]);						//Aggiungo il path dell'immagine positiva all'arraylist PositiveImages

						MyTabAnalysis.pathPositive.Text += fileImages[i]+"\n";		//Stampo il path dell'immagine positiva nella textarea della form
					}
					else 															//Caso immagine negativa
					{
						Negatives++;												//Incremento il numero di immagini negative
						MyTabAnalysis.negative.Text = Negatives.ToString ();		//Stampo il numero di immagini negative nella form
					}
						
					await Task.Delay(125);											//Aggiungo il ritardo,utile per far in modo che la UI non freezi

					tabAnalysis.progress.Value = ImageAnalyzed;						//Aggiorno il valore attuale della progress bar: è pari al numero di immagini analizzate

					tabAnalysis.percentuage.Text = ((i * 100) / populationSize).ToString () + "%"; //Aggiorno il valore su scala 100 della progress bar

				}
				if (tabAnalysis.progress.Value == tabAnalysis.progress.MaxValue) {	//Se usciti dal ciclo for, la progress bar ha raggiunto il suo valore massimo...
					tabAnalysis.cartella.Enabled = true;							//...l'analisi è conclusa e posso analizzare un nuovo dispositivo:riabilito cartella
					tabAnalysis.PauseResume.Enabled = false;						//...Disabilitiamo il bottono PauseResume
					isStop = true;													//...l'analizzatore è in stop
					tabAnalysis.percentuage.Text = "100%";							//A causa della struttura del ciclo for, non potrei visualizzare mai il valore "100%" e quindi uscito dal ciclo e quando la progress bar è piena sono sicuro di essere arrivato al "100%"
				}
				if (populationSize == 0 && ImageAnalyzed == 0 && Positives == 0 && Negatives == 0) { //Se usciti dal ciclo for,non sono stati trovate file immagine...
					tabAnalysis.images.Text = "0";
					MyTabAnalysis.positive.Text = "0";
					MyTabAnalysis.negative.Text = "0";
				}
			}
				
			stopwatch.Stop();
			Console.WriteLine("Time taken (ms): " + stopwatch.Elapsed.TotalMilliseconds);
			Console.WriteLine("Time taken (s): " + stopwatch.Elapsed.TotalSeconds);
			Console.WriteLine("Time taken (min): " + stopwatch.Elapsed.TotalMinutes);

		}
	}
}
