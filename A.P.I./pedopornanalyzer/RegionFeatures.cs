using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using System.Drawing;
using Emgu.CV.Util;

namespace pedopornanalyzer
{
	class RegionFeatures
	{
		public float regionArea;
		public float regionPerimeter;
		public float compactness;
		public float rectangularity;
		public float boundRectArea;
		public bool isBad;
		public VectorOfPoint hull;
		public Image<Bgr, Byte> roi;
		public float[] BGRMeansSD = new float[6];
		public Rectangle rect;

		public RegionFeatures ()
		{
			regionArea = 0;
			regionPerimeter = 0;
			compactness = 0;
			boundRectArea = 0;
			rectangularity = 0;
			isBad = false;
		}

		public RegionFeatures(float a, float b, float c, float d, float e, Image<Bgr, Byte> f, bool g, VectorOfPoint h, float[] i, Rectangle l)
		{
			regionArea = a;
			regionPerimeter = b;
			compactness = c;
			rectangularity = d;
			boundRectArea = e;
			roi = f.Clone();
			isBad = g;
			hull = h;
			BGRMeansSD = i;
			rect = l;

		}

	}
}

