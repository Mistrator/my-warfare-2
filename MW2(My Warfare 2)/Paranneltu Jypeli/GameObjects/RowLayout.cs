using System;
using System.Collections.Generic;
using System.ComponentModel;
using Jypeli;

namespace Jypeli
{
    /// <summary>
    /// Asettelee widgetit riveihin. <c>TargetWidth</c> määrittää
    /// kuinka leveä yhden rivin tulisi olla. Kun yksi rivi tulee
    /// täyteen, jatketaan seuraavalle riville.
    /// </summary>
    public class RowLayout : ILayout
    {
        private Vector _preferredSize;
        private double _targetWidth = 400;
        private double _horizontalSpacing = 0;
        private double _verticalSpacing = 0;
        private double _topPadding = 0;
        private double _bottomPadding = 0;
        private double _leftPadding = 0;
        private double _rightPadding = 0;

        [EditorBrowsable( EditorBrowsableState.Never )]
        public GameObject Parent { get; set; }

        /// <summary>
        /// Olioiden väliin vaakasuunnassa jäävä tyhjä tila.
        /// </summary>
        public double HorizontalSpacing
        {
            get { return _horizontalSpacing; }
            set { _horizontalSpacing = value; NotifyParent(); }
        }

        public double VerticalSpacing
        {
            get { return _verticalSpacing; }
            set { _verticalSpacing = value; NotifyParent(); }
        }

        /// <summary>
        /// Yläreunaan jäävä tyhjä tila.
        /// </summary>
        public double TopPadding
        {
            get { return _topPadding; }
            set { _topPadding = value; NotifyParent(); }
        }

        /// <summary>
        /// Alareunaan jäävä tyhjä tila.
        /// </summary>
        public double BottomPadding
        {
            get { return _bottomPadding; }
            set { _bottomPadding = value; NotifyParent(); }
        }

        /// <summary>
        /// Vasempaan reunaan jäävä tyhjä tila.
        /// </summary>
        public double LeftPadding
        {
            get { return _leftPadding; }
            set { _leftPadding = value; NotifyParent(); }
        }

        /// <summary>
        /// Oikeaan reunaan jäävä tyhjä tila.
        /// </summary>
        public double RightPadding
        {
            get { return _rightPadding; }
            set { _rightPadding = value; NotifyParent(); }
        }

        /// <summary>
        /// Kuinka leveitä rivien tulisi korkeintaan olla.
        /// </summary>
        public double TargetWidth
        {
            get { return _targetWidth; }
            set { _targetWidth = value; NotifyParent(); }
        }


        private void NotifyParent()
        {
            if (Parent != null)
            {
                Parent.NotifyParentAboutChangedSizingAttributes();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void UpdateSizeHints( IList<GameObject> objects )
        {
            if ( objects.Count == 0 )
                return;

            double targetContentWidth = TargetWidth - ( LeftPadding + RightPadding );
            int i = 0;
            int objectsInRow = 0;
            double left = 0;
            double totalHeight = 0;
            double rowMaxHeight = 0;
            double maxRowWidth = 0;
            bool atFirstColumn = true;

            while ( i < objects.Count )
            {
                var o = objects[i];
                bool rowIsTooWide = ( left + HorizontalSpacing + o.PreferredSize.X ) > targetContentWidth;

                if ( ( objectsInRow >= 1 ) && rowIsTooWide )
                {
                    if ( left > maxRowWidth )
                        maxRowWidth = left;

                    totalHeight += rowMaxHeight + VerticalSpacing;

                    left = 0;
                    objectsInRow = 0;
                    rowMaxHeight = 0;
                    atFirstColumn = true;
                    continue;
                }

                if ( o.PreferredSize.Y > rowMaxHeight ) rowMaxHeight = o.PreferredSize.Y;

                if ( !atFirstColumn ) left += HorizontalSpacing;
                atFirstColumn = false;
                left += o.PreferredSize.X;
                objectsInRow++;
                i++;
            }

            if ( left > maxRowWidth ) maxRowWidth = left;
            totalHeight += rowMaxHeight;

            double preferredWidth = LeftPadding + maxRowWidth + RightPadding;
            double preferredHeight = TopPadding + totalHeight + BottomPadding;

            _preferredSize = new Vector( preferredWidth, preferredHeight );
        }

        [EditorBrowsable( EditorBrowsableState.Never )]
        public Sizing HorizontalSizing
        {
            get { return Sizing.FixedSize; }
        }

        [EditorBrowsable( EditorBrowsableState.Never )]
        public Sizing VerticalSizing
        {
            get { return Sizing.FixedSize; }
        }

        [EditorBrowsable( EditorBrowsableState.Never )]
        public Vector PreferredSize
        {
            get { return _preferredSize; }
        }


        [EditorBrowsable( EditorBrowsableState.Never )]
        public void Update( IList<GameObject> objects, Vector maximumSize )
        {
            double rightLimit = maximumSize.X / 2 - RightPadding;
            double leftLimit = -maximumSize.X / 2 + LeftPadding;

            int objectsInRow = 0;
            double rowMaxHeight = 0;
            double top = maximumSize.Y / 2 - TopPadding;
            double left = leftLimit;
            bool atFirstColumn = true;
            int i = 0;

            while ( i < objects.Count )
            {
                var o = objects[i];
                bool rowIsTooWide = ( left + HorizontalSpacing + o.PreferredSize.X ) > rightLimit;

                if ( ( objectsInRow >= 1 ) && rowIsTooWide )
                {
                    left = leftLimit;
                    top -= rowMaxHeight + VerticalSpacing;
                    objectsInRow = 0;
                    rowMaxHeight = 0;
                    atFirstColumn = true;
                    continue;
                }

                if ( !atFirstColumn ) left += HorizontalSpacing;
                atFirstColumn = false;

                o.Size = o.PreferredSize;
                o.X = left + o.Width / 2;
                o.Y = top - o.Height / 2;

                if ( o.PreferredSize.Y > rowMaxHeight ) rowMaxHeight = o.PreferredSize.Y;

                left += o.Width;
                objectsInRow++;
                i++;
            }
        }
    }
}
