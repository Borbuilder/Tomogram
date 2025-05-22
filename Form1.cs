using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//namespace Tomogram
//{
//    public partial class Form1 : Form
//    {
//        private View view = new View();
//        private Bin bin;
//        private bool loaded = false;
//        private bool needReload = false;
//        private int currentLayer;
//        private OpenTK.GLControl glControl1;
//        int FrameCount;
//        DateTime NextFPSUpdate = DateTime.Now.AddSeconds(1);

//        private void Form1_Load(object sender, EventArgs e)
//        {
//            Application.Idle += Application_Idle;
//        }

//        private void displayFPS()
//        {
//            if (DateTime.Now >= NextFPSUpdate)
//            {
//                //textBox1.Text = String.Format("CT Visualizer (fps={0})", FrameCount);
//                this.Text = String.Format("CT Visualizer (fps={0})", FrameCount);
//                NextFPSUpdate = DateTime.Now.AddSeconds(1);
//                FrameCount = 0;
//            }
//            FrameCount++;
//        }

//        [Obsolete]
//        private void InitializeGLControl()
//        {
//            this.glControl1 = new OpenTK.GLControl();
//            this.glControl1.BackColor = System.Drawing.Color.Black;
//            this.glControl1.Location = new System.Drawing.Point(10, 10);
//            this.glControl1.Name = "glControl1";
//            this.glControl1.Size = new System.Drawing.Size(800, 600);
//            this.glControl1.TabIndex = 0;
//            this.glControl1.VSync = false;
//            this.glControl1.Load += GlControl1_Load;
//            this.glControl1.Paint += GlControl1_Paint;
//            this.Controls.Add(this.glControl1);
//        }

//        private void GlControl1_Load(object sender, EventArgs e)
//        {

//            GL.ClearColor(Color.Black);
//        }

//        [Obsolete]
//        private void GlControl1_Paint(object sender, PaintEventArgs e)
//        {
//            if (loaded)
//            {
//                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
//                view.DrawQuads(currentLayer);


//                glControl1.SwapBuffers();

//                if (needReload)
//                {
//                    needReload = false;
//                    GL.Finish();
//                }
//            }
//        }

//        [Obsolete]
//        public Form1()
//        {
//            InitializeComponent();
//            InitializeGLControl();
//        }

//        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            OpenFileDialog dialog = new OpenFileDialog();
//            if (dialog.ShowDialog() == DialogResult.OK)
//            {
//                string str = dialog.FileName;
//                Bin.readBIN(str);
//                view.SetupView(glControl1.Width, glControl1.Height);
//                loaded = true;
//                glControl1.Invalidate();
//            }
//        }

//        //[Obsolete]
//        //private void glControl1_Paint(object sender, PaintEventArgs e)
//        //{
//        //    if (loaded)
//        //    {
//        //        //if (needReload)
//        //        //{
//        //        //    view.generateTextureImage(currentLayer);
//        //        //    view.Load2DTexture(); // Загрузка новой текстуры после обновления Transfer Function
//        //        //    needReload = false;
//        //        //}

//        //        //switch (currentMode)
//        //        //{
//        //        //    case VisualizationMode.Quads:
//        //        //        view.DrawQuads(currentLayer); // Отрисовка четырехугольников
//        //        //        break;
//        //        //    case VisualizationMode.Texture:
//        //        //        view.DrawTexture(); // Отрисовка текстуры
//        //        //        break;
//        //        //    case VisualizationMode.QuadStrip:
//        //        //        view.DrawQuadStrip(currentLayer); // Отрисовка полосы четырехугольников
//        //        //        break;
//        //        //}
//        //        view.DrawQuads(currentLayer);
//        //        glControl1.SwapBuffers();
//        //    }
//        //}

//        private void trackBar1_Scroll(object sender, EventArgs e)
//        {
//            currentLayer = trackBar1.Value;
//            if (loaded)
//            {
//                glControl1.Invalidate();
//            }
//        }

//        public void Application_Idle(object sender, EventArgs e)
//        {
//            while (glControl1.IsIdle)
//            {
//                displayFPS();
//                glControl1.Invalidate();
//            }
//        }

//        private void textBox1_TextChanged(object sender, EventArgs e)
//        {

//        }
//    }
//}

namespace Tomogram
{
    public partial class Form1 : Form
    {
        private enum VisualizationMode
        {
            Quads,
            Texture,
            QuadStrip
        }
        private VisualizationMode mode;

