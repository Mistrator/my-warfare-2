﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

namespace Jypeli
{
    public partial class PhysicsObject
    {
        public bool IsShattered { get; private set; }

        /// <summary>
        /// Hajottaa olion pienempiin palasiin.
        /// </summary>
        /// <param name="minSideLength">Palasen minimikoko.</param>
        public void Shatter(double minSideLength, Vector impact)
        {
            if (this.Width < minSideLength || this.Height < minSideLength) return;
            if (this.IsShattered) return;

            Angle parentAngle = this.Angle;
            this.Angle = Angle.Zero;

            Vector splitPoint = RandomGen.NextVector(this.Width / 4, this.Height / 4, 3 * (this.Width / 4), 3 * (this.Height / 4)); // 0 - width/height, sallittu alue 1/4 - 3/4
            Vector topLeftCorner = new Vector(this.Position.X - this.Width / 2, this.Position.Y + this.Height / 2);

            PhysicsObject topLeft = new PhysicsObject(splitPoint.X, splitPoint.Y);
            topLeft.Position = new Vector(topLeftCorner.X + splitPoint.X / 2, topLeftCorner.Y - splitPoint.Y / 2);
            //topLeft.X = topLeft.X * Math.Cos(parentAngle.Radians) - topLeft.Y * Math.Sin(parentAngle.Radians);
            //topLeft.Y = topLeft.X * Math.Sin(parentAngle.Radians) + topLeft.Y * Math.Cos(parentAngle.Radians);
            topLeft.Angle = parentAngle;
            topLeft.Velocity = this.Velocity;
            topLeft.Hit(RandomGen.NextVector(-this.Velocity.Magnitude / 4, this.Velocity.Magnitude / 4));

            PhysicsObject topRight = new PhysicsObject(this.Width - splitPoint.X, splitPoint.Y);
            topRight.Position = new Vector(topLeftCorner.X + (splitPoint.X + (this.Width - splitPoint.X) / 2), topLeftCorner.Y - splitPoint.Y / 2);
            //topRight.X = topRight.X * Math.Cos(parentAngle.Radians) - topRight.Y * Math.Sin(parentAngle.Radians);
            //topRight.Y = topRight.X * Math.Sin(parentAngle.Radians) + topRight.Y * Math.Cos(parentAngle.Radians);
            topRight.Angle = parentAngle;
            topRight.Velocity = this.Velocity;
            topRight.Hit(RandomGen.NextVector(-this.Velocity.Magnitude / 4, this.Velocity.Magnitude / 4));

            PhysicsObject bottomLeft = new PhysicsObject(splitPoint.X, this.Height - splitPoint.Y);
            bottomLeft.Position = new Vector(topLeftCorner.X + splitPoint.X / 2, topLeftCorner.Y - (splitPoint.Y + (this.Height - splitPoint.Y) / 2));
            //bottomLeft.X = bottomLeft.X * Math.Cos(parentAngle.Radians) - bottomLeft.Y * Math.Sin(parentAngle.Radians);
            //bottomLeft.Y = bottomLeft.X * Math.Sin(parentAngle.Radians) + bottomLeft.Y * Math.Cos(parentAngle.Radians);
            bottomLeft.Angle = parentAngle;
            bottomLeft.Velocity = this.Velocity;
            bottomLeft.Hit(RandomGen.NextVector(-this.Velocity.Magnitude / 4, this.Velocity.Magnitude / 4));

            PhysicsObject bottomRight = new PhysicsObject(this.Width - splitPoint.X, this.Height - splitPoint.Y);
            bottomRight.Position = new Vector(topLeftCorner.X + (splitPoint.X + (this.Width - splitPoint.X) / 2), topLeftCorner.Y - (splitPoint.Y + (this.Height - splitPoint.Y) / 2));
            //bottomRight.X = bottomRight.X * Math.Cos(parentAngle.Radians) - bottomRight.Y * Math.Sin(parentAngle.Radians);
            //bottomRight.Y = bottomRight.X * Math.Sin(parentAngle.Radians) + bottomRight.Y * Math.Cos(parentAngle.Radians);
            bottomRight.Angle = parentAngle;
            bottomRight.Velocity = this.Velocity;
            bottomRight.Hit(RandomGen.NextVector(-this.Velocity.Magnitude / 4, this.Velocity.Magnitude / 4));

            // paloitellaan kuva
            if (this.Image != null)
            {
                int parentImageWidth = this.Image.Width;
                int parentImageHeight = this.Image.Height;

                double suhdeX = splitPoint.X / (this.Width - splitPoint.X);
                double suhdeY = splitPoint.Y / (this.Height - splitPoint.Y);

                double imageSplitX = (suhdeX * parentImageWidth) / (suhdeX + 1);
                double imageSplitY = (suhdeY * parentImageHeight) / (suhdeY + 1);

                // jaetaan kuva osiin samassa suhteessa kuin alkuperäinen olio
                IntPoint imageSplitPoint = new IntPoint((int)Math.Round(imageSplitX), (int)Math.Round(imageSplitY));

                Image topLeftImage = new Image(imageSplitPoint.X, imageSplitPoint.Y);
                uint[,] topLeftData = this.Image.GetDataUInt(0, 0, imageSplitPoint.X, imageSplitPoint.Y);
                if (IsCompletelyTransparent(topLeftData)) // jos tekstuuri täysin läpinäkyvä, tuhotaan sirpale
                {
                    topLeft.Destroy();
                }
                else
                {
                    topLeftImage.SetData(topLeftData);
                    topLeft.Image = topLeftImage;
                    Game.Instance.Add(topLeft);
                }

                Image topRightImage = new Image(parentImageWidth - imageSplitPoint.X, imageSplitPoint.X);
                uint[,] topRightData = this.Image.GetDataUInt(imageSplitPoint.X + 1, 0, parentImageWidth - (imageSplitPoint.X + 1), imageSplitPoint.Y);
                if (IsCompletelyTransparent(topRightData))
                {
                    topRight.Destroy();
                }
                else
                {
                    topRightImage.SetData(topRightData);
                    topRight.Image = topRightImage;
                    Game.Instance.Add(topRight);
                }

                Image bottomLeftImage = new Image(imageSplitPoint.X, parentImageHeight - imageSplitPoint.Y);
                uint[,] bottomLeftData = this.Image.GetDataUInt(0, imageSplitPoint.Y + 1, imageSplitPoint.X, parentImageHeight - (imageSplitPoint.Y + 1));
                if (IsCompletelyTransparent(bottomLeftData))
                {
                    bottomLeft.Destroy();
                }
                else
                {
                    bottomLeftImage.SetData(bottomLeftData);
                    bottomLeft.Image = bottomLeftImage;
                    Game.Instance.Add(bottomLeft);
                }

                Image bottomRightImage = new Image(parentImageWidth - imageSplitPoint.X, parentImageHeight - imageSplitPoint.Y);
                uint[,] bottomRightData = this.Image.GetDataUInt(imageSplitPoint.X + 1, imageSplitPoint.Y + 1, parentImageWidth - (imageSplitPoint.X + 1), parentImageHeight - (imageSplitPoint.Y + 1));
                if (IsCompletelyTransparent(bottomRightData))
                {
                    bottomRight.Destroy();
                }
                else
                {
                    bottomRightImage.SetData(bottomRightData);
                    bottomRight.Image = bottomRightImage;
                    Game.Instance.Add(bottomRight);
                }
            }
            else
            {
                Game.Instance.Add(topLeft);
                Game.Instance.Add(topRight);
                Game.Instance.Add(bottomLeft);
                Game.Instance.Add(bottomRight);
            }

            if (!topLeft.IsDestroying) topLeft.Shatter(minSideLength, impact);
            if (!topRight.IsDestroying) topRight.Shatter(minSideLength, impact);
            if (!bottomLeft.IsDestroying) bottomLeft.Shatter(minSideLength, impact);
            if (!bottomRight.IsDestroying) bottomRight.Shatter(minSideLength, impact);

            this.IsShattered = true;
            // this.Destroy();
        }

        /// <summary>
        /// Onko ARGB-pakatussa kuvassa yhtään ei-läpinäkyvää pikseliä.
        /// </summary>
        /// <param name="ARGBImage">Tutkittava kuva.</param>
        /// <returns></returns>
        private bool IsCompletelyTransparent(uint[,] ARGBImage)
        {
            for (int i = 0; i < ARGBImage.GetLength(0); i++)
            {
                for (int j = 0; j < ARGBImage.GetLength(1); j++)
                {
                    if ((ARGBImage[i, j] >> 24) > 0) return false; // otetaan pikselin ylin tavu (alpha) ja tutkitaan, onko suurempaa kuin 0 eli täysin läpinäkyvä
                }
            }
            return true;
        }
    }
}