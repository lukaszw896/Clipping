using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CGProject4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WriteableBitmap writeableBitmap;
        public bool isSecondClick;
        public bool drawLine = true;
        public Point firstPoint;
        public int lineThickness = 1;   
        public MainWindow()
        {
            writeableBitmap = new WriteableBitmap(800, 500, 300, 300, System.Windows.Media.PixelFormats.Bgra32, null);
            InitializeComponent();
            myImage.Source = writeableBitmap;
            drawBorder();
        }

        public void InitWrtiteableBitmap()
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(300, 300, 96, 96, System.Windows.Media.PixelFormats.Default, null);
        }


        void MidpointLine(int x1, int y1, int x2, int y2)
        {
            bool thickenX;
            int longerAxis;
            int shorterAxis;
            int dx = x2 - x1;
            int dy = y2 - y1;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (dx < 0) dx1 = -1; else if (dx > 0) dx1 = 1;
            if (dy < 0) dy1 = -1; else if (dy > 0) dy1 = 1;
            if (Math.Abs(dx) <= Math.Abs(dy))
            {
                thickenX = false;
                longerAxis = Math.Abs(dy);
                shorterAxis = Math.Abs(dx);
                if (dy < 0) dy2 = -1; else if (dy > 0) dy2 = 1;
            }
            else
            {
                thickenX = true;
                longerAxis = Math.Abs(dx);
                shorterAxis = Math.Abs(dy);
                if (dx < 0) dx2 = -1; else if (dx > 0) dx2 = 1;
            }
            int numerator = longerAxis;
            #region line thickness
            int loopStartValue = lineThickness / 2 * (-1);
            int loopEndValue;
            if (lineThickness == 1)
            {
                loopEndValue = 1;
            }
            else
                loopEndValue = lineThickness / 2;
            #endregion
            for (int i = 0; i <= longerAxis; i++)
            {
                if (thickenX)
                {
                    for (int j = loopStartValue; j < loopEndValue; j++)
                    {
                        putPixel(x1, y1 + j);
                    }
                }
                else
                {
                    for (int j = loopStartValue; j < loopEndValue; j++)
                    {
                        putPixel(x1 + j, y1);
                    }
                }
                numerator += shorterAxis;
                if (numerator >= longerAxis)
                {
                    numerator -= longerAxis;
                    x1 += dx1;
                    y1 += dy1;
                }
                else
                {
                    x1 += dx2;
                    y1 += dy2;
                }
            }
        }

        private void putPixel(int x, int y)
        {
            int column = x;
            int row = y;

            // Reserve the back buffer for updates.
            writeableBitmap.Lock();

            unsafe
            {
                // Get a pointer to the back buffer.
                int pBackBuffer = (int)writeableBitmap.BackBuffer;

                // Find the address of the pixel to draw.
                pBackBuffer += row * writeableBitmap.BackBufferStride;
                pBackBuffer += column * 4;

                // Compute the pixel's color.
                int color_data = 255 << 24; // R
                color_data |= 0 << 16;   // G
                color_data |= 0 << 8;   // B
                color_data |= 0 << 0;  
                // Assign the color data to the pixel.
                *((int*)pBackBuffer) = color_data;
            }

            // Specify the area of the bitmap that changed.
            writeableBitmap.AddDirtyRect(new Int32Rect(column, row, 1, 1));

            // Release the back buffer and make it available for display.
            writeableBitmap.Unlock();
        }

        private void myImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (drawLine == true)
            {
                Point point = e.GetPosition(myImage);
                if (isSecondClick == false)
                {
                    putMarker(point);
                    firstPoint = point;
                    isSecondClick = true;
                }
                else
                {
                    putMarker(point);
                    isSecondClick = false;
                    CohenSutherland(firstPoint, point, 100, 700, 50, 450);
                   // MidpointLine((int)firstPoint.X, (int)firstPoint.Y, (int)point.X, (int)point.Y);
                }
            }
            else
            {
                Point point = e.GetPosition(myImage);
                if (isSecondClick == false)
                {
                    putMarker(point);
                    firstPoint = point;
                    isSecondClick = true;
                }
                else
                {
                    putMarker(point);
                    isSecondClick = false;
                   // DrawCircle((int)firstPoint.X, (int)firstPoint.Y, (int)point.X, (int)point.Y);
                }
            }

        }

        private void drawBorder()
        {
            for (int i = 100; i < 700; i++)
            {
                putPixel(i, 50);
                putPixel(i, 450);
            }
            for(int j=50;j<450;j++){
                putPixel(100,j);
                putPixel(700,j);
            }
        }

        private void putMarker(Point p)
        {

            for (int i = (int)p.X - 5; i < (int)p.X + 5; i++)
            {
                for (int j = (int)p.Y - 5; j < (int)p.Y + 5; j++)
                {
                    putPixel(i, j);
                }
            }


          //  myImage.Source = writeableBitmap;
        }

        enum Outcodes
        {
            LEFT = 1,
            RIGHT = 2,
            BOTTOM = 4,
            TOP = 8
        };
        byte ComputeOutcode(Point p, int left, int right, int top, int bottom)
        {
            unsafe
            {
                byte outcode = 0;
                if (p.X > right) outcode |= 2;
                else if (p.X < left) outcode |= 1;
                if (p.Y < top) outcode |= 8;
                else if (p.Y > bottom) outcode |= 4;
                return outcode;
            }
        }

        void CohenSutherland(Point p1, Point p2, int left, int right, int top, int bottom)
        {
            bool accept = false, done = false;
            byte outcode1 = ComputeOutcode(p1, left,right,top,bottom);
            byte outcode2 = ComputeOutcode(p2, left,right,top,bottom);
            do {
                if ( ( outcode1 | outcode2 ) == 0 ) { //trivially accepted
                    accept = true;
                    done = true;
                }
                else if ( ( outcode1 & outcode2 ) != 0 ) { //trivially rejected
                    accept = false;
                    done = true;
                }
                else { //subdivide
                    byte outcodeOut = (outcode1 != 0) ? outcode1 : outcode2;
                    Point p = new Point();
                    if ( ( outcodeOut & 8 ) != 0 ) {
                        p.X = p1.X + (p2.X - p1.X)*(top - p1.Y)/(p2.Y - p1.Y);
                        p.Y = top;
                    }
                    else if ( ( outcodeOut & 4 ) != 0 ) {
                        p.X = p1.X + (p2.X - p1.X)*(bottom - p1.Y)/(p2.Y - p1.Y);
                        p.Y = bottom;
                   }
                    else if ((outcodeOut & 2) != 0)
                    {
                        p.X = right;
                        p.Y = ((p2.Y-p1.Y)/(p2.X-p1.X))*(right-p1.X)+p1.Y;
                    }
                    else if ((outcodeOut & 1) != 0)
                    {
                        p.X = left;
                        p.Y = ((p2.Y - p1.Y) / (p2.X - p1.X)) * (left - p1.X) + p1.Y;
                    }
                    if (outcodeOut == outcode1){
                    p1 = p;
                    outcode1 = ComputeOutcode(p1, left,right,top,bottom);
                    }
                    else {
                        p2 = p;
                        outcode2 = ComputeOutcode(p2, left,right,top,bottom);
                    }
                }
            } while (!done);
            if (accept)
                MidpointLine((int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y);
        }


      
    }
}
