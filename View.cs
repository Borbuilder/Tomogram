using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomogram
{
    internal class View
    {
        private  int minTransferValue = 0;
        private  int TransferWidth = 700;
        Bitmap textureImage;
        int VBOtexture;

        public void setmintfvalue(int value)
        {
            minTransferValue = value;
        }

        public void settfwidth(int width)
        {
            TransferWidth = width;
        }
        private int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(value, max));
        }
        public void SetupView(int width, int height)
        {
            GL.ShadeModel(ShadingModel.Smooth);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Bin.X, 0, Bin.Y, -1, 1);
            GL.Viewport(0, 0, width, height);
        }

        Color TransferFunction(short value)
        {
            int min = 0;
            int max = minTransferValue + TransferWidth; 
            int newVal = Clamp((value - min) * 255 / (max - min), 0, 255);
            return Color.FromArgb(255, newVal, newVal, newVal);
        }

        [Obsolete]
        public void DrawQuads(int layerNumber, bool mirrorLeftHalf)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Begin(BeginMode.Quads);
            //for (int x_coord = 0; x_coord < Bin.X - 1; x_coord++)
            //    for (int y_coord = 0; y_coord < Bin.Y - 1; y_coord++)
            //    {
            //        short value;


            //        value = Bin.array[x_coord + y_coord * Bin.X + layerNumber * Bin.X * Bin.Y];
            //        GL.Color3(TransferFunction(value));
            //        GL.Vertex2(x_coord, y_coord);


            //        value = Bin.array[x_coord + (y_coord + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
            //        GL.Color3(TransferFunction(value));
            //        GL.Vertex2(x_coord, y_coord + 1);


            //        value = Bin.array[(x_coord + 1) + (y_coord + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
            //        GL.Color3(TransferFunction(value));
            //        GL.Vertex2(x_coord + 1, y_coord + 1);


            //        value = Bin.array[(x_coord + 1) + y_coord * Bin.X + layerNumber * Bin.X * Bin.Y];
            //        GL.Color3(TransferFunction(value));
            //        GL.Vertex2(x_coord + 1, y_coord);
            //    }
            for (int x_coord = 0; x_coord < Bin.X - 1; x_coord++)
            {
                for (int y_coord = 0; y_coord < Bin.Y - 1; y_coord++)
                {
                    int sourceY = mirrorLeftHalf && x_coord < Bin.X / 2 ? Bin.Y - 1 - y_coord : y_coord;

                    short value;

                    value = Bin.array[x_coord + sourceY * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord, y_coord);

                    value = Bin.array[x_coord + (sourceY + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord, y_coord + 1);

                    value = Bin.array[(x_coord + 1) + (sourceY + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord + 1, y_coord + 1);

                    value = Bin.array[(x_coord + 1) + sourceY * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord + 1, y_coord);
                }
            }
            GL.End();
        }

        public void Load2DTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);
            BitmapData data = textureImage.LockBits(
                new System.Drawing.Rectangle(0, 0, textureImage.Width, textureImage.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, data.Scan0);

            textureImage.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);

            ErrorCode Er = GL.GetError();
            string str = Er.ToString();
        }

        public void generateTextureImage(int layerNumber, bool _mirrorLeftHalf)
        {
            textureImage = new Bitmap(Bin.X, Bin.Y);
            //for (int i = 0; i < Bin.X; ++i)
            //    for (int j = 0; j < Bin.Y; ++j)
            //    {
            //        int targetX = i;
            //        int targetY = j;

            //        if (_mirrorLeftHalf && i < Bin.X / 2)
            //        {
            //            targetY = Bin.Y - 1 - j; // Зеркалим по вертикали
            //        }

            //        int pixelNumber = targetX + targetY * Bin.X + layerNumber * Bin.X * Bin.Y;
            //        textureImage.SetPixel(i, j, TransferFunction(Bin.array[pixelNumber]));
            //    }
            for (int x = 0; x < Bin.X; x++)
            {
                for (int y = 0; y < Bin.Y; y++)
                {
                    int sourceY = _mirrorLeftHalf && x < Bin.X / 2 ? Bin.Y - 1 - y : y;
                    int pixelNumber = x + sourceY * Bin.X + layerNumber * Bin.X * Bin.Y;
                    Color color = TransferFunction(Bin.array[pixelNumber]);
                    textureImage.SetPixel(x, y, color);  // Устанавливаем в (x, y), а не (x, sourceY)
                }
            }
        }

        [Obsolete]
        public void DrawTexture()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);

            GL.Begin(BeginMode.Quads);
            GL.Color3(Color.White);
            GL.TexCoord2(0f, 0f);
            GL.Vertex2(0, 0);
            GL.TexCoord2(0f, 1f);
            GL.Vertex2(0, Bin.Y);
            GL.TexCoord2(1f, 1f);
            GL.Vertex2(Bin.X, Bin.Y);
            GL.TexCoord2(1f, 0f);
            GL.Vertex2(Bin.X, 0);

            GL.End();
            GL.Disable(EnableCap.Texture2D);

        }

        [Obsolete]
        public void DrawQuadStrip(int layerNumber, bool mirrorLeftHalf)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //GL.Begin(BeginMode.QuadStrip);
            for (int y_coord = 0; y_coord < Bin.Y - 1; y_coord++)
            {
                GL.Begin(BeginMode.QuadStrip);
                for (int x_coord = 0; x_coord < Bin.X; x_coord++)
                {
                    int sourceY1 = mirrorLeftHalf && x_coord < Bin.X / 2 ? Bin.Y - 1 - y_coord : y_coord;
                    int sourceY2 = mirrorLeftHalf && x_coord < Bin.X / 2 ? Bin.Y - 2 - y_coord : y_coord + 1;

                    short value;

                    value = Bin.array[x_coord + sourceY1 * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord, y_coord);

                    value = Bin.array[x_coord + sourceY2 * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coord, y_coord + 1);
                }
                GL.End();
            }
        }
    }
}