        private View view = new View();
        private bool loaded = false;
        private int currentLayer;
        private bool needReload = false;
        private bool _mirrorLeftHalf = false;
        private OpenTK.GLControl glControl1;

        // Для точного подсчёта FPS
        private Stopwatch fpsTimer = Stopwatch.StartNew();
        private int frameCount = 0;
        private float currentFps = 0;

        [Obsolete]
        public Form1()
        {
            InitializeComponent();
            InitializeGLControl();
            Application.Idle += Application_Idle;
        }

        [Obsolete]
        private void InitializeGLControl()
        {
            this.glControl1 = new OpenTK.GLControl();
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(10, 10);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(800, 600);
            this.glControl1.TabIndex = 0;
            this.glControl1.VSync = false;
            this.glControl1.Load += GlControl1_Load;
            this.glControl1.Paint += GlControl1_Paint;
            this.Controls.Add(this.glControl1);
        }

        private void GlControl1_Load(object sender, EventArgs e)
        {

            GL.ClearColor(Color.Black);
        }

        [Obsolete]
        private void GlControl1_Paint(object sender, PaintEventArgs e)
        {
            if (!loaded) return;
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (needReload)
            {
                view.generateTextureImage(currentLayer, _mirrorLeftHalf);
                view.Load2DTexture(); // Загрузка новой текстуры после обновления Transfer Function
                needReload = false;
            }

            switch(mode)
            {
                case VisualizationMode.Quads:
                    view.DrawQuads(currentLayer, _mirrorLeftHalf);
                    break;
                case VisualizationMode.QuadStrip:
                    view.DrawQuadStrip(currentLayer, _mirrorLeftHalf);
                    break;
                case VisualizationMode.Texture:
                    view.DrawTexture();
                    break;
            }

            // Отрисовка сцены
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //view.DrawQuads(currentLayer);
            glControl1.SwapBuffers();

            // Подсчёт FPS
            frameCount++;

            // Обновляем FPS каждую секунду
            if (fpsTimer.Elapsed.TotalSeconds >= 1.0)
            {
                currentFps = (float)(frameCount / fpsTimer.Elapsed.TotalSeconds);
                frameCount = 0;
                fpsTimer.Restart();

                // Выводим FPS в заголовок окна
                this.Text = $"CT Visualizer | FPS: {currentFps:0.0} | Layer: {currentLayer}";
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Bin.readBIN(dialog.FileName);
                view.SetupView(glControl1.Width, glControl1.Height);
                view.generateTextureImage(0, _mirrorLeftHalf); // Инициализация текстуры для первого слоя
                view.Load2DTexture();

                loaded = true;
                glControl1.Invalidate();
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            currentLayer = trackBar1.Value;
            if (loaded)
            {
                view.generateTextureImage(currentLayer,_mirrorLeftHalf); // Пересоздаём текстуру
                view.Load2DTexture();
                glControl1.Invalidate();
            }
            //if (loaded) glControl1.Invalidate();
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            if (loaded && glControl1.IsIdle)
            {
                glControl1.Invalidate();
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                mode = VisualizationMode.Quads;
                needReload = true;
                glControl1.Invalidate();
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                mode = VisualizationMode.Texture;
                needReload = true;
                glControl1.Invalidate();
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                mode = VisualizationMode.QuadStrip;
                needReload = true;
                glControl1.Invalidate();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            view.setmintfvalue(trackBar2.Value);
            needReload = true;
            glControl1.Invalidate();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            view.settfwidth(trackBar3.Value);
        }

        private void radioButton3_CheckedChanged_1(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                mode = VisualizationMode.QuadStrip;
                needReload = true;
                glControl1.Invalidate();
            }
        }

        //private void radioButton4_CheckedChanged(object sender, EventArgs e)
        //{
        //    if(radioButton4.Checked)
        //    {
        //        _mirrorLeftHalf = true;
        //        if (loaded) glControl1.Invalidate(); 
        //    }
        //}

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //if (checkBox1.Checked)
            //{
                _mirrorLeftHalf = checkBox1.Checked;
                //needReload = true;
                if (loaded)
                {
                    view.generateTextureImage(currentLayer, _mirrorLeftHalf);
                    view.Load2DTexture();
                    glControl1.Invalidate();
                }
            
        }
    }
}
