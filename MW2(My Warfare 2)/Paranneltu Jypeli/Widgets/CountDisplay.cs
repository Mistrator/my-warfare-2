using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Jypeli.Widgets
{
    public class CountDisplay : BindableWidget
    {
        int fulls = 0;
        double part = 0;
        int empties = 0;

        Matrix scale = Matrix.Identity;
        Matrix rotate = Matrix.Identity;
        Matrix rotateAndScale = Matrix.Identity;

        Animation _full = null;
        Animation _empty = null;
        Vector _imgSize = Vector.Zero;
        double _spacing = 10;

        public Animation Full
        {
            get { return _full; }
            set
            {
                _full = value;
                if ( ImageSize == Vector.Zero )
                    ImageSize = value.Size;
            }
        }

        public Animation Empty
        {
            get { return _empty; }
            set
            {
                _empty = value;
                if ( ImageSize == Vector.Zero )
                    ImageSize = value.Size;
            }
        }

        public Vector ImageSize
        {
            get { return _imgSize; }
            set
            {
                _imgSize = value;
                scale = Matrix.CreateScale( (float)value.X, (float)value.Y, 1f );
                rotateAndScale = scale * rotate;
                UpdateSize();
            }
        }

        public override Angle Angle
        {
            get
            {
                return base.Angle;
            }
            set
            {
                base.Angle = value;
                rotate = Matrix.CreateRotationZ( (float)( Angle ).Radians );
                rotateAndScale = scale * rotate;
            }
        }

        public double Spacing
        {
            get { return _spacing; }
            set
            {
                _spacing = value;
                UpdateSize();
            }
        }

        public CountDisplay()
            : base( 10, 10 )
        {
            this.Color = Color.Transparent;
        }

        private void UpdateSize()
        {
            double maxval = Bound ? Meter.GetMaxValue() : 10;
            this.Width = ( ImageSize.X + Spacing ) * Meter.GetMaxValue() - Spacing;
            this.Height = ImageSize.Y;
        }

        public override void BindTo( Meter meter )
        {
            base.BindTo( meter );
            UpdateSize();
        }

        protected override void UpdateValue()
        {
            if ( Meter is IntMeter )
            {
                var meter = (IntMeter)Meter;
                fulls = meter.Value;
                part = 0;
                empties = meter.MaxValue - meter.Value;
            }
            else if ( Meter is DoubleMeter )
            {
                var meter = (DoubleMeter)Meter;
                fulls = (int)Math.Floor( meter.Value );
                part = meter.Value - fulls;
                empties = (int)( meter.MaxValue - fulls );
            }
        }

        protected override void Draw( Matrix parentTransformation, Matrix transformation )
        {
            if ( Full == null )
            {
                base.Draw( parentTransformation, transformation );
                return;
            }

            float imgdist = (float)( ImageSize.X + Spacing );
            
            Matrix m;

            // TODO: Optimization?
            UpdateValue();

            for ( int i = 0; i < fulls; i++ )
            {
                m = rotateAndScale
                    * Matrix.CreateTranslation( (float)Position.X + i * imgdist, (float)Position.Y, 0.0f )
                    * parentTransformation;

                Renderer.DrawImage( Full.CurrentFrame, ref m, Vector.Diagonal );
            }

            if ( Empty == null )
            {
                base.Draw( parentTransformation, transformation );
                return;
            }

            for ( int i = fulls; i < fulls + empties; i++ )
            {
                m = rotateAndScale
                    * Matrix.CreateTranslation( (float)Position.X + i * imgdist, (float)Position.Y, 0.0f )
                    * parentTransformation;

                Renderer.DrawImage( Empty.CurrentFrame, ref m, Vector.Diagonal );
            }

            if ( part > 0 )
            {
                double partWidth = ImageSize.X * part;

                Matrix translateAndRotate = rotate
                    * Matrix.CreateTranslation( (float)Position.X + fulls * imgdist, (float)Position.Y, 0.0f )
                    * parentTransformation;

                Matrix imp = Matrix.CreateScale( (float)partWidth, (float)ImageSize.Y, 1f )
                    * Matrix.CreateTranslation( (float)( partWidth / 2 ), 0, 0 )
                    * Matrix.CreateTranslation( (float)( -ImageSize.X / 2 ), 0, 0 )
                    * translateAndRotate;

                Matrix imf = scale * translateAndRotate;

                Renderer.BeginDrawingInsideShape( Shape.Rectangle, ref imp );
                Renderer.DrawImage( Full.CurrentFrame, ref imf, Vector.Diagonal );
                Renderer.EndDrawingInsideShape();
            }

            base.Draw( parentTransformation, transformation );
        }
    }
}
