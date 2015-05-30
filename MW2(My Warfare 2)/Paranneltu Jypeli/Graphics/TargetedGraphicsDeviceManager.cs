using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Jypeli
{
#if WINDOWS
    // see: http://www.carbonatethis.com/archives/121
    internal class TargetedGraphicsDeviceManager : GraphicsDeviceManager
    {
        private int target = 1;

        public TargetedGraphicsDeviceManager( Game game, int displayTarget )
            : base( game )
        {
            target = displayTarget;
        }

        protected override void OnPreparingDeviceSettings( object sender, PreparingDeviceSettingsEventArgs args )
        {
            //  args.GraphicsDeviceInformation.PresentationParameters.FullScreenRefreshRateInHz = 0;
            args.GraphicsDeviceInformation.PresentationParameters.IsFullScreen = false;

            base.OnPreparingDeviceSettings( sender, args );
        }

        protected override void RankDevices( List<GraphicsDeviceInformation> foundDevices )
        {
            List<GraphicsDeviceInformation> removals = new List<GraphicsDeviceInformation>();
            for ( int i = 0; i < foundDevices.Count; i++ )
            {
                if ( !foundDevices[i].Adapter.DeviceName.Contains( "DISPLAY" + target ) )
                    removals.Add( foundDevices[i] );
            }
            foreach ( GraphicsDeviceInformation info in removals )
            {
                foundDevices.Remove( info );
            }

            base.RankDevices( foundDevices );
        }
    }
#endif
}
