using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptCrop
{
    class Program
    {
        static void Main(string[] args)
        {
            var files = Directory.GetFiles(@"C:\Users\lucas\OneDrive\Imagens\receipts");

            foreach (var path in files)
            {
                var fileName = Path.GetFileName(path);

                Bitmap bmp = new Bitmap(path);
                bmp.SetResolution(50, 50);

                Image<Gray, Byte> GrayBmp;
                Image<Bgr, Byte> orig = new Image<Bgr, Byte>(bmp);

                double ratioX = (double)500 / (double)orig.Width;
                double ratioY = (double)500 / (double)orig.Height;

                double ratio = ratioX < ratioY ? ratioX : ratioY;

                int newHeight = Convert.ToInt32(orig.Height * ratio);
                int newWidth = Convert.ToInt32(orig.Width * ratio);

                orig = orig.Resize(newWidth, newHeight, Emgu.CV.CvEnum.Inter.Area);

                Bitmap output;

                GrayBmp = orig.Convert<Gray, byte>();

                GrayBmp._SmoothGaussian(3);

                GrayBmp.Save(Path.Combine(@"C:\Users\lucas\OneDrive\Imagens\receipts\result\", DateTime.Now.ToString("dd-MM-yy-mm-hh-ss") + "__GRAY__" + fileName));

                Gray grayCannyThreshold = new Gray(75);
                Gray grayThreshLinking = new Gray(200);

                var Cannybmp = GrayBmp.Canny(grayCannyThreshold.Intensity, grayThreshLinking.Intensity);

                output = Cannybmp.ToBitmap();
                output.Save(Path.Combine(@"C:\Users\lucas\OneDrive\Imagens\receipts\result\", DateTime.Now.ToString("dd-MM-yy-mm-hh-ss") + "__CANNY__" + fileName));

                var r = Cannybmp.HoughLinesBinary(2, Math.PI / 180.0, 100, 30, 3)[0];

                var biggestLines = r.OrderByDescending(x => x.Length).Take(4).ToList();

                var edges = PointCollection.PolyLine(biggestLines.Select(x => (PointF)x.P1).ToArray(), true);

                PointF[] f = new PointF[4];




                //PointF[] srcs = new PointF[4]; //Trapezoid shape
                //srcs[0] = new PointF(1, 1);
                //srcs[1] = new PointF(300, 1);
                //srcs[2] = new PointF(400, 150);
                //srcs[3] = new PointF(100, 150);

                //PointF[] dests = new PointF[4]; // Rectangle Shape
                //dests[0] = new PointF(3, 3);
                //dests[1] = new PointF(150, 3);
                //dests[2] = new PointF(180, 200);
                //dests[3] = new PointF(150, 200);

                //CvInvoke.WarpPerspective


                //var wr = CvInvoke.GetPerspectiveTransform(srcs, dests);

                //Image<Gray, byte> transformed = Cannybmp.WarpPerspective<Mat>(
                //    mapMatrix: wr, width: 400, height: 200, interpolationType: Emgu.CV.CvEnum.Inter.Cubic,
                //warpType: Emgu.CV.CvEnum.Warp.Default, borderType: Emgu.CV.CvEnum.BorderType.Default, backgroundColor: new Gray(5));


                Cannybmp.DrawPolyline(biggestLines.Select(x => x.P1).ToArray(), true, new Gray(100));
                //output.Save(Path.Combine(@"C:\Users\lucas\OneDrive\Imagens\receipts\result\", DateTime.Now.ToString("dd-MM-yy-mm-hh-ss") + "__DRAW__" + fileName));

                Mat hierarchy = new Mat();
                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

                VectorOfPoint screenCnt;

                CvInvoke.FindContours(Cannybmp.Copy(), contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

                //VectorOfVectorOfPoint asa = new VectorOfVectorOfPoint();
                //VectorOfVectorOfPoint asfa = new VectorOfVectorOfPoint(1000);

                //CvInvoke.ConvertPointsToHomogeneous(contours, asfa);

                for (int i = 0; i < contours.Size; i++)
                {
                    var peri = CvInvoke.ArcLength(contours[i], true);
                    VectorOfPoint approxCurve = new VectorOfPoint();

                    CvInvoke.ApproxPolyDP(contours[i], approxCurve, 0.02 * peri, true);

                    if (approxCurve.Size == 4)
                    {
                        screenCnt = approxCurve;
                        break;
                    }
                }

            }
        }
    }
}