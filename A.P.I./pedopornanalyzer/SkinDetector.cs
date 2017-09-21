using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using System.Drawing;
using Emgu.CV.Util;
using System.Timers;

namespace pedopornanalyzer
{
	class SkinDetector
	{
		public int count = 0;
		private int Y_MIN;
		private int Y_MAX;
		private int Cr_MIN;
		private int Cr_MAX;
		private int Cb_MIN;
		private int Cb_MAX;


		public SkinDetector()
		{
			Y_MIN = 0;
			Y_MAX = 255;
			Cr_MIN = 133;
			Cr_MAX = 173;
			Cb_MIN = 80;
			Cb_MAX = 120;
		}

		public Image<Bgr, Byte> getSkinMask(string path)
		{


			Bitmap bit = new Bitmap (path);
			Image<Bgr, Byte> img = new Image<Bgr, byte> (bit);
			if (img.Width > 1000 && img.Height > 1000)
			{
				img = img.Resize(1000, 1000, Emgu.CV.CvEnum.Inter.Linear);
			}
			else if (img.Width > 1000) img = img.Resize(1000, img.Height, Emgu.CV.CvEnum.Inter.Linear);
			else if (img.Height > 1000) img = img.Resize(img.Width, 1000, Emgu.CV.CvEnum.Inter.Linear);

			img._EqualizeHist();
			using (Image<Hsv, Byte> hsv = new Image<Hsv, Byte> (img.Data)) {

				CvInvoke.CvtColor (img, hsv, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
				byte[,,] data = img.Data;
				byte[,,] data2 = hsv.Data;

				int rows = img.Rows;
				int cols = img.Cols;

				for (int i = 0; i < rows; i++) {
					for (int j = 0; j < cols; j++) {
						int B = data [i, j, 0];
						int G = data [i, j, 1];
						int R = data [i, j, 2];
						int H = data2 [i, j, 0];
						int S = data2 [i, j, 1];
						int V = data2 [i, j, 2];

						if (isSkin (R, G, B, H, S, V)) {
							data [i, j, 0] = 255;
							data [i, j, 1] = 255;
							data [i, j, 2] = 255;



						} else {
							data [i, j, 0] = 0;
							data [i, j, 1] = 0;
							data [i, j, 2] = 0;
						}
					}
				}
			}






			using (Mat kernel = CvInvoke.GetStructuringElement (Emgu.CV.CvEnum.ElementShape.Cross, new Size (6, 6), new Point (-1, -1))) {

				img._MorphologyEx (MorphOp.Close, kernel, new Point (-1, -1), 3, BorderType.Default, new MCvScalar ());

			}

			return img;
		}

		public Matrix<float> getFeatures(string path)
		{
			try{
			Bitmap bit = new Bitmap (path);
			Image<Bgr, Byte> bgr2 = new Image<Bgr, byte> (bit);
			if (bgr2.Width > 1000 && bgr2.Height > 1000)
			{
				bgr2 = bgr2.Resize(1000, 1000, Emgu.CV.CvEnum.Inter.Linear);
			}
			else if (bgr2.Width > 1000) bgr2 = bgr2.Resize(1000, bgr2.Height, Emgu.CV.CvEnum.Inter.Linear);
			else if (bgr2.Height > 1000) bgr2 = bgr2.Resize(bgr2.Width, 1000, Emgu.CV.CvEnum.Inter.Linear);


			using (Image<Bgr, Byte> img = getSkinMask (path)) {


				using (Image<Gray, Byte> contour = img.Convert<Gray, Byte> ()) {
					VectorOfVectorOfPoint contoursDetected = new VectorOfVectorOfPoint ();
					CvInvoke.FindContours (contour, contoursDetected, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);





					List<RegionFeatures> regions = new List<RegionFeatures> ();




					for (int i = 0; i < contoursDetected.Size; i++) {



						VectorOfPoint hull = new VectorOfPoint ();
						CvInvoke.ConvexHull (contoursDetected [i], hull);
						Rectangle currentRect = CvInvoke.BoundingRectangle (contoursDetected [i]);
						float perimeter = (float)CvInvoke.ArcLength (contoursDetected [i], true);


						float currentRegionArea = (float)CvInvoke.ContourArea (contoursDetected [i]);
						float currentBoundRectArea = (currentRect.Height * currentRect.Width);
						if (currentRegionArea > 5000) {




							float den = Convert.ToSingle (Math.Pow (perimeter, 2) / 4 * Math.PI);
							float compactness = Convert.ToSingle (Math.Sqrt (currentRegionArea / den));
							if (float.IsNaN (compactness))
								compactness = 0;
							float rectangularity = currentRegionArea / currentBoundRectArea;
							if (float.IsNaN (rectangularity))
								rectangularity = 0;
							using (Image<Bgr, Byte> roi = img.GetSubRect (currentRect)){
								using (Image<Bgr, Byte> bgrRoi = bgr2.GetSubRect (currentRect)) {
									float[] bgrmeans = getBGRMeanSD (bgrRoi);

									RegionFeatures rf = new RegionFeatures (currentRegionArea, perimeter, compactness, rectangularity, currentBoundRectArea, roi, false, hull, bgrmeans, currentRect);
									regions.Add (rf);
								}
							}


						} else {



							float compactness = Convert.ToSingle ((4 * Math.PI * currentRegionArea) / Math.Pow (perimeter, 2));
							if (float.IsNaN (compactness))
								compactness = 0;
							float rectangularity = currentRegionArea / currentBoundRectArea;
							if (float.IsNaN (rectangularity))
								rectangularity = 0;
							using (Image<Bgr, Byte> roi = img.GetSubRect (currentRect)){
								using (Image<Bgr, Byte> bgrRoi = bgr2.GetSubRect (currentRect)){
									float[] bgrmeans = getBGRMeanSD (bgrRoi);

									RegionFeatures rf = new RegionFeatures (currentRegionArea, perimeter, compactness, rectangularity, currentBoundRectArea, roi, true, hull, bgrmeans, currentRect);
									regions.Add (rf);
								}
							}


						}


					}
					int count = 0;

					foreach (RegionFeatures rf in new ArrayList(regions)) {
						if (rf.isBad == true || checkRegionGeometry (rf.compactness, rf.rectangularity)) {
							CvInvoke.FillConvexPoly (img, rf.hull, new MCvScalar (0, 0, 0));
							count++;
							regions.Remove (rf);

						}
					}

					Matrix<float> result = new Matrix<float> (1, 10);

					if (regions.Count () != 0) {


						List<RegionFeatures> orderedlist = regions.OrderByDescending (o => o.regionArea).ToList ();
						RegionFeatures max = orderedlist.First<RegionFeatures> ();





						result [0, 1] = max.compactness;
						result [0, 2] = max.rectangularity;
						Image<Bgr, Byte> roi1 = img.GetSubRect (max.rect);
						result [0, 3] = getRegionSkinPercentage (roi1.Convert<Gray,Byte> (), img.Cols, img.Rows);
						result [0, 4] = max.BGRMeansSD [0];
						result [0, 5] = max.BGRMeansSD [1];
						result [0, 6] = max.BGRMeansSD [2];
						result [0, 7] = max.BGRMeansSD [3];
						result [0, 8] = max.BGRMeansSD [4];
						result [0, 9] = max.BGRMeansSD [5];
					} else {
						result [0, 1] = 0;
						result [0, 2] = 0;
						result [0, 3] = 0;
						result [0, 4] = 0;
						result [0, 5] = 0;
						result [0, 6] = 0;
						result [0, 7] = 0;
						result [0, 8] = 0;
						result [0, 9] = 0;
					}


					float skinPerc = getImageSkinPercentage (img.Convert<Gray,Byte> ());


					result [0, 0] = skinPerc;
					return result;
					}
				}
			}
			catch(Exception e) {
				Matrix<float> result = new Matrix<float> (1, 10);
				result [0, 0] = 0;
				result [0, 1] = 0;
				result [0, 2] = 0;
				result [0, 3] = 0;
				result [0, 4] = 0;
				result [0, 5] = 0;
				result [0, 6] = 0;
				result [0, 7] = 0;
				result [0, 8] = 0;
				result [0, 9] = 0;
				Console.WriteLine("L'immagine: "+path+" non è supportata per l'analisi.");
				return result;
			}
		}
						
		public bool isSkin(int R, int G, int B, int H, int S, int V)
		{
			bool e1 = (R > 220 && G > 210 && B > 170 && Math.Abs(R - G) > 15 && R > B && G > B) ||
				(R > 95 && G > 40 && B > 20 && Max(R, G, B) - Min(R, G, B) > 15 && Math.Abs(R - G) > 15 && R > G && R > B);

			bool e2 = ((H >= 0 && H <= 50) || (H >= 340 && H <= 360)) && S > 0.2 && V > 0.35;
			return (e1 && e2);
		}

		public int Max(int a, int b, int c)
		{

			if (a >= b && a >= c) return a;
			else if (b >= a && b >= c) return b;
			else return c;
		}

		public int Min(int a, int b, int c)
		{
			if (a <= b && a <= c) return a;
			else if (b <= a && b <= c) return b;
			else return c;
		}

		public int CountNonZero(Image<Bgr, Byte> input)
		{
			int counter = 0;
			byte[,,] data = input.Data;
			for (int i = 0; i < input.Rows; i++)
			{
				for (int j = 0; j < input.Cols; j++)
				{
					if (data[i, j, 0] == 255 && data[i, j, 1] == 255 && data[i, j, 2] == 255) counter++;
				}
			}

			return counter;
		}

		public float getImageSkinPercentage(Image<Gray, Byte> input)
		{
			float area = input.Cols * input.Rows;

			int white = CvInvoke.CountNonZero(input);
			float skinPerc = (white / area);
			if (float.IsNaN (skinPerc))
				skinPerc = 0;
			return skinPerc;
		}

		public float getRegionSkinPercentage(Image<Gray, Byte> input, float cols, float rows)
		{
			float area = cols * rows;
			int white = CvInvoke.CountNonZero(input);
			float skinPerc = (white / area);
			return skinPerc;
		}

		public bool checkRegionGeometry(float compact, float rect)
		{
			if (rect > 0.81 || compact > 0.8) return true;
			else if (rect > 0.75 && compact > 0.75) return true;
			else if (compact < 0.06) return true;
			else return false;
		}

		public float[] getBGRMeanSD(Image<Bgr, Byte> input)
		{

			float Bsum = 0;
			float Gsum = 0;
			float Rsum = 0;
			float Bsum2 = 0;
			float Gsum2 = 0;
			float Rsum2 = 0;

			int rows = input.Rows;
			int cols = input.Cols;


			Byte[,,] data = input.Data;
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < cols; j++)
				{
					float B = data[i, j, 0];
					float G = data[i, j, 1];
					float R = data[i, j, 2];
					Bsum += B;
					Gsum += G;
					Rsum += R;
					Bsum2 += B * B;
					Gsum2 += G * G;
					Rsum2 += R * R;

				}
			}

			float dimensions = input.Cols * input.Rows;

			float Bmean = Bsum / dimensions;
			if (float.IsNaN (Bmean))
				Bmean = 0;
			float Gmean = Gsum / dimensions;
			if (float.IsNaN (Gmean))
				Gmean = 0;
			float Rmean = Rsum / dimensions;
			if (float.IsNaN (Rmean))
				Rmean = 0;

			float Bdev = 0;
			float Gdev = 0;
			float Rdev = 0;

			Bdev = Convert.ToSingle(Math.Sqrt((dimensions * Bsum2) - (Bsum * Bsum))) / (dimensions - 1);
			if (float.IsNaN (Bdev))
				Bdev = 0;
			Gdev = Convert.ToSingle(Math.Sqrt((dimensions * Gsum2) - (Gsum * Gsum))) / (dimensions - 1);
			if (float.IsNaN (Gdev))
				Gdev = 0;
			Rdev = Convert.ToSingle(Math.Sqrt((dimensions * Rsum2) - (Rsum * Rsum))) / (dimensions - 1);
			Gdev = Convert.ToSingle(Math.Sqrt((dimensions * Gsum2) - (Gsum * Gsum))) / (dimensions - 1);
			if (float.IsNaN (Rdev))
				Rdev = 0;



			float[] result = new float[6];
			result[0] = Bmean / 255;
			result[1] = Gmean / 255;
			result[2] = Rmean / 255;
			result[3] = Bdev / Bmean;
			if (float.IsNaN (result [3]))
				result [3] = 0;
			result[4] = Gdev / Gmean;
			if (float.IsNaN (result [4]))
				result [4] = 0;
			result[5] = Rdev / Rmean;
			if (float.IsNaN (result [5]))
				result [5] = 0;
			return result;
		}
		
	}
}

